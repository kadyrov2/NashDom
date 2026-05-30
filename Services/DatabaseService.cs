using Microsoft.EntityFrameworkCore;
using NashDom.Data;
using NashDom.Models;

namespace NashDom.Services
{
    public class DatabaseService
    {
        // Удаляем поле _context - больше не нужно!

        public async Task EnsureDatabaseCreatedAsync()
        {
            using var context = new ApplicationDbContext();
            await context.Database.EnsureCreatedAsync();
        }

        public async Task<List<Apartment>> LoadAllApartmentsAsync()
        {
            using var context = new ApplicationDbContext();
            return await context.Apartments
                .Include(a => a.PersonalAccounts)
                .Include(a => a.MeterReadings)
                .Include(a => a.RegistrationCard)
                .Include(a => a.MeterReplacements)
                .OrderBy(a => a.ApartmentNumber)
                .ToListAsync();
        }

        public async Task<List<Apartment>> GetApartmentsByEntranceAsync(int entrance)
        {
            using var context = new ApplicationDbContext();
            return await context.Apartments
                .Include(a => a.PersonalAccounts)
                .Include(a => a.MeterReadings)
                .Include(a => a.RegistrationCard)
                .Include(a => a.MeterReplacements)
                .Where(a => a.Entrance == entrance)
                .OrderBy(a => a.Floor)
                .ThenBy(a => a.ApartmentNumber)
                .ToListAsync();
        }

        public async Task<List<Apartment>> GetApartmentsByFloorAsync(int entrance, int floor)
        {
            using var context = new ApplicationDbContext();
            return await context.Apartments
                .Include(a => a.PersonalAccounts)
                .Include(a => a.MeterReadings)
                .Include(a => a.RegistrationCard)
                .Include(a => a.MeterReplacements)
                .Where(a => a.Entrance == entrance && a.Floor == floor)
                .OrderBy(a => a.ApartmentNumber)
                .ToListAsync();
        }

