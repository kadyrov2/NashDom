using System;
using System.Windows;
using NashDom.ViewModels;
using NashDom.Models;

namespace NashDom.Views
{
    public partial class SettingsWindow : Window
    {
        private MainViewModel _viewModel;

        public SettingsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (_viewModel.OrgSettings != null)
            {
                TemplateNameBox.Text = _viewModel.OrgSettings.TemplateName;
                OrganizationNameBox.Text = _viewModel.OrgSettings.OrganizationName;
                InnBox.Text = _viewModel.OrgSettings.Inn;
                KppBox.Text = _viewModel.OrgSettings.Kpp;
                BikBox.Text = _viewModel.OrgSettings.Bik;
                CorrespondentAccountBox.Text = _viewModel.OrgSettings.CorrespondentAccount;
                BankNameBox.Text = _viewModel.OrgSettings.BankName;
                SettlementAccountBox.Text = _viewModel.OrgSettings.SettlementAccount;
                ServiceCodeBox.Text = _viewModel.OrgSettings.ServiceCode;
                HeaderTextBox.Text = _viewModel.OrgSettings.HeaderText ?? "";
                FooterTextBox.Text = _viewModel.OrgSettings.FooterText ?? "";
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.OrgSettings == null)
            {
                _viewModel.OrgSettings = new OrganizationSettings();
            }

            _viewModel.OrgSettings.TemplateName = TemplateNameBox.Text;
            _viewModel.OrgSettings.OrganizationName = OrganizationNameBox.Text;
            _viewModel.OrgSettings.Inn = InnBox.Text;
            _viewModel.OrgSettings.Kpp = KppBox.Text;
            _viewModel.OrgSettings.Bik = BikBox.Text;
            _viewModel.OrgSettings.CorrespondentAccount = CorrespondentAccountBox.Text;
            _viewModel.OrgSettings.BankName = BankNameBox.Text;
            _viewModel.OrgSettings.SettlementAccount = SettlementAccountBox.Text;
            _viewModel.OrgSettings.ServiceCode = ServiceCodeBox.Text;
            _viewModel.OrgSettings.HeaderText = HeaderTextBox.Text;
            _viewModel.OrgSettings.FooterText = FooterTextBox.Text;

            await _viewModel.SaveOrganizationSettingsAsync();
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