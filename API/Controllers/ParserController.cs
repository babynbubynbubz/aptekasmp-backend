using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Threading.Tasks;
using API.DTOs;


namespace API.Controllers
{

    [ApiController]
    [Route("api/drugs")]
    public class ParserController : ControllerBase
    {
        private readonly string _connectionString;

        public ParserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new ArgumentNullException("Connection string not found");
        }
        
        [HttpGet("barcode/{barcode}")]
        public async Task<ActionResult<DrugResponse>> GetDrugByBarcode(long barcode)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
                    SELECT trade_name, inn, barcode, package_quantity
                    FROM drugs
                    WHERE barcode = @barcode";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@barcode", barcode);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var drug = new DrugResponse
                    {
                        TradeName = reader.GetString(0),
                        INN = reader.GetString(1),
                        Barcode = reader.GetInt64(2),
                        PackageQuantity = reader.GetDouble(3)
                    };

                    return Ok(drug);
                }

                return NotFound(new { detail = "Препарат не найден" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return StatusCode(500, new { detail = "Внутренняя ошибка сервера" });
            }
        }
        
        [HttpGet("health")]
        public ActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", service = "drug-reference" });
        }
    }
}
