using NashDom.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class MeterReading
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;  // ← DateTime
    public double ColdWater { get; set; }
    public double HotWater { get; set; }

    public int ApartmentId { get; set; }
    [ForeignKey("ApartmentId")]
    public virtual Apartment? Apartment { get; set; }
}