
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashDom.Models
{
    public class OrganizationSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string TemplateName { get; set; } = "Шаблон 10.1";

        [Required]
        public string OrganizationName { get; set; } = "ТСЖ \"Наш Дом\"";

        [Required]
        public string Inn { get; set; } = "0258950110";

        [Required]
        public string Kpp { get; set; } = "026801001";

        [Required]
        public string Bik { get; set; } = "048073601";

        [Required]
        public string CorrespondentAccount { get; set; } = "30101810300000000601";

        [Required]
        public string BankName { get; set; } = "БАШКИРСКОЕ ОТДЕЛЕНИЕ N8598 ПАО СБЕРБАНК";

        [Required]
        public string SettlementAccount { get; set; } = "40703810506000000661";

        [Required]
        public string ServiceCode { get; set; } = "001";

        public string? HeaderText { get; set; }
        
        public string? FooterText { get; set; }
    }
}