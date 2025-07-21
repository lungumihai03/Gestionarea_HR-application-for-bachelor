using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using MySql.Data.MySqlClient;

namespace Gestionarea_HR
{
    public sealed partial class SettingsPage : Page
    {
        double sp, sc, iv_p, med_p;
        public SettingsPage()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM employee.calc_salariu";
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sp = Convert.ToDouble(reader["sp"]);
                        sc = Convert.ToDouble(reader["sc"]);
                        iv_p = Convert.ToDouble(reader["iv"]);
                        med_p = Convert.ToDouble(reader["im"]);
                    }
                }
            }
            PersonalExemptionTextBox.Text = sp.ToString();
            ChildExemptionTextBox.Text = sc.ToString();
            IncomeTaxTextBox.Text = iv_p.ToString();
            HealthTaxTextBox.Text = med_p.ToString();
        }
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"UPDATE employee.calc_salariu SET sp = {PersonalExemptionTextBox.Text}, sc = {ChildExemptionTextBox.Text}, iv = {IncomeTaxTextBox.Text}, im = {HealthTaxTextBox.Text} WHERE id = 1";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            ShowMessage("Setările au fost salvate cu succes!");
            LoadSettings();

        }
        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Setări",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
