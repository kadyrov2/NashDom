using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashDom.Models
{
    public class PersonalAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string AccountNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;

        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }
    }
}