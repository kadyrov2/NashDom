using NashDom.Models;
using NashDom.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NashDom.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private ObservableCollection<Apartment> _apartments = new();
        private Apartment? _selectedApartment;
        private bool _isLoading;
        private ObservableCollection<AccrualReportDto> _reportData = new();
        private OrganizationSettings? _orgSettings;
        private DateTime _reportStartDate = DateTime.Today.AddMonths(-1);
        private DateTime _reportEndDate = DateTime.Today;
        private decimal _totalAmount;
        private AccrualChargesViewModel _chargesViewModel;
        public List<string> Months { get; } = new List<string>
        {
            "Январь", "Февраль", "Март", "Апрель",
            "Май", "Июнь", "Июль", "Август",
            "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        };
        // Свойство для хранения выбранного месяца
        private string _selectedMonth;
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged(nameof(SelectedMonth));
            }
        }
        public AccrualChargesViewModel ChargesViewModel
        {
            get
            {
                if (_chargesViewModel == null)
                    _chargesViewModel = new AccrualChargesViewModel();
                return _chargesViewModel;
            }
        }
        public ObservableCollection<Apartment> Apartments
        {
            get => _apartments;
            set
            {
                _apartments = value;
                OnPropertyChanged();
            }
        }

        public Apartment? SelectedApartment
        {
            get => _selectedApartment;
            set
            {
                if (_selectedApartment != value)
                {
                    _selectedApartment = value;
                    OnPropertyChanged(); // ← Это должно быть
                    OnPropertyChanged(nameof(Apartments)); // Обновить список если нужно

                    if (value != null && !IsLoading)
                    {
                        SaveCurrentApartmentAsync();
                    }
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AccrualReportDto> ReportData
        {
            get => _reportData;
            set
            {
                _reportData = value;
                OnPropertyChanged();
            }
        }

        public DateTime ReportStartDate
        {
            get => _reportStartDate;
            set
            {
                _reportStartDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime ReportEndDate
        {
            get => _reportEndDate;
            set
            {
                _reportEndDate = value;
                OnPropertyChanged();
            }
        }

        public OrganizationSettings? OrgSettings
        {
            get => _orgSettings;
            set
            {
                _orgSettings = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            _ = LoadDataAsync();
            _ = LoadOrganizationSettingsAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await _dbService.EnsureDatabaseCreatedAsync();
                var apartments = await _dbService.LoadAllApartmentsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Apartments.Clear();
                    foreach (var apt in apartments)
                    {
                        Apartments.Add(apt);
                    }

                    if (Apartments.Any() && SelectedApartment == null)
                    {
                        SelectedApartment = Apartments.First();
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string errorMessage = $"Ошибка загрузки данных: {ex.Message}";
                    
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                        
                        // Проверяем на временную ошибку подключения
                        if (ex.InnerException.Message.Contains("transient") || 
                            ex.InnerException.Message.Contains("connection") ||
                            ex.InnerException.Message.Contains("timeout") ||
                            ex.InnerException.Message.Contains("08"))
                        {
                            errorMessage += "\n\n" +
                                "Это временная ошибка подключения к базе данных.\n\n" +
                                "Возможные причины:\n" +
                                "1. Сервер PostgreSQL не запущен\n" +
                                "2. Неправильные параметры подключения\n" +
                                "3. Сетевые проблемы\n\n" +
                                "Проверьте, что Docker-контейнер с PostgreSQL запущен:\n" +
                                "docker-compose up -d\n\n" +
                                "Или подождите несколько секунд и попробуйте снова.";
                        }
                    }
                    
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void SaveCurrentApartmentAsync()
        {
            if (SelectedApartment == null || IsLoading) return;

            try
            {
                await _dbService.UpdateApartmentAsync(SelectedApartment);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task AddMeterReplacementAsync(
            string hotWaterMeterNumber,
            DateTime? hotWaterMeterDate,
            int? hotWaterVerificationYear,
            string hotWaterNote,
            string coldWaterMeterNumber,
            DateTime? coldWaterMeterDate,
            int? coldWaterVerificationYear,
            string coldWaterNote)
        {
            if (SelectedApartment == null) return;

            var replacement = new MeterReplacement
            {
                HotWaterMeterNumber = hotWaterMeterNumber ?? "",
                HotWaterMeterDate = hotWaterMeterDate,
                HotWaterVerificationYear = hotWaterVerificationYear,
                HotWaterNote = hotWaterNote ?? "",
                ColdWaterMeterNumber = coldWaterMeterNumber ?? "",
                ColdWaterMeterDate = coldWaterMeterDate,
                ColdWaterVerificationYear = coldWaterVerificationYear,
                ColdWaterNote = coldWaterNote ?? "",
                ApartmentId = SelectedApartment.Id
            };

            await _dbService.AddMeterReplacementAsync(replacement);
            SelectedApartment.MeterReplacements.Add(replacement);
        }

        public async Task AddMeterReadingAsync(DateTime date, double coldWater, double hotWater)
        {
            if (SelectedApartment == null) return;

            var reading = new MeterReading
            {
                Date = date,
                ColdWater = coldWater,
                HotWater = hotWater,
                ApartmentId = SelectedApartment.Id
            };

            await _dbService.AddMeterReadingAsync(reading);
            SelectedApartment.MeterReadings.Add(reading);
        }

        public async Task RemoveMeterReplacementAsync(MeterReplacement replacement)
        {
            if (SelectedApartment == null) return;

            await _dbService.RemoveMeterReplacementAsync(replacement.Id);
            SelectedApartment.MeterReplacements.Remove(replacement);
        }

        public async Task AddPersonalAccountAsync(string accountNumber, string ownerName)
        {
            if (SelectedApartment == null) return;

            var account = new PersonalAccount
            {
                AccountNumber = accountNumber,
                OwnerName = ownerName,
                ApartmentId = SelectedApartment.Id
            };

            await _dbService.AddPersonalAccountAsync(account);
            SelectedApartment.PersonalAccounts.Add(account);
        }

        public async Task RemovePersonalAccountAsync(PersonalAccount account)
        {
            if (SelectedApartment == null) return;

            await _dbService.RemovePersonalAccountAsync(account.Id);
            SelectedApartment.PersonalAccounts.Remove(account);
        }

        public async Task GenerateReportAsync()
        {
            try
            {
                var data = await _dbService.GetAccrualReportDataAsync(ReportStartDate, ReportEndDate);
                ReportData.Clear();
                foreach (var item in data)
                {
                    ReportData.Add(item);
                }

                TotalAmount = ReportData.Sum(r => r.Amount);

                MessageBox.Show($"Сформировано {ReportData.Count} записей на сумму {TotalAmount:N2} руб.",
                    "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        public async Task ExportReportToExcelAsync()
        {
            if (!ReportData.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sb = new System.Text.StringBuilder();

            if (OrgSettings != null)
            {
                sb.AppendLine(OrgSettings.OrganizationName);
                sb.AppendLine($"ИНН: {OrgSettings.Inn} | КПП: {OrgSettings.Kpp}");
                sb.AppendLine($"БИК: {OrgSettings.Bik} | Счет: {OrgSettings.SettlementAccount}");
                sb.AppendLine($"Банк: {OrgSettings.BankName}");
                sb.AppendLine($"Корр. счет: {OrgSettings.CorrespondentAccount}");
                sb.AppendLine($"Код услуги: {OrgSettings.ServiceCode}");
                sb.AppendLine($"Период: {ReportStartDate:dd.MM.yyyy} - {ReportEndDate:dd.MM.yyyy}");
                sb.AppendLine();
            }

            sb.AppendLine("Лицевой счет\tФИО\tАдрес\tСумма\tДата начисления\tКод услуги");

            foreach (var item in ReportData)
            {
                sb.AppendLine($"{item.AccountNumber}\t{item.FullName}\t{item.Address}\t{item.Amount:F2}\t{item.AccrualDate:dd.MM.yyyy}\t{item.ServiceCode}");
            }

            sb.AppendLine();
            sb.AppendLine($"ИТОГО: {TotalAmount:N2} руб.");

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Реестр_начислений_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".txt",
                Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                await System.IO.File.WriteAllTextAsync(dialog.FileName, sb.ToString(),
                    System.Text.Encoding.UTF8);
                MessageBox.Show($"Отчет сохранен: {dialog.FileName}", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async Task RemoveMeterReadingAsync(MeterReading reading)
        {
            if (SelectedApartment == null) return;

            await _dbService.RemoveMeterReadingAsync(reading.Id);
            SelectedApartment.MeterReadings.Remove(reading);
        }

        public async Task LoadOrganizationSettingsAsync()
        {
            try
            {
                OrgSettings = await _dbService.GetOrganizationSettingsAsync();
                if (OrgSettings == null)
                {
                    OrgSettings = new OrganizationSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        public async Task SaveOrganizationSettingsAsync()
        {
            if (OrgSettings == null) return;

            try
            {
                await _dbService.SaveOrganizationSettingsAsync(OrgSettings);
                MessageBox.Show("Настройки сохранены", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        public async Task AddAccrualToAccountAsync(int personalAccountId, decimal amount, string? description = null)
        {
            var accrual = new Accrual
            {
                PersonalAccountId = personalAccountId,
                Amount = amount,
                Description = description,
                ServiceCode = OrgSettings?.ServiceCode ?? "001",
                AccrualDate = DateTime.Today
            };

            await _dbService.AddAccrualAsync(accrual);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}