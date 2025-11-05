namespace API.DTOs
{
    public class ScanRequest
    {
        public string ScanData { get; set; } = string.Empty;
    }

    public class ReceivingRequest
    {
        public string ScanData { get; set; } = string.Empty;  
        public DateTime ExpiryDate { get; set; }
    }

    public class DispensingRequest
    {
        public string ScanData { get; set; } = string.Empty;  
        public int MedkitId { get; set; }
        public int TransferAmount { get; set; }
    }

    // Остальные DTO без изменений
    public class MedkitRequest
    {
        public int MedkitId { get; set; }
    }
}