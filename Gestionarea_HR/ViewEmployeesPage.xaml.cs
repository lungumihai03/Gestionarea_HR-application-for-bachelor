using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Gestionarea_HR
{
    public sealed partial class ViewEmployeesPage : Page
    {
        private string connectionString = "SERVER=localhost; DATABASE=employee; UID=root; PASSWORD=;";

        public class Employee
        {
            public int EmployeeId { get; set; }
            public string Name1 { get; set; }
            public string Name2 { get; set; }
            public string Name3 { get; set; }

        }

        private ObservableCollection<Employee> employees = new ObservableCollection<Employee>();

        public ViewEmployeesPage()
        {
            this.InitializeComponent();

            EmployeeDataGrid.ItemsSource = employees;
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                bool showActiveOnly = CheckBoxFaildate.IsChecked ?? false;
                string searchText = SearchTextBox.Text ?? "";

                Debug.WriteLine($"Loading employees with search: '{searchText}', ShowActiveOnly: {showActiveOnly}");

                employees.Clear();

                GetEmployees(searchText, showActiveOnly);

                Debug.WriteLine($"Employees loaded: {employees.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadEmployees: {ex.Message}");
                ShowError($"Error loading employees: {ex.Message}");
            }
        }

        private void GetEmployees(string search, bool showActiveOnly)
        {
            try
            {
                using (MySqlConnection connect = new MySqlConnection(connectionString))
                {
                    connect.Open();

                    string query = @"SELECT 
                                        employeeid, 
                                        name1, 
                                        name2, 
                                        name3 
                                    FROM 
                                        employee.employee 
                                    WHERE 
                                        1=1 ";

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        query += "AND (name1 LIKE @search OR name2 LIKE @search OR name3 LIKE @search) ";
                    }

                    if (showActiveOnly)
                    {
                        query += "AND faildate IS NULL ";
                    }

                    Debug.WriteLine($"Query: {query}");

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Employee emp = new Employee
                                    {
                                        EmployeeId = reader.GetInt32("employeeid"),
                                        Name1 = reader.IsDBNull(reader.GetOrdinal("name1")) ? "" : reader.GetString("name1"),
                                        Name2 = reader.IsDBNull(reader.GetOrdinal("name2")) ? "" : reader.GetString("name2"),
                                        Name3 = reader.IsDBNull(reader.GetOrdinal("name3")) ? "" : reader.GetString("name3")
                                    };

                                    employees.Add(emp);
                                    Debug.WriteLine($"Added employee: ID={emp.EmployeeId}, Name1={emp.Name1}, Name2={emp.Name2}, Name3={emp.Name3}");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("No employees found matching the criteria");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetEmployees: {ex.Message}");
                if (ex is MySqlException mysqlEx)
                {
                    Debug.WriteLine($"MySQL Error: {mysqlEx.Number}, State: {mysqlEx.SqlState}");
                }
                ShowError($"Database error: {ex.Message}");
            }
        }

        private async void ShowError(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Eroare",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadEmployees();
        }

        private void CheckBoxFaildate_Checked(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeeDataGrid.SelectedItem is Employee selectedEmployee)
                {
                    int id = selectedEmployee.EmployeeId;
                    Debug.WriteLine($"ID = {id}");

                    EditEmployeeDialog dialog = new EditEmployeeDialog(id);
                    dialog.XamlRoot = this.XamlRoot;

                    await dialog.ShowAsync();
                    LoadEmployees();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }




    }
}