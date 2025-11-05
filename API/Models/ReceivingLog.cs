namespace API.Models;

public class ReceivingLog
{
    public int Id { get; set; }
    public int BoxId { get; set; }
    public DateTime Date { get; set; }
    
    public virtual Box Box { get; set; } = null!;
}