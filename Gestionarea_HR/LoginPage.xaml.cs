using Gestionarea_HR;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using MySql.Data.MySqlClient;
using System;

namespace Gestionarea_HR
{
    public partial class LoginPage : Page
    { 
        private string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
        private string? pass;
        private string? rights;
        private int? onetime;

        public LoginPage()
        {
            this.InitializeComponent();

        }

        private void LoadData(string userid)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM employee.users WHERE Login='{userid}'";
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pass = reader["Password"].ToString();
                        rights = reader["Rights"].ToString();
                        onetime = Convert.ToInt32(reader["OneTime"]);
                    }
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AppNotification notification = new AppNotificationBuilder()
        .AddText("Welcome to Gestionarea HR")
        .AddText("Aplicatia scrisa pentru teza de licenta")
        .BuildNotification();

            AppNotificationManager.Default.Show(notification);
            string userid = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            LoadData(userid);
            if (password == pass && onetime == 0)
            {
                if (this.Frame != null)
                {
                    this.Frame.Navigate(typeof(AdminPanelPage), rights);
                }
            }
            else if (password != pass)
            {
                ShowMessage("Login sau parolă incorectă!", "Eroare");
            }
            else if (password == pass && onetime == 1)
            {
                ShowMessage("Parola trebuie schimbată!", "Notificare");
                Frame.Navigate(typeof(ChangePasswordPage), userid); 
            }
        }

        private void ShowMessage(string content, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void PasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginButton_Click(this, new RoutedEventArgs());
            }
        }
    }
}
