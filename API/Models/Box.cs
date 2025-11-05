namespace API.Models
{
    public class Box
    {
        public int Id { get; set; }
        public required string GId { get; set; }        
        public required string SerialNumber { get; set; } 
        public int InBoxRemaining { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        public ICollection<DispensingLog> DispensingLogs { get; set; } = new List<DispensingLog>();
        public ICollection<ReceivingLog> ReceivingLogs { get; set; } = new List<ReceivingLog>();
    }
}