namespace NashDom.Models
{
    public class ExcelImportDto
    {
        public int ApartmentNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public double TotalArea { get; set; }
        
        // Показания счетчиков
        public double? HotWaterPrevious { get; set; }
        public double? HotWaterCurrent { get; set; }
        public double? ColdWaterPrevious { get; set; }
        public double? ColdWaterCurrent { get; set; }
        
        // Сальдо и начисления
        public decimal? BalanceStart { get; set; }  // С-до на 01.04.2026
        public decimal? CreditTurnover { get; set; }  // Обороты по К-ту (если есть)
        public decimal? Accrued { get; set; }  // Начисл.
        public decimal? Paid { get; set; }  // Оплачено в т.ч.
        public DateTime? PaymentDate { get; set; }  // дата оплаты
        public decimal? DebtWithoutPenalty { get; set; }  // Д-т без пеней на 01.04.2026
        public decimal? Penalty { get; set; }  // пеня
        
        // Детализация начислений
        public decimal? Maintenance { get; set; }  // Содержание Тех. обсл.
        public decimal? Sewage { get; set; }  // Канализация
        public decimal? Elevator { get; set; }  // Лифт
        public decimal? HotWater { get; set; }  // ГВС
        public decimal? HotWaterSoi { get; set; }  // СОИ ГВС
        public decimal? ColdWater { get; set; }  // ХВС
        public decimal? ColdWaterSoi { get; set; }  // СОИ ХВС
        public decimal? ElectricitySoi { get; set; }  // СОИ элект. энер
        public bool IsPaid => Paid.HasValue && Paid.Value >= (Accrued ?? 0);
    }
}