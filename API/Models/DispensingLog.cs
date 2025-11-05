namespace API.Models;

public class DispensingLog
{
    public int Id { get; set; }
    public int BoxId { get; set; }
    public int MedkitId { get; set; }
    public int DispensingAmount { get; set; }
    public DateTime Date { get; set; }
    
    public virtual Box Box { get; set; } = null!;
    public virtual Medkit Medkit { get; set; } = null!;
}