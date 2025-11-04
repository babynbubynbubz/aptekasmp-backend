namespace API.Models;

public class Box
{
    public int Id { get; set; }
    public string GId { get; set; }
    public string SerialNumber { get; set; }
    public int InBoxRemaining { get; set; }
    public DateTime ExpiryDate { get; set; }
}