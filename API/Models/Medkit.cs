namespace API.Models;

public class Medkit
{
    public int Id { get; set; }
    public int CrewId { get; set; }
    
    public virtual ICollection<DispensingLog> DispensingLogs { get; set; } = new List<DispensingLog>();
}