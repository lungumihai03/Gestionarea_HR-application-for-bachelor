using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Gestionarea_HR
{
    public partial class FisaAngajat : Page
    {
        private Employee employeed = new Employee();
        public FisaAngajat()
        {
            InitializeComponent();
            LoadEmployees();
            LoadEducation();
            LoadDepartments();
        }

        private void LoadEmployees()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT employeeid, CONCAT(name1, ' ', name3) AS full_name FROM employee";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                employeeComboBox.Items.Clear();

                while (reader.Read())
                {
                    string fullName = reader["full_name"].ToString();
                    int employeeId = reader.GetInt32("employeeid");

                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = fullName,
                        Tag = employeeId
                    };

                    employeeComboBox.Items.Add(item);
                }
            }
        }

        private void EmployeeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditButton.IsEnabled = true;
            if (employeeComboBox.SelectedItem != null)
            {
                var selectedItem = (ComboBoxItem)employeeComboBox.SelectedItem;
                int employeeId = (int)selectedItem.Tag;
                LoadEmployeeData(employeeId);
                
            }
        }
        private void LoadEducation()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT code, descript_r FROM educat";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    EducationComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = reader["descript_r"].ToString(),
                        Tag = reader["code"].ToString()
                    });
                }
            }
        }

        private void LoadDepartments()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT code, descript_r FROM depart";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DepartmentComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = reader["descript_r"].ToString(),
                        Tag = reader["code"].ToString()
                    });
                }
            }
        }
        private void LoadEmployeeData(int employeeId)
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                connect.Open();
                string query = @"SELECT 
                           e.employeeid, e.name1, e.name2, e.name3, e.phone, e.birthdate, e.hiredate, 
                           e.speciality, e.salary, e.education, e.children, e.married, e.faildate, 
                           e.depart, e.ordin, e.idcod AS CNP, e.cpas AS CPAS, e.contract, e.contr_data AS contract_date,
                           s.descript_r AS speciality_desc, ed.descript_r AS education_desc, d.descript_r AS depart_desc
                         FROM employee.employee e
                         LEFT JOIN specties s ON e.speciality = s.code
                         LEFT JOIN educat ed ON e.education = ed.code
                         LEFT JOIN depart d ON e.depart = d.code
                         WHERE e.employeeid = @employeeId";

                using (MySqlCommand cmd = new MySqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@employeeId", employeeId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            EmployeeIDTextBox.Text = reader["employeeid"].ToString();
                            Name1TextBox.Text = reader["name1"].ToString();
                            Name2TextBox.Text = reader["name2"].ToString();
                            Name3TextBox.Text = reader["name3"].ToString();
                            PhoneTextBox.Text = reader["phone"].ToString();
                            SalaryTextBox.Text = reader["salary"].ToString();
                            ChildrenTextBox.Text = reader["children"].ToString();
                            CNPTextBox.Text = reader["CNP"].ToString();
                            CPASTextBox.Text = reader["CPAS"].ToString();
                            
                            MarriedTextBox.Text = reader["married"].ToString();
                            SetComboBoxSelection(EducationComboBox, reader["education_desc"].ToString());
                            SetComboBoxSelection(DepartmentComboBox, reader["depart_desc"].ToString());
                            BirthDatePicker.SelectedDate = reader.IsDBNull(reader.GetOrdinal("birthdate"))
     ? (DateTime?)null
     : DateTime.ParseExact(reader["birthdate"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        }
                    }
                }
            }
            EditButton.IsEnabled = true;
        }

        private void SetComboBoxSelection(ComboBox comboBox, string selectedDescription)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == selectedDescription)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode();
        }

        private void SetEditMode()
        {
            Name1TextBox.IsReadOnly = false;
            Name2TextBox.IsReadOnly = false;
            Name3TextBox.IsReadOnly = false;
            PhoneTextBox.IsReadOnly = false;
            SalaryTextBox.IsReadOnly = false;
            ChildrenTextBox.IsReadOnly = false;
            CPASTextBox.IsReadOnly = false;
            MarriedTextBox.IsReadOnly = false;
            SaveButton.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveEmployeeData();
            SaveButton.Visibility = Visibility.Collapsed;
        }

        private void SaveEmployeeData()
        {
            employeed.children = Convert.ToInt32(ChildrenTextBox.Text);
            employeed.CPAS = CPASTextBox.Text;
            employeed.depart = DepartmentComboBox.Text;
            employeed.education = EducationComboBox.Text;
            employeed.employeeid = Convert.ToInt64(EmployeeIDTextBox.Text);
            employeed.married = Convert.ToInt32(MarriedTextBox.Text);
            employeed.name1 = Name1TextBox.Text;
            employeed.name2 = Name2TextBox.Text;
            employeed.name3 = Name3TextBox.Text;
            employeed.phone = PhoneTextBox.Text;
            employeed.salary = Convert.ToInt32(SalaryTextBox.Text);
            string connectionString;
            connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            string query = @"
    UPDATE employee 
    JOIN educat ON employee.education = educat.code 
    JOIN depart ON employee.depart = depart.code
    SET employee.name1 = @name1, 
        employee.name2 = @name2, 
        employee.name3 = @name3, 
        employee.phone = @phone, 
        employee.salary = @salary, 
        employee.children = @children, 
        employee.married = @married, 
        employee.cpas = @CPAS 
    WHERE employee.employeeid = @employeeId ;";

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                connect.Open();
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@name1", employeed.name1);
                        cmd.Parameters.AddWithValue("@name2", employeed.name2);
                        cmd.Parameters.AddWithValue("@name3", employeed.name3);
                        cmd.Parameters.AddWithValue("@phone", employeed.phone);
                        cmd.Parameters.AddWithValue("@birthdate", employeed.birthdate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@salary", employeed.salary);
                        cmd.Parameters.AddWithValue("@children", employeed.children);
                        cmd.Parameters.AddWithValue("@married", employeed.married);
                        cmd.Parameters.AddWithValue("@CNP", employeed.CNP);
                        cmd.Parameters.AddWithValue("@CPAS", employeed.CPAS);
                        cmd.Parameters.AddWithValue("@employeeId", employeed.employeeid);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Debug.WriteLine("Employee ID: " + employeed.employeeid);
                        Debug.WriteLine("Education: " + employeed.education);
                        Debug.WriteLine("Department: " + employeed.depart);

                        if (rowsAffected > 0)
                            ShowMessage($"Informatia despre {employeed.name3} {employeed.name1} a fost actualizata cu succes.", "Success");
                        else
                            Debug.WriteLine("No records updated. Check employee ID.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
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
    }
}
