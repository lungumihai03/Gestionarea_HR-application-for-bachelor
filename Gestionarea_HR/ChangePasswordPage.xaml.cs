using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;
using System;
using Gestionarea_HR;
namespace Gestionarea_HR
{
    public sealed partial class ChangePasswordPage : Page
    {
        private string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
        private string user;

        public ChangePasswordPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string userId)
            {
                user = userId;
            }
        }

        public void ShowPage(string userid)
        {
            user = userid;
        }

        private bool PassTest(string password)
        {
            bool hasLower = false;
            bool hasUpper = false;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (char.IsLower(c)) hasLower = true;
                if (char.IsUpper(c)) hasUpper = true;
                if (char.IsDigit(c)) hasDigit = true;
            }

            return hasLower && hasUpper && hasDigit;
        }

        private void CheckFields()
        {
            ChangePasswordButton.IsEnabled = NewPasswordTextBox.Password.Length >= 8
                && ConfirmPasswordTextBox.Password.Length >= 8
                && NewPasswordTextBox.Password == ConfirmPasswordTextBox.Password;
        }


        private void NewPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckFields();
        }

        private void ConfirmPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckFields();
        }


        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewPasswordTextBox.Password != ConfirmPasswordTextBox.Password)
            {
                await ShowMessage("Parolele nu se potrivesc!", "Eroare");
                return;
            }

            if (!PassTest(NewPasswordTextBox.Password))
            {
                await ShowMessage("Parola nu este suficient de sigură! (Minim o MAJUSCULĂ, o literă mică și o cifră)", "Eroare");
                NewPasswordTextBox.Password = "";
                ConfirmPasswordTextBox.Password = "";
                ChangePasswordButton.IsEnabled = false;
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE employee.users SET OneTime=0, Password='{NewPasswordTextBox.Password}' WHERE Login='{user}'";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                await ShowMessage("Parola a fost schimbată cu succes!", "Succes");
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await ShowMessage($"Eroare la actualizarea parolei: {ex.Message}", "Eroare");
            }
        }

        private async System.Threading.Tasks.Task ShowMessage(string content, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
