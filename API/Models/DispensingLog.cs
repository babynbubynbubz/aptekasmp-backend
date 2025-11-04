namespace API.Models;

public class DispensingLog
{
    public int Id { get; set; }
    public int BoxId { get; set; }
    public int MedkitId { get; set; }
    public int DispensingAmount { get; set; }
    public DateTime Date { get; set; }
    public Box Box { get; set; }
    public Medkit Medkit { get; set; }
}