namespace NashDom.Models
{
    public class AccrualReportDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime AccrualDate { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public int ApartmentNumber { get; set; }
        public int Entrance { get; set; }
        public int Floor { get; set; }

    }
}