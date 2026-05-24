using OfficeOpenXml;
using NashDom.Models;
using System.Globalization;
using System.IO;

namespace NashDom.Services
{
    public class ExcelImportService
    {
        private readonly DatabaseService _dbService;

        public ExcelImportService(DatabaseService dbService)
        {
            _dbService = dbService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<List<ExcelImportDto>> ReadApril2026SheetAsync(string filePath)
        {
            var result = new List<ExcelImportDto>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets["Апрель 2026"];

            if (worksheet == null)
                throw new Exception("Лист 'Апрель 2026' не найден");

            // Начинаем с 3 строки (после заголовков)
            int row = 3;
            while (true)
            {
                var apartmentNumberCell = worksheet.Cells[row, 1]?.Text; // Колонка A - № квартиры
                if (string.IsNullOrWhiteSpace(apartmentNumberCell) || apartmentNumberCell == "ИТОГО")
                    break;

                if (!int.TryParse(apartmentNumberCell?.ToString(), out int apartmentNumber))
                {
                    row++;
                    continue;
                }

                var dto = new ExcelImportDto
                {
                    ApartmentNumber = apartmentNumber,
                    FullName = worksheet.Cells[row, 2]?.Text ?? "", // Колонка B - Ф.И.О.
                    TotalArea = ParseDouble(worksheet.Cells[row, 3]?.Text), // Колонка C - Общая площадь
                    
                    // Показания ГВС
                    HotWaterPrevious = ParseDouble(worksheet.Cells[row, 4]?.Text), // D - преды-дущее
                    HotWaterCurrent = ParseDouble(worksheet.Cells[row, 5]?.Text), // E - конеч-ное
                    
                    // Показания ХВС
                    ColdWaterPrevious = ParseDouble(worksheet.Cells[row, 7]?.Text), // G - преды-дущее
                    ColdWaterCurrent = ParseDouble(worksheet.Cells[row, 8]?.Text), // H - конеч-ное
                    
                    // Сальдо и начисления
                    BalanceStart = ParseDecimal(worksheet.Cells[row, 10]?.Text), // J - С-до на 01.04.2026 (Д-т)
                    CreditTurnover = ParseDecimal(worksheet.Cells[row, 11]?.Text), // K - Обороты по К-ту
                    
                    // Детализация начислений
                    Maintenance = ParseDecimal(worksheet.Cells[row, 12]?.Text), // L - Содержание
                    Sewage = ParseDecimal(worksheet.Cells[row, 13]?.Text), // M - Канализация
                    Elevator = ParseDecimal(worksheet.Cells[row, 14]?.Text), // N - Лифт
                    HotWater = ParseDecimal(worksheet.Cells[row, 15]?.Text), // O - ГВС
                    HotWaterSoi = ParseDecimal(worksheet.Cells[row, 16]?.Text), // P - СОИ ГВС
                    ColdWater = ParseDecimal(worksheet.Cells[row, 17]?.Text), // Q - ХВС
                    ColdWaterSoi = ParseDecimal(worksheet.Cells[row, 18]?.Text), // R - СОИ ХВС
                    ElectricitySoi = ParseDecimal(worksheet.Cells[row, 19]?.Text), // S - СОИ элект. энер
                    
                    Accrued = ParseDecimal(worksheet.Cells[row, 20]?.Text), // T - Начисл.
                    Paid = ParseDecimal(worksheet.Cells[row, 21]?.Text), // U - Оплачено
                    DebtWithoutPenalty = ParseDecimal(worksheet.Cells[row, 23]?.Text), // W - Д-т без пеней
                    Penalty = ParseDecimal(worksheet.Cells[row, 26]?.Text), // Z - пеня
                };

                // Парсим дату оплаты (колонка V)
                var paymentDateStr = worksheet.Cells[row, 22]?.Text;
                if (!string.IsNullOrWhiteSpace(paymentDateStr))
                {
                    dto.PaymentDate = ParsePaymentDate(paymentDateStr);
                }

                result.Add(dto);
                row++;
            }

            return result;
        }

        public async Task<List<AccrualChargeDto>> ReadAccrualSheetAsync(string filePath, string sheetName)
        {
            var result = new List<AccrualChargeDto>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            if (worksheet == null)
                throw new Exception($"Лист '{sheetName}' не найден. Доступные листы: {string.Join(", ", package.Workbook.Worksheets.Select(w => w.Name))}");

            int row = 3;
            while (true)
            {
                var apartmentNumberCell = worksheet.Cells[row, 1]?.Text;
                if (string.IsNullOrWhiteSpace(apartmentNumberCell) || apartmentNumberCell == "ИТОГО")
                    break;

                if (!int.TryParse(apartmentNumberCell?.ToString(), out int apartmentNumber))
                {
                    row++;
                    continue;
                }

                var dto = new AccrualChargeDto
                {
                    ApartmentNumber = apartmentNumber,
                    FullName = worksheet.Cells[row, 2]?.Text ?? "",
                    TotalArea = ParseDouble(worksheet.Cells[row, 3]?.Text),

                    // Показания ГВС
                    HotWaterPrevious = ParseDouble(worksheet.Cells[row, 4]?.Text),
                    HotWaterCurrent = ParseDouble(worksheet.Cells[row, 5]?.Text),
                    HotWaterConsumption = ParseDouble(worksheet.Cells[row, 6]?.Text),

                    // Показания ХВС
                    ColdWaterPrevious = ParseDouble(worksheet.Cells[row, 7]?.Text),
                    ColdWaterCurrent = ParseDouble(worksheet.Cells[row, 8]?.Text),
                    ColdWaterConsumption = ParseDouble(worksheet.Cells[row, 9]?.Text),

                    // Сальдо
                    BalanceStartDebit = ParseDecimal(worksheet.Cells[row, 10]?.Text),
                    BalanceStartCredit = ParseDecimal(worksheet.Cells[row, 11]?.Text),

                    // Начисления
                    Maintenance = ParseDecimal(worksheet.Cells[row, 12]?.Text),
                    Sewage = ParseDecimal(worksheet.Cells[row, 13]?.Text),
                    Elevator = ParseDecimal(worksheet.Cells[row, 14]?.Text),
                    HotWater = ParseDecimal(worksheet.Cells[row, 15]?.Text),
                    HotWaterSoi = ParseDecimal(worksheet.Cells[row, 16]?.Text),
                    ColdWater = ParseDecimal(worksheet.Cells[row, 17]?.Text),
                    ColdWaterSoi = ParseDecimal(worksheet.Cells[row, 18]?.Text),
                    ElectricitySoi = ParseDecimal(worksheet.Cells[row, 19]?.Text),

                    TotalAccrued = ParseDecimal(worksheet.Cells[row, 20]?.Text),
                    PaidAmount = ParseDecimal(worksheet.Cells[row, 21]?.Text),
                    PaymentDateRaw = worksheet.Cells[row, 22]?.Text,

                    DebtWithoutPenalty = ParseDecimal(worksheet.Cells[row, 23]?.Text),
                    Penalty = ParseDecimal(worksheet.Cells[row, 26]?.Text),
                    CommissionReimbursement = ParseDecimal(worksheet.Cells[row, 27]?.Text),
                    TotalDebit = ParseDecimal(worksheet.Cells[row, 28]?.Text),
                    BalanceEndDebit = ParseDecimal(worksheet.Cells[row, 29]?.Text),
                    BalanceEndCredit = ParseDecimal(worksheet.Cells[row, 30]?.Text),
                };

                dto.PaymentDate = ParsePaymentDate(dto.PaymentDateRaw);

                result.Add(dto);
                row++;
            }

            return result;
        }

        public async Task ImportAccrualsToDatabaseAsync(List<AccrualChargeDto> charges, DateTime month)
        {
            var apartments = await _dbService.LoadAllApartmentsAsync();
            var apartmentDict = apartments.ToDictionary(a => a.ApartmentNumber, a => a);

            var accrualDate = new DateTime(month.Year, month.Month, 1);

            foreach (var charge in charges)
            {
                // Находим или создаем квартиру
                if (!apartmentDict.TryGetValue(charge.ApartmentNumber, out var apartment))
                {
                    apartment = new Apartment
                    {
                        ApartmentNumber = charge.ApartmentNumber,
                        FullName = charge.FullName,
                        Entrance = (charge.ApartmentNumber - 1) / 36 + 1,
                        Floor = ((charge.ApartmentNumber - 1) % 36) / 4 + 1
                    };
                    await _dbService.AddApartmentAsync(apartment);
                    apartmentDict[charge.ApartmentNumber] = apartment;
                }

                // Обновляем ФИО
                if (!string.IsNullOrWhiteSpace(charge.FullName) && apartment.FullName != charge.FullName)
                {
                    apartment.FullName = charge.FullName;
                    await _dbService.UpdateApartmentAsync(apartment);
                }

                // Добавляем показания счетчиков
                if (charge.HotWaterCurrent.HasValue || charge.ColdWaterCurrent.HasValue)
                {
                    // Проверяем, нет ли уже показаний за этот месяц
                    var existingReading = apartment.MeterReadings
                        .FirstOrDefault(r => r.Date.Year == month.Year && r.Date.Month == month.Month);

                    if (existingReading == null)
                    {
                        var reading = new MeterReading
                        {
                            Date = accrualDate,
                            ColdWater = charge.ColdWaterCurrent ?? 0,
                            HotWater = charge.HotWaterCurrent ?? 0,
                            ApartmentId = apartment.Id
                        };
                        await _dbService.AddMeterReadingAsync(reading);
                    }
                }

                // Находим или создаем лицевой счет
                var account = apartment.PersonalAccounts
                    .FirstOrDefault(pa => pa.OwnerName == charge.FullName);

                if (account == null && !string.IsNullOrWhiteSpace(charge.FullName))
                {
                    var accountNumber = $"ЛС-{charge.ApartmentNumber:0000}";
                    account = new PersonalAccount
                    {
                        AccountNumber = accountNumber,
                        OwnerName = charge.FullName,
                        ApartmentId = apartment.Id
                    };
                    await _dbService.AddPersonalAccountAsync(account);
                }

                // Добавляем начисление
                if (account != null && charge.TotalAccrued.HasValue && charge.TotalAccrued.Value > 0)
                {
                    // Проверяем, нет ли уже начисления за этот месяц
                    var existingAccrual = await _dbService.GetAccrualsByAccountAsync(account.Id);
                    var hasAccrualForMonth = existingAccrual.Any(a => a.AccrualDate.Year == month.Year && a.AccrualDate.Month == month.Month);

                    if (!hasAccrualForMonth)
                    {
                        var accrual = new Accrual
                        {
                            PersonalAccountId = account.Id,
                            AccrualDate = accrualDate,
                            Amount = charge.TotalAccrued.Value,
                            ServiceCode = "001",
                            Description = GenerateDetailedDescription(charge),
                            IsPaid = charge.PaidAmount.HasValue && charge.PaidAmount.Value >= charge.TotalAccrued.Value,
                            PaidDate = charge.IsPaid ? charge.PaymentDate ?? DateTime.Now : null
                        };
                        await _dbService.AddAccrualAsync(accrual);
                    }
                }
            }
        }

        private string GenerateDetailedDescription(AccrualChargeDto charge)
        {
            var parts = new List<string>();

            if (charge.Maintenance.HasValue && charge.Maintenance.Value > 0)
                parts.Add($"Содержание: {charge.Maintenance.Value:F2}");
            if (charge.Sewage.HasValue && charge.Sewage.Value > 0)
                parts.Add($"Канализация: {charge.Sewage.Value:F2}");
            if (charge.Elevator.HasValue && charge.Elevator.Value > 0)
                parts.Add($"Лифт: {charge.Elevator.Value:F2}");
            if (charge.HotWater.HasValue && charge.HotWater.Value > 0)
                parts.Add($"ГВС: {charge.HotWater.Value:F2}");
            if (charge.HotWaterSoi.HasValue && charge.HotWaterSoi.Value > 0)
                parts.Add($"СОИ ГВС: {charge.HotWaterSoi.Value:F2}");
            if (charge.ColdWater.HasValue && charge.ColdWater.Value > 0)
                parts.Add($"ХВС: {charge.ColdWater.Value:F2}");
            if (charge.ColdWaterSoi.HasValue && charge.ColdWaterSoi.Value > 0)
                parts.Add($"СОИ ХВС: {charge.ColdWaterSoi.Value:F2}");
            if (charge.ElectricitySoi.HasValue && charge.ElectricitySoi.Value > 0)
                parts.Add($"СОИ э/э: {charge.ElectricitySoi.Value:F2}");

            return string.Join(", ", parts);
        }
        public async Task ImportToDatabaseAsync(List<ExcelImportDto> data)
        {
            var apartments = await _dbService.LoadAllApartmentsAsync();
            var apartmentDict = apartments.ToDictionary(a => a.ApartmentNumber, a => a);

            foreach (var item in data)
            {
                if (!apartmentDict.TryGetValue(item.ApartmentNumber, out var apartment))
                {
                    // Если квартира не найдена, создаем новую
                    apartment = new Apartment
                    {
                        ApartmentNumber = item.ApartmentNumber,
                        FullName = item.FullName,
                        Entrance = CalculateEntrance(item.ApartmentNumber),
                        Floor = CalculateFloor(item.ApartmentNumber)
                    };
                    await _dbService.AddApartmentAsync(apartment);
                    apartmentDict[item.ApartmentNumber] = apartment;
                }

                // Обновляем ФИО в квартире
                if (!string.IsNullOrWhiteSpace(item.FullName) && apartment.FullName != item.FullName)
                {
                    apartment.FullName = item.FullName;
                    await _dbService.UpdateApartmentAsync(apartment);
                }

                // Добавляем показания счетчиков
                if (item.HotWaterCurrent.HasValue || item.ColdWaterCurrent.HasValue)
                {
                    var reading = new MeterReading
                    {
                        Date = new DateTime(2026, 4, 1), // Дата снятия показаний - начало апреля
                        ColdWater = item.ColdWaterCurrent ?? 0,
                        HotWater = item.HotWaterCurrent ?? 0,
                        ApartmentId = apartment.Id
                    };
                    await _dbService.AddMeterReadingAsync(reading);
                }

                // Добавляем лицевой счет (если есть ФИО)
                if (!string.IsNullOrWhiteSpace(item.FullName))
                {
                    // Проверяем, есть ли уже такой лицевой счет
                    var existingAccount = apartment.PersonalAccounts
                        .FirstOrDefault(pa => pa.OwnerName == item.FullName);
                    
                    if (existingAccount == null)
                    {
                        var accountNumber = GenerateAccountNumber(item.ApartmentNumber);
                        await _dbService.AddPersonalAccountAsync(new PersonalAccount
                        {
                            AccountNumber = accountNumber,
                            OwnerName = item.FullName,
                            ApartmentId = apartment.Id
                        });
                    }
                }

                // Добавляем начисления
                if (item.Accrued.HasValue && item.Accrued.Value > 0)
                {
                    var accrual = new Accrual
                    {
                        PersonalAccountId = GetOrCreatePersonalAccount(apartment, item.FullName).Id,
                        AccrualDate = new DateTime(2026, 4, 1),
                        Amount = item.Accrued.Value,
                        ServiceCode = "001",
                        Description = GenerateAccrualDescription(item),
                        IsPaid = item.Paid.HasValue && item.Paid.Value >= item.Accrued.Value,
                        PaidDate = item.IsPaid ? item.PaymentDate ?? DateTime.Now : null
                    };
                    await _dbService.AddAccrualAsync(accrual);
                }
            }
        }

        // Вспомогательные методы
        private double ParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;
            if (double.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;
            return 0;
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
            if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;
            return 0;
        }

        private DateTime? ParsePaymentDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            
            // Примеры форматов: "2026-04-15 00:00:00", "03.04.,26.04", "04-05.03.26"
            
            // Формат с дефисами
            if (value.Contains('-') && value.Length >= 10)
            {
                if (DateTime.TryParse(value, out DateTime result))
                    return result;
            }
            
            // Формат с точками
            if (value.Contains('.'))
            {
                // Для формата "03.04.,26.04" - берем первую дату
                if (value.Contains(','))
                {
                    var firstPart = value.Split(',')[0];
                    if (DateTime.TryParse(firstPart, out DateTime result))
                        return result;
                }
                else
                {
                    if (DateTime.TryParse(value, out DateTime result))
                        return result;
                }
            }
            
            // Формат "04-05.03.26"
            if (value.Contains('-') && value.Contains('.'))
            {
                var parts = value.Split('-');
                if (parts.Length > 0 && DateTime.TryParse(parts[0], out DateTime result))
                    return result;
            }
            
            return null;
        }

