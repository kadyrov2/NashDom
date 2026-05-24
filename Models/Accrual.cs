using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashDom.Models
{
    public class Accrual
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PersonalAccountId { get; set; }

        [ForeignKey("PersonalAccountId")]
        public virtual PersonalAccount? PersonalAccount { get; set; }

        [Required]
        public DateTime AccrualDate { get; set; } = DateTime.Today;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string ServiceCode { get; set; } = "001";

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }
    }
}