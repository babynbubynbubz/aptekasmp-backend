namespace API.DTOs
{
    public class ScanRequest
    {
        public string ScanData { get; set; } = string.Empty;
    }

    public class ReceivingRequest
    {
        public string ScanData { get; set; } = string.Empty;
        public string ExpiryDate { get; set; }
    }

    public class DispensingRequest
    {
        public string ScanData { get; set; } = string.Empty;  
        public int MedkitId { get; set; }
        public int TransferAmount { get; set; }
    }

    public class MedkitRequest
    {
        public int MedkitId { get; set; }
    }
	
	public class AddMedkitRequest
    {
        public int Id { get; set; }
        public int CrewId { get; set; }
    }
}