        private int CalculateEntrance(int apartmentNumber)
        {
            // Примерное распределение по подъездам
            if (apartmentNumber <= 72) return 1;
            if (apartmentNumber <= 144) return 2;
            return 3;
        }

        private int CalculateFloor(int apartmentNumber)
        {
            // Примерное распределение по этажам (пусть в подъезде 9 этажей по 4 квартиры)
            int apartmentsPerFloor = 4;
            int floor = ((apartmentNumber - 1) % 36) / apartmentsPerFloor + 1;
            return Math.Min(floor, 9);
        }

        private string GenerateAccountNumber(int apartmentNumber)
        {
            return $"ЛС-{apartmentNumber:0000}";
        }

        private PersonalAccount GetOrCreatePersonalAccount(Apartment apartment, string ownerName)
        {
            var account = apartment.PersonalAccounts.FirstOrDefault(pa => pa.OwnerName == ownerName);
            if (account == null)
            {
                account = new PersonalAccount
                {
                    AccountNumber = GenerateAccountNumber(apartment.ApartmentNumber),
                    OwnerName = ownerName,
                    ApartmentId = apartment.Id
                };
                // Здесь нужно будет добавить асинхронное создание
            }
            return account;
        }

        // Добавьте этот метод в класс ExcelImportService
        public async Task<List<AccrualChargeDto>> ReadApril2026SheetNewAsync(string filePath)
        {
            var result = new List<AccrualChargeDto>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets["Апрель 2026"];

            if (worksheet == null)
                throw new Exception("Лист 'Апрель 2026' не найден");

            // Начинаем с 3 строки (после заголовков)
            int row = 3;
            while (true)
            {
                var apartmentNumberCell = worksheet.Cells[row, 1]?.Text;
                if (string.IsNullOrWhiteSpace(apartmentNumberCell) || apartmentNumberCell == "ИТОГО")
                    break;

                if (!int.TryParse(apartmentNumberCell?.ToString(), out int apartmentNumber))
                {
                    row++;
                    continue;
                }

                var dto = new AccrualChargeDto
                {
                    ApartmentNumber = apartmentNumber,
                    FullName = worksheet.Cells[row, 2]?.Text ?? "",
                    TotalArea = ParseDouble(worksheet.Cells[row, 3]?.Text),

                    // Показания ГВС (колонки D, E, F)
                    HotWaterPrevious = ParseDouble(worksheet.Cells[row, 4]?.Text),
                    HotWaterCurrent = ParseDouble(worksheet.Cells[row, 5]?.Text),
                    HotWaterConsumption = ParseDouble(worksheet.Cells[row, 6]?.Text),

                    // Показания ХВС (колонки G, H, I)
                    ColdWaterPrevious = ParseDouble(worksheet.Cells[row, 7]?.Text),
                    ColdWaterCurrent = ParseDouble(worksheet.Cells[row, 8]?.Text),
                    ColdWaterConsumption = ParseDouble(worksheet.Cells[row, 9]?.Text),

                    // Сальдо на начало (колонки J, K)
                    BalanceStartDebit = ParseDecimal(worksheet.Cells[row, 10]?.Text),
                    BalanceStartCredit = ParseDecimal(worksheet.Cells[row, 11]?.Text),

                    // Начисления по статьям (колонки L - S)
                    Maintenance = ParseDecimal(worksheet.Cells[row, 12]?.Text),      // L - Содержание
                    Sewage = ParseDecimal(worksheet.Cells[row, 13]?.Text),           // M - Канализация
                    Elevator = ParseDecimal(worksheet.Cells[row, 14]?.Text),         // N - Лифт
                    HotWater = ParseDecimal(worksheet.Cells[row, 15]?.Text),         // O - ГВС
                    HotWaterSoi = ParseDecimal(worksheet.Cells[row, 16]?.Text),      // P - СОИ ГВС
                    ColdWater = ParseDecimal(worksheet.Cells[row, 17]?.Text),        // Q - ХВС
                    ColdWaterSoi = ParseDecimal(worksheet.Cells[row, 18]?.Text),     // R - СОИ ХВС
                    ElectricitySoi = ParseDecimal(worksheet.Cells[row, 19]?.Text),   // S - СОИ элект. энер

                    // Итого начислено (колонка T)
                    TotalAccrued = ParseDecimal(worksheet.Cells[row, 20]?.Text),

                    // Оплачено (колонка U)
                    PaidAmount = ParseDecimal(worksheet.Cells[row, 21]?.Text),

                    // Дата оплаты (колонка V)
                    PaymentDateRaw = worksheet.Cells[row, 22]?.Text,

                    // Долги и пени (колонка W, X, Y, Z)
                    DebtWithoutPenalty = ParseDecimal(worksheet.Cells[row, 23]?.Text),   // W - Д-т без пеней
                                                                                         // X - начисл. За прошлый месяц (пропускаем)
                                                                                         // Y - сумма задолж-ности на пеня (пропускаем)
                    Penalty = ParseDecimal(worksheet.Cells[row, 26]?.Text),              // Z - пеня

                    // Комиссия и итоги (колонки AA, AB, AC)
                    CommissionReimbursement = ParseDecimal(worksheet.Cells[row, 27]?.Text), // AA - возмещение по комиссии
                    TotalDebit = ParseDecimal(worksheet.Cells[row, 28]?.Text),              // AB - Итого Д-т без пеней
                    BalanceEndDebit = ParseDecimal(worksheet.Cells[row, 29]?.Text),         // AC - С-до на конец месяца Д-т
                    BalanceEndCredit = ParseDecimal(worksheet.Cells[row, 30]?.Text)         // AD - С-до на конец месяца К-т
                };

                // Парсим дату оплаты
                if (!string.IsNullOrWhiteSpace(dto.PaymentDateRaw))
                {
                    dto.PaymentDate = ParsePaymentDate(dto.PaymentDateRaw);
                }

                result.Add(dto);
                row++;
            }

            return result;
        }

        private string GenerateAccrualDescription(ExcelImportDto dto)
        {
            var parts = new List<string>();
            
            if (dto.Maintenance.HasValue && dto.Maintenance.Value > 0)
                parts.Add($"Содержание: {dto.Maintenance.Value:F2}");
            if (dto.Sewage.HasValue && dto.Sewage.Value > 0)
                parts.Add($"Канализация: {dto.Sewage.Value:F2}");
            if (dto.Elevator.HasValue && dto.Elevator.Value > 0)
                parts.Add($"Лифт: {dto.Elevator.Value:F2}");
            if (dto.HotWater.HasValue && dto.HotWater.Value > 0)
                parts.Add($"ГВС: {dto.HotWater.Value:F2}");
            if (dto.ColdWater.HasValue && dto.ColdWater.Value > 0)
                parts.Add($"ХВС: {dto.ColdWater.Value:F2}");
            
            return string.Join(", ", parts);
        }
    }
}