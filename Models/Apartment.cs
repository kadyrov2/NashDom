using NashDom.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Apartment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ApartmentNumber { get; set; }
    public int Entrance { get; set; }
    public int Floor { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Car { get; set; } = string.Empty;
    public double TotalArea { get; set; }   

    public virtual ICollection<PersonalAccount> PersonalAccounts { get; set; } = new List<PersonalAccount>();
    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
    public virtual RegistrationCard? RegistrationCard { get; set; }
    public virtual ICollection<MeterReplacement> MeterReplacements { get; set; } = new List<MeterReplacement>();
}