using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gestionarea_HR
{
    public sealed partial class EditEmployeeDialog : ContentDialog
    {
        public Employee EmployeeData { get; private set; }

        public EditEmployeeDialog(int employeeId)
        {
            this.InitializeComponent();
            FindEmployeeByIdAsync(employeeId).Wait();
        }





        private async Task<Employee> FindEmployeeByIdAsync(int employeeId)
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                await connect.OpenAsync();
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
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())

                    {
                        if (await reader.ReadAsync())
                        {
                            Name1TextBox.Text = reader.IsDBNull(reader.GetOrdinal("name1")) ? "" : reader.GetString("name1");
                            Name2TextBox.Text = reader.IsDBNull(reader.GetOrdinal("name2")) ? "" : reader.GetString("name2");
                            Name3TextBox.Text = reader.IsDBNull(reader.GetOrdinal("name3")) ? "" : reader.GetString("name3");
                            PhoneTextBox.Text = reader.IsDBNull(reader.GetOrdinal("phone")) ? "" : reader.GetString("phone");
                            BirthdatePicker.Date = reader.IsDBNull(reader.GetOrdinal("birthdate"))
                                ? DateTimeOffset.Now
                                : new DateTimeOffset(reader.GetDateTime("birthdate"));
                            HiredatePicker.Date = reader.IsDBNull(reader.GetOrdinal("hiredate"))
                                ? DateTimeOffset.Now
                                : new DateTimeOffset(reader.GetDateTime("hiredate"));
                            ContractDatePicker.Date = reader.IsDBNull(reader.GetOrdinal("contract_date"))
                                ? DateTimeOffset.Now
                                : new DateTimeOffset(reader.GetDateTime("contract_date"));

                            if (reader.IsDBNull(reader.GetOrdinal("faildate")))
                            {
                                FaildatePicker.SelectedDate = null;
                            }
                            else
                            {
                                FaildatePicker.Date = new DateTimeOffset(reader.GetDateTime("faildate"));
                            }

                            SalaryTextBox.Text = reader.IsDBNull(reader.GetOrdinal("salary")) ? "0" : reader.GetInt32("salary").ToString();
                            ChildrenTextBox.Text = reader.IsDBNull(reader.GetOrdinal("children")) ? "0" : reader.GetInt32("children").ToString();
                            MarriedTextBox.Text = reader.IsDBNull(reader.GetOrdinal("married")) ? "0" : reader.GetInt32("married").ToString();
                            CNPTextBox.Text = reader.IsDBNull(reader.GetOrdinal("CNP")) ? "" : reader.GetString("CNP");
                            CPASTextBox.Text = reader.IsDBNull(reader.GetOrdinal("CPAS")) ? "" : reader.GetString("CPAS");
                            ContractNrTextBox.Text = reader.IsDBNull(reader.GetOrdinal("contract")) ? "" : reader.GetString("contract");

                            SelectComboBoxItem(SpecialityTextBox, reader.IsDBNull(reader.GetOrdinal("speciality_desc")) ? "" : reader.GetString("speciality_desc"));
                            SelectComboBoxItem(EducationTextBox, reader.IsDBNull(reader.GetOrdinal("education_desc")) ? "" : reader.GetString("education_desc"));
                            SelectComboBoxItem(DepartTextBox, reader.IsDBNull(reader.GetOrdinal("depart_desc")) ? "" : reader.GetString("depart_desc"));
                        }
                    }
                }
            }
            return EmployeeData;
        }


        private void SelectComboBoxItem(ComboBox comboBox, string description)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == description)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeData = GetUpdatedEmployee();
            this.Hide();
        }

        public Employee GetUpdatedEmployee()
        {
            EmployeeData.name1 = Name1TextBox.Text;
            EmployeeData.name2 = Name2TextBox.Text;
            EmployeeData.name3 = Name3TextBox.Text;
            EmployeeData.phone = PhoneTextBox.Text;
            EmployeeData.birthdate = BirthdatePicker.Date.DateTime;
            EmployeeData.hiredate = HiredatePicker.Date.DateTime;
            EmployeeData.speciality = SpecialityTextBox.Text;
            EmployeeData.salary = int.Parse(SalaryTextBox.Text);
            EmployeeData.education = EducationTextBox.Text;
            EmployeeData.children = int.Parse(ChildrenTextBox.Text);
            EmployeeData.married = int.Parse(MarriedTextBox.Text);
            EmployeeData.depart = DepartTextBox.Text;
            EmployeeData.faildate = FaildatePicker.Date.DateTime;
            EmployeeData.contract_nr = ContractNrTextBox.Text;
            EmployeeData.contract_date = ContractDatePicker.Date.DateTime;
            EmployeeData.CNP = CNPTextBox.Text;
            EmployeeData.CPAS = CPASTextBox.Text;

            return EmployeeData;
        }
    }
}