        public async Task<Apartment?> GetApartmentByIdAsync(int id)
        {
            using var context = new ApplicationDbContext();
            return await context.Apartments
                .Include(a => a.PersonalAccounts)
                .Include(a => a.MeterReadings)
                .Include(a => a.RegistrationCard)
                .Include(a => a.MeterReplacements)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddMeterReplacementAsync(MeterReplacement replacement)
        {
            using var context = new ApplicationDbContext();
            try
            {
                await context.MeterReplacements.AddAsync(replacement);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Ошибка БД: {inner}");
            }
        }

        public async Task UpdateMeterReplacementAsync(MeterReplacement replacement)
        {
            using var context = new ApplicationDbContext();
            context.MeterReplacements.Update(replacement);
            await context.SaveChangesAsync();
        }

        public async Task RemoveMeterReplacementAsync(int id)
        {
            using var context = new ApplicationDbContext();
            var replacement = await context.MeterReplacements.FindAsync(id);
            if (replacement != null)
            {
                context.MeterReplacements.Remove(replacement);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddApartmentAsync(Apartment apartment)
        {
            using var context = new ApplicationDbContext();
            await context.Apartments.AddAsync(apartment);
            await context.SaveChangesAsync();
        }

        public async Task UpdateApartmentAsync(Apartment apartment)
        {
            using var context = new ApplicationDbContext();
            context.Apartments.Update(apartment);
            await context.SaveChangesAsync();
        }

        public async Task DeleteApartmentAsync(int id)
        {
            using var context = new ApplicationDbContext();
            var apartment = await context.Apartments.FindAsync(id);
            if (apartment != null)
            {
                context.Apartments.Remove(apartment);
                await context.SaveChangesAsync();
            }
        }

        // Рассчитать и сохранить начисления для всех квартир
        public async Task CalculateAndSaveAccrualsAsync(DateTime month)
        {
            using var context = new ApplicationDbContext();

            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1);

            // Получаем все квартиры с лицевыми счетами
            var apartments = await context.Apartments
                .Include(a => a.PersonalAccounts)
                .Where(a => a.TotalArea > 0)
                .ToListAsync();

            // Удаляем старые начисления за этот месяц
            var oldAccruals = await context.Accruals
                .Where(a => a.AccrualDate >= startDate && a.AccrualDate < endDate)
                .ToListAsync();

            context.Accruals.RemoveRange(oldAccruals);

            // Создаем новые начисления
            foreach (var apartment in apartments)
            {
                foreach (var account in apartment.PersonalAccounts)
                {
                    decimal maintenance = (decimal)apartment.TotalArea * 48.88m;
                    decimal elevator = (decimal)apartment.TotalArea * 4.39m;
                    decimal totalAmount = maintenance + elevator;

                    var accrual = new Accrual
                    {
                        PersonalAccountId = account.Id,
                        AccrualDate = startDate,
                        Amount = totalAmount,
                        ServiceCode = "001",
                        Description = $"Содержание: {maintenance:F2} руб., Лифт: {elevator:F2} руб.",
                        IsPaid = false,
                        CreatedAt = DateTime.Now
                    };

                    await context.Accruals.AddAsync(accrual);
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<AccrualChargeDto>> GetAccrualChargesByMonthAsync(DateTime month)
        {
            using var context = new ApplicationDbContext();
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1);

            // Константы
            decimal sewageRate = 34.52m;
            decimal hotWaterRate = 236.95m;
            decimal coldWaterRate = 27.38m;
            decimal hotWaterSoiConstant = 36m;
            decimal coldWaterSoiConstant = 38m;
            decimal electricitySoiConstant = 1143m;
            decimal electricitySoiRate = 3.49m;
            decimal totalAreaBuilding = 4294.9m;

            // Проверяем, есть ли начисления за этот месяц
            var hasAccruals = await context.Accruals
                .AnyAsync(a => a.AccrualDate >= startDate && a.AccrualDate < endDate);

            // Если нет начислений - автоматически рассчитываем
            if (!hasAccruals)
            {
                await CalculateAndSaveAccrualsAsync(month);
            }

            // Получаем квартиры
            var apartments = await context.Apartments.ToListAsync();

            var result = new List<AccrualChargeDto>();

            foreach (var apartment in apartments)
            {
                // Получаем текущие показания
                var currentReading = await context.MeterReadings
                    .Where(m => m.ApartmentId == apartment.Id && m.Date >= startDate && m.Date < endDate)
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefaultAsync();

                // Получаем предыдущие показания
                var previousReading = await context.MeterReadings
                    .Where(m => m.ApartmentId == apartment.Id && m.Date < startDate)
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefaultAsync();

                if (currentReading == null) continue;

                double hotWaterPrevious = previousReading?.HotWater ?? 0;
                double hotWaterCurrent = currentReading.HotWater;
                double hotWaterConsumption = hotWaterCurrent - hotWaterPrevious;
                if (hotWaterConsumption < 0) hotWaterConsumption = 0;

                double coldWaterPrevious = previousReading?.ColdWater ?? 0;
                double coldWaterCurrent = currentReading.ColdWater;
                double coldWaterConsumption = coldWaterCurrent - coldWaterPrevious;
                if (coldWaterConsumption < 0) coldWaterConsumption = 0;

                // Получаем лицевой счет
                var account = await context.PersonalAccounts
                    .FirstOrDefaultAsync(a => a.ApartmentId == apartment.Id);

                if (account == null) continue;

                // Получаем начисление
                var accrual = await context.Accruals
                    .FirstOrDefaultAsync(a => a.PersonalAccountId == account.Id && a.AccrualDate >= startDate && a.AccrualDate < endDate);

                // Рассчитываем значения
                decimal maintenance = (decimal)apartment.TotalArea * 48.88m;
                decimal elevator = (decimal)apartment.TotalArea * 4.39m;
                decimal sewage = ((decimal)hotWaterConsumption + (decimal)coldWaterConsumption) * sewageRate;
                decimal hotWater = (decimal)hotWaterConsumption * hotWaterRate;
                decimal hotWaterSoi = (hotWaterRate * hotWaterSoiConstant) / totalAreaBuilding * (decimal)apartment.TotalArea;
                decimal coldWater = (decimal)coldWaterConsumption * coldWaterRate;
                decimal coldWaterSoi = (coldWaterRate * coldWaterSoiConstant) / totalAreaBuilding * (decimal)apartment.TotalArea;
                decimal electricitySoi = (electricitySoiRate * electricitySoiConstant) / totalAreaBuilding * (decimal)apartment.TotalArea;

                decimal totalAccrued = maintenance + sewage + elevator + hotWater + hotWaterSoi + coldWater + coldWaterSoi + electricitySoi;

                var dto = new AccrualChargeDto
                {
                    ApartmentNumber = apartment.ApartmentNumber,
                    FullName = apartment.FullName,
                    TotalArea = apartment.TotalArea,

                    // Показания
                    HotWaterPrevious = hotWaterPrevious,
                    HotWaterCurrent = hotWaterCurrent,
                    HotWaterConsumption = hotWaterConsumption,
                    ColdWaterPrevious = coldWaterPrevious,
                    ColdWaterCurrent = coldWaterCurrent,
                    ColdWaterConsumption = coldWaterConsumption,

                    // Начисления
                    Maintenance = Math.Round(maintenance, 2),
                    Sewage = Math.Round(sewage, 2),
                    Elevator = Math.Round(elevator, 2),
                    HotWater = Math.Round(hotWater, 2),
                    HotWaterSoi = Math.Round(hotWaterSoi, 2),
                    ColdWater = Math.Round(coldWater, 2),
                    ColdWaterSoi = Math.Round(coldWaterSoi, 2),
                    ElectricitySoi = Math.Round(electricitySoi, 2),

                    TotalAccrued = Math.Round(totalAccrued, 2),

                    // Оплата
                    PaidAmount = accrual?.IsPaid == true ? accrual?.Amount : 0,
                    PaymentDate = accrual?.PaidDate
                };

                result.Add(dto);
            }

            return result.OrderBy(x => x.ApartmentNumber).ToList();
        }

        // Рассчитать начисления для квартиры на основе площади
        public async Task CalculateAccrualsForApartmentAsync(int apartmentId, DateTime month)
        {
            using var context = new ApplicationDbContext();

            // Получаем квартиру
            var apartment = await context.Apartments.FindAsync(apartmentId);
            if (apartment == null) return;

            // Получаем лицевой счет
            var account = await context.PersonalAccounts
                .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId);
            if (account == null) return;

            // Рассчитываем по формулам
            decimal maintenanceRate = 48.88m;  // ставка за содержание
            decimal elevatorRate = 4.39m;      // ставка за лифт

            decimal maintenance = (decimal)apartment.TotalArea * maintenanceRate;
            decimal elevator = (decimal)apartment.TotalArea * elevatorRate;

            // Проверяем, есть ли уже начисление за этот месяц
            var existingAccrual = await context.Accruals
                .FirstOrDefaultAsync(a => a.PersonalAccountId == account.Id &&
                                          a.AccrualDate.Year == month.Year &&
                                          a.AccrualDate.Month == month.Month);

            if (existingAccrual != null)
            {
                // Обновляем существующее начисление
                existingAccrual.Amount = maintenance + elevator;
                existingAccrual.Description = $"Содержание: {maintenance:F2} руб., Лифт: {elevator:F2} руб.";
                context.Accruals.Update(existingAccrual);
            }
            else
            {
                // Создаем новое начисление
                var accrual = new Accrual
                {
                    PersonalAccountId = account.Id,
                    AccrualDate = new DateTime(month.Year, month.Month, 1),
                    Amount = maintenance + elevator,
                    ServiceCode = "001",
                    Description = $"Содержание: {maintenance:F2} руб., Лифт: {elevator:F2} руб.",
                    IsPaid = false,
                    CreatedAt = DateTime.Now
                };
                await context.Accruals.AddAsync(accrual);
            }

            await context.SaveChangesAsync();
        }

        // Рассчитать начисления для всех квартир
        public async Task CalculateAllAccrualsAsync(DateTime month)
        {
            using var context = new ApplicationDbContext();

            var apartments = await context.Apartments.ToListAsync();
            decimal maintenanceRate = 48.88m;
            decimal elevatorRate = 4.39m;

            foreach (var apartment in apartments)
            {
                var account = await context.PersonalAccounts
                    .FirstOrDefaultAsync(a => a.ApartmentId == apartment.Id);

                if (account == null) continue;

                decimal maintenance = (decimal)apartment.TotalArea * maintenanceRate;
                decimal elevator = (decimal)apartment.TotalArea * elevatorRate;
                decimal totalAmount = maintenance + elevator;

                var existingAccrual = await context.Accruals
                    .FirstOrDefaultAsync(a => a.PersonalAccountId == account.Id &&
                                              a.AccrualDate.Year == month.Year &&
                                              a.AccrualDate.Month == month.Month);

                if (existingAccrual != null)
                {
                    existingAccrual.Amount = totalAmount;
                    existingAccrual.Description = $"Содержание: {maintenance:F2} руб., Лифт: {elevator:F2} руб.";
                    context.Accruals.Update(existingAccrual);
                }
                else
                {
                    var accrual = new Accrual
                    {
                        PersonalAccountId = account.Id,
                        AccrualDate = new DateTime(month.Year, month.Month, 1),
                        Amount = totalAmount,
                        ServiceCode = "001",
                        Description = $"Содержание: {maintenance:F2} руб., Лифт: {elevator:F2} руб.",
                        IsPaid = false,
                        CreatedAt = DateTime.Now
                    };
                    await context.Accruals.AddAsync(accrual);
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task UpdateAccrualChargeAsync(AccrualChargeDto charge, DateTime month)
        {
            using var context = new ApplicationDbContext();
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var accounts = await (from a in context.PersonalAccounts
                                  join apt in context.Apartments on a.ApartmentId equals apt.Id
                                  where apt.ApartmentNumber == charge.ApartmentNumber
                                  select a).ToListAsync();

            foreach (var account in accounts)
            {
                var accruals = await context.Accruals
                    .Where(a => a.PersonalAccountId == account.Id &&
                                a.AccrualDate >= startDate &&
                                a.AccrualDate <= endDate)
                    .ToListAsync();

                var remainingToPay = charge.PaidAmount ?? 0;
                foreach (var accrual in accruals.OrderBy(a => a.AccrualDate))
                {
                    if (remainingToPay <= 0)
                    {
                        accrual.IsPaid = false;
                        accrual.PaidDate = null;
                    }
                    else if (remainingToPay >= accrual.Amount)
                    {
                        accrual.IsPaid = true;
                        accrual.PaidDate = DateTime.Now;
                        remainingToPay -= accrual.Amount;
                    }
                    else
                    {
                        accrual.IsPaid = false;
                    }
                    context.Accruals.Update(accrual);
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task AddPersonalAccountAsync(PersonalAccount account)
        {
            using var context = new ApplicationDbContext();
            await context.PersonalAccounts.AddAsync(account);
            await context.SaveChangesAsync();
        }

        public async Task RemovePersonalAccountAsync(int id)
        {
            using var context = new ApplicationDbContext();
            var account = await context.PersonalAccounts.FindAsync(id);
            if (account != null)
            {
                context.PersonalAccounts.Remove(account);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddMeterReadingAsync(MeterReading reading)
        {
            using var context = new ApplicationDbContext();
            await context.MeterReadings.AddAsync(reading);
            await context.SaveChangesAsync();
        }

        public async Task RemoveMeterReadingAsync(int id)
        {
            using var context = new ApplicationDbContext();
            var reading = await context.MeterReadings.FindAsync(id);
            if (reading != null)
            {
                context.MeterReadings.Remove(reading);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Accrual>> GetAccrualsByAccountAsync(int personalAccountId)
        {
            using var context = new ApplicationDbContext();
            return await context.Accruals
                .Include(a => a.PersonalAccount)
                .ThenInclude(pa => pa!.Apartment)
                .Where(a => a.PersonalAccountId == personalAccountId)
                .OrderByDescending(a => a.AccrualDate)
                .ToListAsync();
        }

        public async Task AddAccrualAsync(Accrual accrual)
        {
            using var context = new ApplicationDbContext();
            await context.Accruals.AddAsync(accrual);
            await context.SaveChangesAsync();
        }

        public async Task<OrganizationSettings?> GetOrganizationSettingsAsync()
        {
            using var context = new ApplicationDbContext();
            return await context.OrganizationSettings.FirstOrDefaultAsync();
        }

        public async Task SaveOrganizationSettingsAsync(OrganizationSettings settings)
        {
            using var context = new ApplicationDbContext();
            var existing = await context.OrganizationSettings.FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.TemplateName = settings.TemplateName;
                existing.OrganizationName = settings.OrganizationName;
                existing.Inn = settings.Inn;
                existing.Kpp = settings.Kpp;
                existing.Bik = settings.Bik;
                existing.CorrespondentAccount = settings.CorrespondentAccount;
                existing.BankName = settings.BankName;
                existing.SettlementAccount = settings.SettlementAccount;
                existing.ServiceCode = settings.ServiceCode;
                existing.HeaderText = settings.HeaderText;
                existing.FooterText = settings.FooterText;

                context.OrganizationSettings.Update(existing);
            }
            else
            {
                await context.OrganizationSettings.AddAsync(settings);
            }
            await context.SaveChangesAsync();
        }

        public async Task<List<AccrualReportDto>> GetAccrualReportDataAsync(DateTime startDate, DateTime endDate)
        {
            using var context = new ApplicationDbContext();
            var query = from accrual in context.Accruals
                        join account in context.PersonalAccounts on accrual.PersonalAccountId equals account.Id
                        join apartment in context.Apartments on account.ApartmentId equals apartment.Id
                        where accrual.AccrualDate >= startDate && accrual.AccrualDate <= endDate
                        select new AccrualReportDto
                        {
                            AccountNumber = account.AccountNumber,
                            FullName = account.OwnerName,
                            Address = $"ул. Ваша, д. 1, кв. {apartment.ApartmentNumber}",
                            Amount = accrual.Amount,
                            AccrualDate = accrual.AccrualDate,
                            ServiceCode = accrual.ServiceCode,
                            ApartmentNumber = apartment.ApartmentNumber,
                            Entrance = apartment.Entrance,
                            Floor = apartment.Floor
                        };

            return await query.ToListAsync();
        }
    }
}