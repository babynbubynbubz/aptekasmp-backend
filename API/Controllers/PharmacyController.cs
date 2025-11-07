using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.DTOs;
using API.Data;
using System.Text.Json;
using Npgsql;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PharmacyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PharmacyController> _logger;

        public PharmacyController(ApplicationDbContext context, ILogger<PharmacyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<DrugResponse?> GetDrugFromReference(string gid)
        {
            try
            {
                if (long.TryParse(gid, out long barcode))
                {
                    await using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
                    await connection.OpenAsync();

                    const string sql = @"
                        SELECT trade_name, inn, barcode, package_quantity 
                        FROM drugs 
                        WHERE barcode = @barcode";

                    await using var command = new NpgsqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@barcode", barcode);

                    await using var reader = await command.ExecuteReaderAsync();
            
                    if (await reader.ReadAsync())
                    {
                        return new DrugResponse
                        {
                            TradeName = reader.GetString(0),
                            INN = reader.GetString(1),
                            Barcode = reader.GetInt64(2),
                            PackageQuantity = reader.GetDouble(3)
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось получить данные из базы для GID: {GID}", gid);
                return null;
            }
        }

        [HttpPost("add-medkit")]
        public async Task<ActionResult<OperationResponse>> AddMedkit([FromBody] AddMedkitRequest request)
        {
            try
            {
                var existingMedkit = await _context.Medkits
                    .FirstOrDefaultAsync(m => m.Id == request.Id);

                if (existingMedkit != null)
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = $"Медкит с ID {request.Id} уже существует"
                    });
                }

                var medkit = new Medkit
                {
                    Id = request.Id,
                    CrewId = request.CrewId
                };

                _context.Medkits.Add(medkit);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Добавлен новый медкит: ID {MedkitId}, Crew ID {CrewId}", request.Id, request.CrewId);

                return Ok(new OperationResponse
                {
                    Success = true,
                    Message = "Медкит успешно добавлен"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении медкита");
                return StatusCode(500, new OperationResponse
                {
                    Success = false,
                    Message = "Внутренняя ошибка сервера"
                });
            }
        }

        [HttpPost("medication-info")]
        public async Task<ActionResult<MedicationInfoResponse>> GetMedicationInfo([FromBody] ScanRequest request)
        {
            try
            {
                var (gid, sn) = ParseScanData(request.ScanData);

                if (string.IsNullOrEmpty(gid))
                {
                    return BadRequest(new { message = "Неверный формат данных сканирования" });
                }

                var box = await _context.Boxes
                    .FirstOrDefaultAsync(b => b.GId == gid && (string.IsNullOrEmpty(sn) || b.SerialNumber == sn));

                var drugInfo = await GetDrugFromReference(gid);

                if (box == null)
                {
                    var responseWO = new MedicationInfoResponseWOStorage
                    {
                        Info = new MedicationInfo
                        {
                            Name = drugInfo?.TradeName ?? "Неизвестный препарат",
                            INN = drugInfo?.INN ?? "Неизвестное МНН",
                            InBoxAmount = drugInfo?.PackageQuantity ?? 100,
                            GID = gid,
                            SN = sn
                        }
                    };
                    return Ok(responseWO);
                }
                else
                {
                    var response = new MedicationInfoResponse
                    {
                        Info = new MedicationInfo
                        {
                            Name = drugInfo?.TradeName ?? "Неизвестный препарат",
                            INN = drugInfo?.INN ?? "Неизвестное МНН",
                            InBoxAmount = drugInfo?.PackageQuantity ?? 100,
                            GID = box.GId,
                            SN = box.SerialNumber
                        },
                        StorageInfo = new StorageInfo
                        {
                            InBoxRemaining = box.InBoxRemaining,
                            ExpiryDate = box.ExpiryDate
                        }
                    };
                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о препарате");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("receive")]
        public async Task<ActionResult<OperationResponse>> ReceiveBox([FromBody] ReceivingRequest request)
        {
            try
            {
                var (gid, sn) = ParseScanData(request.ScanData);

                if (string.IsNullOrEmpty(gid))
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = "Неверный формат данных сканирования"
                    });
                }
                
                if (!DateTime.TryParse(request.ExpiryDate, out DateTime expiryDate))
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = "Неверный формат даты. Используйте формат YYYY-MM-DD"
                    });
                }

                var existingBox = await _context.Boxes
                    .FirstOrDefaultAsync(b => b.GId == gid && b.SerialNumber == sn);

                if (existingBox != null)
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = "Коробка с таким GID и SN уже существует"
                    });
                }

                var drugInfo = await GetDrugFromReference(gid);

                var box = new Box
                {
                    GId = gid,
                    SerialNumber = sn,
                    InBoxRemaining = drugInfo?.PackageQuantity ?? 100,
                    ExpiryDate = DateTime.SpecifyKind(expiryDate.Date, DateTimeKind.Utc)
                };

                _context.Boxes.Add(box);
                await _context.SaveChangesAsync();

                var receivingLog = new ReceivingLog
                {
                    BoxId = box.Id,
                    Date = DateTime.UtcNow
                };

                _context.ReceivingLogs.Add(receivingLog);
                await _context.SaveChangesAsync();

                return Ok(new OperationResponse
                {
                    Success = true,
                    Message = "Коробка успешно принята на склад"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при приемке коробки");
                return StatusCode(500, new OperationResponse
                {
                    Success = false,
                    Message = "Внутренняя ошибка сервера"
                });
            }
        }

        [HttpPost("dispense")]
        public async Task<ActionResult<OperationResponse>> DispenseMedication([FromBody] DispensingRequest request)
        {
            try
            {
                var (gid, sn) = ParseScanData(request.ScanData);

                if (string.IsNullOrEmpty(gid))
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = "Неверный формат данных сканирования"
                    });
                }

                var box = await _context.Boxes
                    .FirstOrDefaultAsync(b => b.GId == gid && b.SerialNumber == sn);

                if (box == null)
                {
                    return NotFound(new OperationResponse
                    {
                        Success = false,
                        Message = "Коробка не найдена"
                    });
                }

                if (box.InBoxRemaining < request.TransferAmount)
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = $"Недостаточно препарата. Доступно: {box.InBoxRemaining}"
                    });
                }

                if (box.ExpiryDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new OperationResponse
                    {
                        Success = false,
                        Message = "Срок годности препарата истек"
                    });
                }

                box.InBoxRemaining -= request.TransferAmount;

                var dispensingLog = new DispensingLog
                {
                    BoxId = box.Id,
                    MedkitId = request.MedkitId,
                    DispensingAmount = request.TransferAmount,
                    Date = DateTime.UtcNow
                };

                _context.DispensingLogs.Add(dispensingLog);
                await _context.SaveChangesAsync();

                return Ok(new OperationResponse
                {
                    Success = true,
                    Message = "Препарат успешно выдан"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выдаче препарата");
                return StatusCode(500, new OperationResponse
                {
                    Success = false,
                    Message = "Внутренняя ошибка сервера"
                });
            }
        }

        [HttpPost("crew-info")]
        public async Task<ActionResult<CrewResponse>> GetCrewInfo([FromBody] MedkitRequest request)
        {
            try
            {
                var medkit = await _context.Medkits
                    .FirstOrDefaultAsync(m => m.Id == request.MedkitId);

                if (medkit == null)
                {
                    return NotFound(new { message = "Аптечка не найдена" });
                }

                return Ok(new CrewResponse { CrewId = medkit.CrewId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о бригаде");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("dispensing-logs")]
        public async Task<ActionResult<List<DispensingLogResponse>>> GetDispensingLogs()
        {
            try
            {
                var logs = await _context.DispensingLogs
                    .Include(dl => dl.Box)
                    .Include(dl => dl.Medkit)
                    .OrderByDescending(dl => dl.Date)
                    .ToListAsync();

                var logResponses = new List<DispensingLogResponse>();

                foreach (var log in logs)
                {
                    var drugInfo = await GetDrugFromReference(log.Box.GId);

                    logResponses.Add(new DispensingLogResponse
                    {
                        BoxId = log.BoxId,
                        MedkitId = log.MedkitId,
                        TransferAmount = log.DispensingAmount,
                        TransferDate = log.Date,
                        MedicationName = drugInfo?.TradeName ?? "Неизвестный препарат",
                        GID = log.Box.GId,
                        SN = log.Box.SerialNumber,
                        ExpiryDate = log.Box.ExpiryDate
                    });
                }

                return Ok(logResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении логов выдачи");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("receiving-logs")]
        public async Task<ActionResult<List<ReceivingLogResponse>>> GetReceivingLogs()
        {
            try
            {
                var logs = await _context.ReceivingLogs
                    .Include(rl => rl.Box)
                    .OrderByDescending(rl => rl.Date)
                    .ToListAsync();

                var logResponses = new List<ReceivingLogResponse>();

                foreach (var log in logs)
                {
                    var drugInfo = await GetDrugFromReference(log.Box.GId);

                    logResponses.Add(new ReceivingLogResponse
                    {
                        BoxId = log.BoxId,
                        ReceiveDate = log.Date,
                        MedicationName = drugInfo?.TradeName ?? "Неизвестный препарат",
                        GID = log.Box.GId,
                        SN = log.Box.SerialNumber,
                        ExpiryDate = log.Box.ExpiryDate
                    });
                }

                return Ok(logResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении логов поступления");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        private (string gid, string sn) ParseScanData(string scanData)
        {
            try
            {
                _logger.LogInformation($"Raw scan data: {scanData}");

                if (scanData.Length >= 31)
                {
                    string gid = scanData.Substring(4, 13);
                    string sn = scanData.Substring(19, 13);

                    _logger.LogInformation($"Parsed - GID: {gid}, SN: {sn}");
                    return (gid, sn);
                }

                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка парсинга scanData: {ScanData}", scanData);
                return (string.Empty, string.Empty);
            }
        }
    }
}
