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
using Gestionarea_HR;

namespace Gestionarea_HR
{

    public sealed partial class EmployeePage : Page
    {
        private int? navigatedEmployeeId = null;

        private Employee employeed = new Employee();
        //string ChildrenTextBox, DepartTextBox, ;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int employeeId)
            {
                navigatedEmployeeId = employeeId;
                LoadEmployeeData(employeeId);
                
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
                            // Populate form fields
                            EmployeeIDTextBox.Text = reader["employeeid"].ToString();
                            Name1TextBox.Text = reader.IsDBNull(reader.GetOrdinal("name1")) ? "" : reader.GetString("name1");
                            Name2TextBox.Text = reader.IsDBNull(reader.GetOrdinal("name2")) ? "" : reader.GetString("name2");
                            Name3TextBox.Text = reader.IsDBNull(reader.GetOrdinal("name3")) ? "" : reader.GetString("name3");
                            PhoneTextBox.Text = reader.IsDBNull(reader.GetOrdinal("phone")) ? "" : reader.GetString("phone");

                            // Handle BirthDatePicker, HireDatePicker, ContractDatePicker
                            BirthDatePicker.Date = reader.IsDBNull(reader.GetOrdinal("birthdate"))
                                ? DateTimeOffset.Now
                                : new DateTimeOffset(reader.GetDateTime("birthdate"));
                            HireDatePicker.Date = reader.IsDBNull(reader.GetOrdinal("hiredate"))
                                ? DateTimeOffset.Now
                                : new DateTimeOffset(reader.GetDateTime("hiredate"));
                            ContractDatePicker.Date = reader.IsDBNull(reader.GetOrdinal("contract_date"))
                                ? DateTimeOffset.Now
                                : new DateTimeOffset(reader.GetDateTime("contract_date"));

                            // Handle FailDatePicker (allow null)
                            if (reader.IsDBNull(reader.GetOrdinal("faildate")))
                            {
                                FailDatePicker.SelectedDate = null; // Clear the selection
                            }
                            else
                            {
                                FailDatePicker.Date = new DateTimeOffset(reader.GetDateTime("faildate"));
                            }

                            SalaryTextBox.Text = reader.IsDBNull(reader.GetOrdinal("salary")) ? "0" : reader.GetInt32("salary").ToString();
                            ChildrenTextBox.Text = reader.IsDBNull(reader.GetOrdinal("children")) ? "0" : reader.GetInt32("children").ToString();
                            MarriedTextBox.Text = reader.IsDBNull(reader.GetOrdinal("married")) ? "0" : reader.GetInt32("married").ToString();
                            IDNPTextBox.Text = reader.IsDBNull(reader.GetOrdinal("CNP")) ? "" : reader.GetString("CNP");
                            CPASTextBox.Text = reader.IsDBNull(reader.GetOrdinal("CPAS")) ? "" : reader.GetString("CPAS");
                            ContractTextBox.Text = reader.IsDBNull(reader.GetOrdinal("contract")) ? "" : reader.GetString("contract");

                            // Set ComboBox selections
                            SelectComboBoxItem(SpecialityComboBox, reader.IsDBNull(reader.GetOrdinal("speciality_desc")) ? "" : reader.GetString("speciality_desc"));
                            SelectComboBoxItem(EducationComboBox, reader.IsDBNull(reader.GetOrdinal("education_desc")) ? "" : reader.GetString("education_desc"));
                            SelectComboBoxItem(DepartComboBox, reader.IsDBNull(reader.GetOrdinal("depart_desc")) ? "" : reader.GetString("depart_desc"));
                        }
                    }
                }
            }
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
       
        public EmployeePage()
        {
            this.InitializeComponent();
            LoadSpeciality();
            LoadEducation();
            LoadDepartments();
        }
        private void LoadSpeciality()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT DISTINCT code, descript_r FROM specties";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                // Clear existing items
                SpecialityComboBox.Items.Clear();

                HashSet<string> uniqueSpecialities = new HashSet<string>();

                while (reader.Read())
                {
                    string speciality = reader["descript_r"].ToString();
                    if (!uniqueSpecialities.Contains(speciality))
                    {
                        uniqueSpecialities.Add(speciality);
                        SpecialityComboBox.Items.Add(new ComboBoxItem
                        {
                            Content = speciality,
                            Tag = reader["code"].ToString()
                        });
                    }
                }
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
                    DepartComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = reader["descript_r"].ToString(),
                        Tag = reader["code"].ToString()
                    });
                }
            }
        }


        public void ShowForm(int k)
        {
            if (k == 1)
            {
               ChildrenTextBox.IsReadOnly=false;
                DepartComboBox.IsEnabled = false;
                EducationComboBox.IsEnabled = false;
                // Missing: textBox_forder.ReadOnly = true;
                MarriedTextBox.IsReadOnly = false;
                Name1TextBox.IsReadOnly = false;
                Name2TextBox.IsReadOnly = false;
                Name3TextBox.IsReadOnly = false;
                // Missing: textBox_ordin.ReadOnly = false;
                PhoneTextBox.IsReadOnly = false;
                SalaryTextBox.IsReadOnly = false;
                SpecialityComboBox.IsEnabled = false;
                CPASTextBox.IsReadOnly = false;
                IDNPTextBox.IsReadOnly = false;
                EmployeeIDTextBox.IsReadOnly = true;

                // Handle buttons
               
                CreateButton.Visibility = Visibility.Visible;

                // Handle DatePickers
                FailDatePicker.IsEnabled = false;
                BirthDatePicker.IsEnabled = true;
                ContractDatePicker.IsEnabled = true;
                HireDatePicker.IsEnabled = true;
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ChildrenTextBox.IsReadOnly = false;
            DepartComboBox.IsEnabled = false;
            EducationComboBox.IsEnabled = false;
            // Missing: textBox_forder.ReadOnly = false;
            MarriedTextBox.IsReadOnly = false;
            Name1TextBox.IsReadOnly = false;
            Name2TextBox.IsReadOnly = false;
            Name3TextBox.IsReadOnly = false;
            // Missing: textBox_ordin.ReadOnly = false;
            PhoneTextBox.IsReadOnly = false;
            SalaryTextBox.IsReadOnly = false;
            SpecialityComboBox.IsEnabled = false;
            CPASTextBox.IsReadOnly = true;
            IDNPTextBox.IsReadOnly = true;
            EmployeeIDTextBox.IsReadOnly = true;

            // Missing: button_save.Visible = true;
            FailDatePicker.IsEnabled = true;
            BirthDatePicker.IsEnabled = true;
            ContractDatePicker.IsEnabled = true;
            HireDatePicker.IsEnabled = true;
            
        }

       

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {

            Application.Current.Exit();
        }

        private void TestForFill()
        {
            if (BirthDatePicker.Date.ToString() != "1/1/2000" &&
                ContractDatePicker.Date.ToString() != "1/1/2000" &&
                HireDatePicker.Date.ToString() != "1/1/2000" &&
                ChildrenTextBox.Text.Length != 0 &&
                ContractTextBox.Text.Length != 0 &&
                CPASTextBox.Text.Length != 0 &&
                DepartComboBox.Text.Length != 0 &&
                EducationComboBox.Text.Length != 0 &&
                IDNPTextBox.Text.Length != 0 &&
                MarriedTextBox.Text.Length != 0 &&
                Name1TextBox.Text.Length != 0 &&
                Name2TextBox.Text.Length != 0 &&
                Name3TextBox.Text.Length != 0 &&
                PhoneTextBox.Text.Length != 0 &&
                SalaryTextBox.Text.Length != 0 &&
                SpecialityComboBox.Text.Length != 0)
            {
                CreateButton.IsEnabled = true;
            }
        }


        private void SpecialityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpecialityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                employeed.speciality = selectedItem.Tag.ToString();
                System.Diagnostics.Debug.WriteLine($"Selected Speciality Code: {employeed.speciality}");
            }
        }

        private void EducationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EducationComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                employeed.education = selectedItem.Tag.ToString();
                Console.WriteLine($"Selected Education Code: {employeed.education}");
            }
        }

        private void DepartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                employeed.depart = selectedItem.Tag.ToString();
                Console.WriteLine($"Selected Department Code: {employeed.depart}");
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TestForFill();
        }

        private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            TestForFill();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (BirthDatePicker.Date.ToString() == "1/1/2000" ||
                    ContractDatePicker.Date.ToString() == "1/1/2000" ||
                    HireDatePicker.Date.ToString() == "1/1/2000" ||
                    ChildrenTextBox.Text.Length == 0 ||
                    ContractTextBox.Text.Length == 0 ||
                    CPASTextBox.Text.Length == 0 ||
                    DepartComboBox.Text.Length == 0 ||
                    EducationComboBox.Text.Length == 0 ||
                    IDNPTextBox.Text.Length == 0 ||
                    MarriedTextBox.Text.Length == 0 ||
                    Name1TextBox.Text.Length == 0 ||
                    Name2TextBox.Text.Length == 0 ||
                    Name3TextBox.Text.Length == 0 ||
                    PhoneTextBox.Text.Length == 0 ||
                    SalaryTextBox.Text.Length == 0 ||
                    SpecialityComboBox.Text.Length == 0)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Please fill in all fields",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

           
            employeed.children = Convert.ToInt32(ChildrenTextBox.Text);
            employeed.birthdate = BirthDatePicker.Date.DateTime;
            employeed.faildate = FailDatePicker.Date.DateTime;
            employeed.hiredate = HireDatePicker.Date.DateTime;
            employeed.contract_date = ContractDatePicker.Date.DateTime;
            employeed.CNP = IDNPTextBox.Text;
            employeed.contract_nr = ContractTextBox.Text;
            employeed.CPAS = CPASTextBox.Text;
            if (DepartComboBox.SelectedItem is ComboBoxItem selectedItemD)
            {
                employeed.depart = selectedItemD.Tag.ToString();
            }
                if (EducationComboBox.SelectedItem is ComboBoxItem selectedItemE)
            {
                employeed.education = selectedItemE.Tag.ToString();
            }
            employeed.married = Convert.ToInt32(MarriedTextBox.Text);
            employeed.name1 = Name1TextBox.Text;
            employeed.name2 = Name2TextBox.Text;
            employeed.name3 = Name3TextBox.Text;
            employeed.phone = PhoneTextBox.Text;
            employeed.salary = Convert.ToInt32(SalaryTextBox.Text);
            if (SpecialityComboBox.SelectedItem is ComboBoxItem selectedItemS)
            {
                employeed.speciality = selectedItemS.Tag.ToString();
            }
                Console.WriteLine("Selected Speciality Code: " + employeed.speciality);
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                connect.Open();
                try
                {
                    MySqlCommand cmd = connect.CreateCommand();
                    cmd.CommandText = "SELECT s.code as CODE_SP, e.code as CODE_ED, d.code as CODE_DPT FROM specties s, educat e, depart d where s.descript_r = '" +
                        SpecialityComboBox.Text + "' and e.descript_r ='" +
                        EducationComboBox.Text + "' and d.descript_r ='" +
                        DepartComboBox.Text + "';";

                    MySqlDataAdapter adap = new MySqlDataAdapter(cmd);
                    DataTable ds = new DataTable();
                    adap.Fill(ds);
                   

                    cmd.CommandText = "INSERT INTO employee.employee (name3,name1,name2,phone,birthdate,hiredate,speciality,salary,education,children,married,faildate,depart,idcod,cpas,contract,contr_data) VALUES ('" +
                        employeed.name1 + "','" +
                        employeed.name2 + "','" +
                        employeed.name3 + "','" +
                        employeed.phone + "','" +
                        employeed.birthdate.ToString("d") + "','" +
                        employeed.hiredate.ToString("d") + "','" +
                        employeed.speciality + "','" +
                        employeed.salary + "','" +
                        employeed.education + "','" +
                        employeed.children + "','" +
                        employeed.married + "','" +
                        employeed.faildate.ToString("d") + "','" +
                        employeed.depart + "','" +
                        
                        employeed.CNP + "','" +
                        employeed.CPAS + "','" +
                        employeed.contract_nr + "','" +
                        employeed.contract_date.ToString("d") + "');";

                    string employeeid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT EMPLOYEEID FROM employee.employee where idcod = '" + employeed.CNP + "';";
                    ds.Clear();
                    adap.Fill(ds);

                    for (int i = 0; i < ds.Rows.Count; i++)
                    {
                        employeeid = ds.Rows[i]["EMPLOYEEID"].ToString();
                        EmployeeIDTextBox.Text = employeeid;
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog dialog = new ContentDialog()
                    {
                        Title = "Error",
                        Content = ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }

            ContentDialog successDialog = new ContentDialog()
            {
                Title = "Success",
                Content = "Employee Created. EmployeeID = " + EmployeeIDTextBox.Text,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await successDialog.ShowAsync();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Set current dates
            ContractDatePicker.Date = DateTime.Now;
            HireDatePicker.Date = DateTime.Now;
        }
    }
}
    