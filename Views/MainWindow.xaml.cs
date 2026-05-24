using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NashDom.ViewModels;
using NashDom.Models;
using NashDom.Data;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace NashDom.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private decimal _totalAmount;
        private AccrualChargesViewModel? _chargesViewModel;
        public string SelectedMonth { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Используем обновлённые имена элементов
            ApartmentsListMain.ItemsSource = _viewModel.Apartments;
            ApartmentsListMain.SelectionChanged += ApartmentsList_SelectionChanged;

            if (_viewModel.Apartments.Any() && _viewModel.SelectedApartment == null)
            {
                ApartmentsListMain.SelectedItem = _viewModel.Apartments.First();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Apartments.Any() && ApartmentsListMain.SelectedItem == null)
            {
                ApartmentsListMain.SelectedItem = _viewModel.Apartments.First();
            }
        }

        private void ApartmentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApartmentsListMain.SelectedItem != null)
            {
                _viewModel.SelectedApartment = ApartmentsListMain.SelectedItem as Apartment;
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_viewModel == null || _viewModel.Apartments == null) return;

            var entranceText = (EntranceFilterMain.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var floorText = (FloorFilterMain.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = _viewModel.Apartments.AsEnumerable();

            if (entranceText != "Все подъезды" && entranceText != null)
            {
                var entrance = int.Parse(entranceText.Replace("Подъезд ", ""));
                filtered = filtered.Where(a => a.Entrance == entrance);
            }

            if (floorText != "Все этажи" && floorText != null)
            {
                var floor = int.Parse(floorText.Replace("Этаж ", ""));
                filtered = filtered.Where(a => a.Floor == floor);
            }

            ApartmentsListMain.ItemsSource = filtered.ToList();

            if (ApartmentsListMain.Items.Count > 0)
            {
                ApartmentsListMain.SelectedItem = ApartmentsListMain.Items[0];
            }
        }

        // Обработчики для лицевых счетов
        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewAccountNumber.Text) &&
                !string.IsNullOrWhiteSpace(NewOwnerName.Text))
            {
                _viewModel.AddPersonalAccountAsync(NewAccountNumber.Text, NewOwnerName.Text);

                NewAccountNumber.Clear();
                NewOwnerName.Clear();
            }
            else
            {
                MessageBox.Show("Заполните номер лицевого счёта и ФИО владельца",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PersonalAccount account)
            {
                if (MessageBox.Show($"Удалить лицевой счёт {account.AccountNumber}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _viewModel.RemovePersonalAccountAsync(account);
                }
            }
        }

        private void AddAccrualForAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PersonalAccount account)
            {
                var accrualWindow = new AccrualWindow(_viewModel, account);
                accrualWindow.Owner = this;
                if (accrualWindow.ShowDialog() == true)
                {
                    MessageBox.Show($"Начисление добавлено на счёт {account.AccountNumber}",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // Обработчики для показаний счётчиков
        private void AddReading_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ColdWaterInput.Text, out double cold) &&
                double.TryParse(HotWaterInput.Text, out double hot))
            {
                _viewModel.AddMeterReadingAsync(
                    ReadingDate.SelectedDate ?? DateTime.Today,
                    cold,
            hot);

                ColdWaterInput.Clear();
                HotWaterInput.Clear();
                ReadingDate.SelectedDate = DateTime.Today;
            }
            else
            {
                MessageBox.Show("Введите корректные показания ХВС и ГВС",
            "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveReading_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MeterReading reading)
            {
                if (MessageBox.Show($"Удалить показания от {reading.Date:dd.MM.yyyy}?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _viewModel.RemoveMeterReadingAsync(reading);
                }
            }
        }

        // Обработчики для замены счётчиков
        private void AddReplacement_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedApartment == null)
            {
                MessageBox.Show("Сначала выберите квартиру");
                return;
            }

            DateTime? hotWaterDate = HotWaterMeterDate.SelectedDate;
            DateTime? coldWaterDate = ColdWaterMeterDate.SelectedDate;

            int? hotWaterYear = HotWaterVerificationDate.SelectedDate?.Year;
            int? coldWaterYear = ColdWaterVerificationDate.SelectedDate?.Year;

            _viewModel.AddMeterReplacementAsync(
                HotWaterMeterNumber.Text.Trim(),
                hotWaterDate,
                hotWaterYear,
                HotWaterNote.Text.Trim(),
                ColdWaterMeterNumber.Text.Trim(),
                coldWaterDate,
                coldWaterYear,
                ColdWaterNote.Text.Trim());

            // Очищаем поля
            HotWaterMeterNumber.Clear();
            HotWaterMeterDate.SelectedDate = DateTime.Today;
            HotWaterVerificationDate.SelectedDate = null;
            HotWaterNote.Clear();
            ColdWaterMeterNumber.Clear();
            ColdWaterMeterDate.SelectedDate = DateTime.Today;
            ColdWaterVerificationDate.SelectedDate = null;
            ColdWaterNote.Clear();

            MessageBox.Show("Запись добавлена", "Успешно",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveReplacement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MeterReplacement replacement)
            {
                if (MessageBox.Show($"Удалить запись о замене счётчиков?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _viewModel.RemoveMeterReplacementAsync(replacement);
                }
            }
        }

        // Обработчики для вкладки "Реестр начислений"
        private async void GenerateReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportStartDatePicker.SelectedDate.HasValue)
                    _viewModel.ReportStartDate = ReportStartDatePicker.SelectedDate.Value;
                if (ReportEndDatePicker.SelectedDate.HasValue)
                    _viewModel.ReportEndDate = ReportEndDatePicker.SelectedDate.Value;

                await _viewModel.GenerateReportAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования реестра: {ex.Message}", "Ошибка",
            MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                    await _viewModel.ExportReportToExcelAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        private async void RefreshChargesBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadChargesDataAsync();
        }

        private async void SaveChargeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChargesGrid.SelectedItem is AccrualChargeDto selectedCharge)
                {
                    if (_chargesViewModel == null)
                    {
                        _chargesViewModel = new AccrualChargesViewModel();
                    }

                    _chargesViewModel.SelectedCharge = selectedCharge;

                    // Обновляем данные
                    await LoadChargesDataAsync();

                    MessageBox.Show("Данные сохранены", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выберите запись для сохранения", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReceiptBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChargesGrid.SelectedItem is AccrualChargeDto selectedCharge)
                {
                    // Логика печати квитанции
                    var receiptText = $@"
=== КВИТАНЦИЯ ОБ ОПЛАТЕ ===

Квартира №: {selectedCharge.ApartmentNumber}
ФИО: {selectedCharge.FullName}
Площадь: {selectedCharge.TotalArea} м²

Начислено: {selectedCharge.TotalAccrued:N2} руб.
Оплачено: {selectedCharge.PaidAmount:N2} руб.
Задолженность: {(selectedCharge.TotalAccrued - selectedCharge.PaidAmount):N2} руб.

Дата: {DateTime.Now:dd.MM.yyyy}
";

                    MessageBox.Show(receiptText, "Квитанция",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выберите запись для печати", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadChargesDataAsync()
        {
            try
            {
                if (_chargesViewModel == null)
                {
                    _chargesViewModel = new AccrualChargesViewModel();
                    // Устанавливаем DataContext для Grid на вкладке
                    var chargesGrid = ChargesGrid;
                    if (chargesGrid != null)
                    {
                        chargesGrid.DataContext = _chargesViewModel;
                    }
                }

                // Здесь должна быть логика загрузки данных начислений
                // Например:
                // await _chargesViewModel.LoadChargesAsync(GetSelectedMonthAsDateTime());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_viewModel);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик изменения выбранной вкладки
            // Можно добавить логику при переключении между вкладками
        }

        // Рассчитать и сохранить начисления для всех квартир
        public async Task CalculateAndSaveAccrualsAsync(DateTime month)
        {
            using var context = new ApplicationDbContext();

            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1);

            // Получаем все квартиры с лицевыми счетами и показаниями
            var apartments = await context.Apartments
                .Include(a => a.PersonalAccounts)
                .Include(a => a.MeterReadings.Where(m => m.Date >= startDate && m.Date < endDate))
                .Where(a => a.TotalArea > 0)
                .ToListAsync();

            // Константы
            decimal sewageRate = 34.52m;           // Канализация
            decimal hotWaterRate = 236.95m;        // ГВС
            decimal coldWaterRate = 27.38m;        // ХВС
            decimal hotWaterSoiConstant = 36m;     // СОИ ГВС
            decimal coldWaterSoiConstant = 38m;    // СОИ ХВС
            decimal electricitySoiConstant = 1143m; // СОИ элект. энер
            decimal electricitySoiRate = 3.49m;    // СОИ элект. энер ставка
            decimal totalAreaBuilding = 4294.9m;   // Общая площадь дома

            // Удаляем старые начисления за этот месяц
            var oldAccruals = await context.Accruals
                .Where(a => a.AccrualDate >= startDate && a.AccrualDate < endDate)
                .ToListAsync();

            context.Accruals.RemoveRange(oldAccruals);
            await context.SaveChangesAsync();

            // Создаём новые начисления
            foreach (var apartment in apartments)
            {
                // Получаем показания счётчиков за месяц
                var reading = apartment.MeterReadings.FirstOrDefault();

                if (reading == null) continue;

                // Рассчитываем расход
                double hotWaterConsumption = reading.HotWater - (apartment.MeterReadings
                    .OrderByDescending(m => m.Date)
                    .Skip(1)
                    .FirstOrDefault()?.HotWater ?? 0);

                double coldWaterConsumption = reading.ColdWater - (apartment.MeterReadings
                    .OrderByDescending(m => m.Date)
                    .Skip(1)
                    .FirstOrDefault()?.ColdWater ?? 0);

                if (hotWaterConsumption < 0) hotWaterConsumption = 0;
                if (coldWaterConsumption < 0) coldWaterConsumption = 0;

                foreach (var account in apartment.PersonalAccounts)
                {
                    // Основные начисления
                    decimal maintenance = (decimal)apartment.TotalArea * 48.88m;
                    decimal elevator = (decimal)apartment.TotalArea * 4.39m;

                    // Канализация = (ГВС расход + ХВС расход) * 34,52
                    decimal sewage = ((decimal)hotWaterConsumption + (decimal)coldWaterConsumption) * sewageRate;

                    // ГВС = ГВС расход * 236,95
                    decimal hotWater = (decimal)hotWaterConsumption * hotWaterRate;

                    // СОИ ГВС = (236,95 * 36) / 4294,9 * площадь
                    decimal hotWaterSoi = (hotWaterRate * hotWaterSoiConstant) / totalAreaBuilding * (decimal)apartment.TotalArea;

                    // ХВС = ХВС расход * 27,38
                    decimal coldWater = (decimal)coldWaterConsumption * coldWaterRate;

                    // СОИ ХВС = (27,38 * 38) / 4294,9 * площадь
                    decimal coldWaterSoi = (coldWaterRate * coldWaterSoiConstant) / totalAreaBuilding * (decimal)apartment.TotalArea;

                    // СОИ элект. энер = (3,49 * 1143) / 4294,9 * площадь
                    decimal electricitySoi = (electricitySoiRate * electricitySoiConstant) / totalAreaBuilding * (decimal)apartment.TotalArea;

                    // Итого начислено
                    decimal totalAccrued = maintenance + sewage + elevator + hotWater + hotWaterSoi + coldWater + coldWaterSoi + electricitySoi;

                    var accrual = new Accrual
                    {
                        PersonalAccountId = account.Id,
                        AccrualDate = startDate,
                        Amount = Math.Round(totalAccrued, 2),
                        ServiceCode = "001",
                        Description = $"Содержание: {Math.Round(maintenance, 2)}; " +
                                    $"Канализация: {Math.Round(sewage, 2)}; " +
                                    $"Лифт: {Math.Round(elevator, 2)}; " +
                                    $"ГВС: {Math.Round(hotWater,
                                                    2)}; " +
                $"СОИ ГВС: {Math.Round(hotWaterSoi, 2)}; " +
                $"ХВС: {Math.Round(coldWater, 2)}; " +
                $"СОИ ХВС: {Math.Round(coldWaterSoi, 2)}; " +
                $"СОИ э/э: {Math.Round(electricitySoi, 2)}",
                        IsPaid = false,
                        CreatedAt = DateTime.Now
                    };


                    await context.Accruals.AddAsync(accrual);
                }
            }

            await context.SaveChangesAsync();
        }

        private async void CalculateAccrualsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedMonth))
                {
                    MessageBox.Show("Выберите месяц для расчёта начислений", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Преобразуем SelectedMonth в DateTime (предполагаем формат "MM.yyyy")
                if (!DateTime.TryParseExact(SelectedMonth, "MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime month))
                {
                    MessageBox.Show("Неверный формат месяца. Используйте формат ММ.ГГГГ", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await CalculateAndSaveAccrualsAsync(month);
                MessageBox.Show($"Начисления за {month:MMMM yyyy} успешно рассчитаны и сохранены",
                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчёта начислений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательный метод для получения даты из SelectedMonth
        private DateTime GetSelectedMonthAsDateTime()
        {
            if (string.IsNullOrEmpty(SelectedMonth) ||
                !DateTime.TryParseExact(SelectedMonth, "MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return DateTime.Now;
            }
            return result;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

        
        }
    }
}
