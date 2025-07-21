using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using Gestionarea_HR;

namespace Gestionarea_HR
{
    public sealed partial class UserManagementPage : Page
    {
        private string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        private string selectedUserId;

        public UserManagementPage()
        {
            this.InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            Users.Clear();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Login, Rights, OneTime as OneTimePassword FROM users";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Users.Add(new User
                        {
                            Login = reader["Login"].ToString(),
                            Rights = reader["Rights"].ToString(),
                            OneTimePassword = reader["OneTimePassword"].ToString()
                        });
                    }
                }
            }
            UsersDataGrid.ItemsSource = Users;
        }

        private void FieldsChanged(object sender, RoutedEventArgs e)
        {
            AddUserButton.IsEnabled =
                !string.IsNullOrWhiteSpace(LoginTextBox.Text) &&
                !string.IsNullOrWhiteSpace(PasswordBox.Password) &&
                AccessRightsComboBox.SelectedItem != null;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO users (Login, Password, Rights, OneTime) VALUES (@Login, @Password, @Rights, 1)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Login", LoginTextBox.Text);
                cmd.Parameters.AddWithValue("@Password", PasswordBox.Password);
                cmd.Parameters.AddWithValue("@Rights", ((ComboBoxItem)AccessRightsComboBox.SelectedItem).Content.ToString());

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageDialog("Utilizator adăugat cu succes.");
                }
                catch (MySqlException ex)
                {
                    MessageDialog($"Eroare la adăugarea utilizatorului: {ex.Message}");
                }
                finally
                {
                    ClearFields();
                    LoadUsers();
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedUserId))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM users WHERE Login = @Login";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", selectedUserId);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageDialog("Utilizator șters cu succes.");
                    }
                    catch (MySqlException ex)
                    {
                        MessageDialog($"Eroare la ștergerea utilizatorului: {ex.Message}");
                    }
                    finally
                    {
                        ClearFields();
                        LoadUsers();
                    }
                }
            }
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                selectedUserId = selectedUser.Login;
                DeleteUserButton.IsEnabled = true;
            }
            else
            {
                selectedUserId = null;
                DeleteUserButton.IsEnabled = false;
            }
        }

        private void ClearFields()
        {
            LoginTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            AccessRightsComboBox.SelectedIndex = -1;
            AddUserButton.IsEnabled = false;
        }

        private async void MessageDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Informație",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    public class User
    {
        public string Login { get; set; }
        public string Rights { get; set; }
        public string OneTimePassword { get; set; }
    }
}
