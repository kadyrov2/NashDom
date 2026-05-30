using System.Collections.Generic;
using System.Windows;
using NashDom.ViewModels;
using NashDom.Models;

namespace NashDom.Views
{
    public partial class AccrualWindow : Window
    {
        private MainViewModel _viewModel;
        private PersonalAccount _selectedAccount;

        // Конструктор для выбранного счета
        public AccrualWindow(MainViewModel viewModel, PersonalAccount account)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _selectedAccount = account;

            AccountCombo.ItemsSource = new List<PersonalAccount> { account };
            AccountCombo.SelectedIndex = 0;
            AccountCombo.IsEnabled = false; // Блокируем выбор счета
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму начисления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var description = DescriptionBox.Text;

            await _viewModel.AddAccrualToAccountAsync(_selectedAccount.Id, amount, description);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}