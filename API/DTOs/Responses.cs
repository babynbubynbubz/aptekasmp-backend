using System;
using System.Text.Json.Serialization;

namespace API.DTOs
{
    public class DrugResponse
    {
        [JsonPropertyName("trade_name")]
        public string TradeName { get; set; } = string.Empty;

        [JsonPropertyName("inn")]
        public string INN { get; set; } = string.Empty;

        [JsonPropertyName("barcode")]
        public long Barcode { get; set; }

        [JsonPropertyName("package_quantity")]
        public int PackageQuantity { get; set; }
    }

    public class MedicationInfoResponse
    {
        public MedicationInfo Info { get; set; } = new();
        public StorageInfo StorageInfo { get; set; } = new();
    }

    public class MedicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string INN { get; set; } = string.Empty;
        public int InBoxAmount { get; set; }
        public string GID { get; set; } = string.Empty;
        public string SN { get; set; } = string.Empty;
    }

    public class StorageInfo
    {
        public int InBoxRemaining { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class CrewResponse
    {
        public int CrewId { get; set; }
    }

    public class OperationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DispensingLogResponse
    {
        public int BoxId { get; set; }
        public int MedkitId { get; set; }
        public int TransferAmount { get; set; }
        public DateTime TransferDate { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string GID { get; set; } = string.Empty;
        public string SN { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }

    public class ReceivingLogResponse
    {
        public int BoxId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string GID { get; set; } = string.Empty;
        public string SN { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
}
