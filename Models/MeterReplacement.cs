using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashDom.Models
{
    public class MeterReplacement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ГВС (горячая вода)
        public string HotWaterMeterNumber { get; set; } = string.Empty;
        public DateTime? HotWaterMeterDate { get; set; }      // ← DateTime, не DateTimeOffset
        public int? HotWaterVerificationYear { get; set; }
        public string HotWaterNote { get; set; } = string.Empty;

        // ХВС (холодная вода)
        public string ColdWaterMeterNumber { get; set; } = string.Empty;
        public DateTime? ColdWaterMeterDate { get; set; }      // ← DateTime, не DateTimeOffset
        public int? ColdWaterVerificationYear { get; set; }
        public string ColdWaterNote { get; set; } = string.Empty;

        // Связь с квартирой
        public int ApartmentId { get; set; }
        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }
    }
}