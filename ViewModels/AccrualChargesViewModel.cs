using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using NashDom.Models;
using NashDom.Services;
using NashDom.Helpers;

namespace NashDom.ViewModels
{
    public class AccrualChargesViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        private ObservableCollection<AccrualChargeDto> _charges = new();
        private AccrualChargeDto? _selectedCharge;
        private bool _isLoading;
        private DateTime _selectedMonth = new DateTime(2026, 4, 1);
        private decimal _totalAccruedSum;
        private decimal _totalPaidSum;
        private decimal _totalDebtSum;
        private string _importStatus = string.Empty;
        private decimal _totalPenaltySum;

        public decimal TotalPenaltySum
        {
            get => _totalPenaltySum;
            set
            {
                _totalPenaltySum = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<AccrualChargeDto> Charges
        {
            get => _charges;
            set
            {
                _charges = value;
                OnPropertyChanged();
            }
        }

        public AccrualChargeDto? SelectedCharge
        {
            get => _selectedCharge;
            set
            {
                _selectedCharge = value;
                OnPropertyChanged();
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

        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
                // Загружаем данные при смене месяца
                Task.Run(async () => await LoadChargesForMonthAsync());
            }
        }

        public decimal TotalAccruedSum
        {
            get => _totalAccruedSum;
            set
            {
                _totalAccruedSum = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalPaidSum
        {
            get => _totalPaidSum;
            set
            {
                _totalPaidSum = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalDebtSum
        {
            get => _totalDebtSum;
            set
            {
                _totalDebtSum = value;
                OnPropertyChanged();
            }
        }

        public string ImportStatus
        {
            get => _importStatus;
            set
            {
                _importStatus = value;
                OnPropertyChanged();
            }
        }

        // Команды
        public ICommand LoadChargesForMonthCommand { get; }
        public ICommand SaveChargeCommand { get; }
        public ICommand RefreshCommand { get; }

        public AccrualChargesViewModel()
        {
            _dbService = new DatabaseService();

            LoadChargesForMonthCommand = new RelayCommand(async _ => await LoadChargesForMonthAsync());
            SaveChargeCommand = new RelayCommand(async _ => await SaveChargeAsync(), _ => SelectedCharge != null);
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        }

  

        public async Task LoadChargesForMonthAsync()
        {
            try
            {
                IsLoading = true;
                ImportStatus = "Загрузка данных...";

                var charges = await _dbService.GetAccrualChargesByMonthAsync(SelectedMonth);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Charges.Clear();
                    foreach (var charge in charges.OrderBy(c => c.ApartmentNumber))
                    {
                        Charges.Add(charge);
                    }

                    CalculateTotals();
                    ImportStatus = $"Загружено {Charges.Count} записей за {SelectedMonth:MMMM yyyy}";
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ImportStatus = $"Ошибка загрузки: {ex.Message}";
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateTotals()
        {
            TotalAccruedSum = Charges.Sum(c => c.TotalAccrued ?? 0);
            TotalPaidSum = Charges.Sum(c => c.PaidAmount ?? 0);
            TotalDebtSum = Charges.Sum(c => (c.TotalAccrued ?? 0) - (c.PaidAmount ?? 0));
            TotalPenaltySum = Charges.Sum(c => c.Penalty ?? 0);   
        }

        public async Task SaveChargeAsync()
        {
            if (SelectedCharge == null) return;

            try
            {
                IsLoading = true;
                await _dbService.UpdateAccrualChargeAsync(SelectedCharge, SelectedMonth);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Данные сохранены", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });

                await LoadChargesForMonthAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshAsync()
        {
            await LoadChargesForMonthAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}