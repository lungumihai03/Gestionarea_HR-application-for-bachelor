using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;
using Windows.Storage;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;

namespace Gestionarea_HR
{
    public sealed partial class Concedii : Page
    {
        private ObservableCollection<Employee> employees;
        private ObservableCollection<Holiday> holidays;

        public Concedii()
        {
            this.InitializeComponent();
            employees = new ObservableCollection<Employee>();
            holidays = new ObservableCollection<Holiday>();
            LoadData();
        }
        public class Employee
        {
            public string EmployeeId { get; set; }
            public string Name1 { get; set; }
            public string Name2 { get; set; }
            public string Name3 { get; set; }
        }
        public class Holiday
        {
            public string EmployeeId { get; set; }
            public string Name1 { get; set; }
            public string Name3 { get; set; }
            public string TipConcediu { get; set; }
            public DateTime DataInceput { get; set; }
            public DateTime DataSfarsit { get; set; }
        }
        private void LoadData()
        {
            employees = GetEmployees(0);
            holidays = GetHolidays(null);
            EmployeeDataGrid.ItemsSource = employees;
            HolidayDataGrid.ItemsSource = holidays;
        }

        private ObservableCollection<Employee> GetEmployees(int k, string searchText = "")
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            ObservableCollection<Employee> employeeList = new ObservableCollection<Employee>();

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    string query = k == 0
                        ? "SELECT employeeid, name1, name2, name3 FROM employee.employee;"
                        : "SELECT employeeid, name1, name2, name3 FROM employee.employee WHERE name3 LIKE @searchText OR name1 LIKE @searchText;";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        if (k == 1)
                        {
                            cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");
                        }

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                employeeList.Add(new Employee
                                {
                                    EmployeeId = dr["employeeid"].ToString(),
                                    Name1 = dr["name1"].ToString(),
                                    Name2 = dr["name2"].ToString(),
                                    Name3 = dr["name3"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Eroare",
                        Content = ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = errorDialog.ShowAsync();
                }
            }

            return employeeList;
        }

        private ObservableCollection<Holiday> GetHolidays(string employeeId = null, string searchText = null)
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            ObservableCollection<Holiday> holidayList = new ObservableCollection<Holiday>();

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    string query = "SELECT e.employeeid, e.name1, e.name3, c.tip_concediu, c.data_inceput, c.data_sfarsit " +
                                   "FROM employee.concedii c " +
                                   "JOIN employee.employee e ON c.employeeid = e.employeeid ";

                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        query += "WHERE c.employeeid = @employeeId ";
                    }
                    else if (!string.IsNullOrEmpty(searchText))
                    {
                        query += "WHERE e.name3 LIKE @searchText OR e.name1 LIKE @searchText ";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        if (!string.IsNullOrEmpty(employeeId))
                        {
                            cmd.Parameters.AddWithValue("@employeeId", employeeId);
                        }
                        else if (!string.IsNullOrEmpty(searchText))
                        {
                            cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");
                        }

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                holidayList.Add(new Holiday
                                {
                                    EmployeeId = dr["employeeid"].ToString(),
                                    Name1 = dr["name1"].ToString(),
                                    Name3 = dr["name3"].ToString(),
                                    TipConcediu = dr["tip_concediu"].ToString(),
                                    DataInceput = Convert.ToDateTime(dr["data_inceput"]),
                                    DataSfarsit = Convert.ToDateTime(dr["data_sfarsit"])
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Eroare",
                        Content = ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = errorDialog.ShowAsync();
                }
            }

            return holidayList;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text;
            employees = GetEmployees(1, searchText);
            holidays = GetHolidays(null, searchText);
            EmployeeDataGrid.ItemsSource = employees;
            HolidayDataGrid.ItemsSource = holidays;
        }

        private void EmployeeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is Employee selectedEmployee && !string.IsNullOrEmpty(selectedEmployee.EmployeeId))
            {
                holidays = GetHolidays(selectedEmployee.EmployeeId);
            }
            else
            {
                holidays = GetHolidays(null, SearchBox.Text);
            }
            HolidayDataGrid.ItemsSource = holidays;
        }

        private async void AddHoliday_Click(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel { Spacing = 10 };
            ComboBox employeeComboBox = new ComboBox
            {
                Header = "Angajat",
                PlaceholderText = "Selectați un angajat"
            };

            var employees = GetEmployees(0);
            int selectedIndex = -1;
            Employee selectedEmployee = EmployeeDataGrid.SelectedItem as Employee;

            for (int i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];
                var comboBoxItem = new ComboBoxItem
                {
                    Content = $"{emp.Name1} {emp.Name3}",
                    Tag = emp
                };
                employeeComboBox.Items.Add(comboBoxItem);

                if (selectedEmployee != null && emp.EmployeeId == selectedEmployee.EmployeeId)
                {
                    selectedIndex = i;
                }
            }

            if (selectedIndex >= 0)
            {
                employeeComboBox.SelectedIndex = selectedIndex;
            }

            ComboBox tipConcediuComboBox = new ComboBox
            {
                Header = "Tip Concediu",
                PlaceholderText = "Selectați tipul concediului"
            };
            tipConcediuComboBox.Items.Add(new ComboBoxItem { Content = "Odihna" });
            tipConcediuComboBox.Items.Add(new ComboBoxItem { Content = "Medical" });
            tipConcediuComboBox.Items.Add(new ComboBoxItem { Content = "Fara plata" });
            DatePicker dataInceputPicker = new DatePicker
            {
                Header = "Data Început"
            };
            DatePicker dataSfarsitPicker = new DatePicker
            {
                Header = "Data Sfârșit"
            };
            panel.Children.Add(employeeComboBox);
            panel.Children.Add(tipConcediuComboBox);
            panel.Children.Add(dataInceputPicker);
            panel.Children.Add(dataSfarsitPicker);
            ContentDialog addHolidayDialog = new ContentDialog
            {
                Title = "Adaugă Concediu",
                Content = panel,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            ContentDialogResult result = await addHolidayDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedEmployeeItem = employeeComboBox.SelectedItem as ComboBoxItem;
                var selectedTipConcediuItem = tipConcediuComboBox.SelectedItem as ComboBoxItem;
                DateTimeOffset? dataInceput = dataInceputPicker.SelectedDate;
                DateTimeOffset? dataSfarsit = dataSfarsitPicker.SelectedDate;

                if (selectedEmployeeItem != null && selectedTipConcediuItem != null && dataInceput.HasValue && dataSfarsit.HasValue)
                {
                    var selectedEmployeeFromCombo = selectedEmployeeItem.Tag as Employee;
                    string employeeId = selectedEmployeeFromCombo.EmployeeId;
                    string prenume = selectedEmployeeFromCombo.Name1;
                    string nume = selectedEmployeeFromCombo.Name3;
                    string tipConcediu = selectedTipConcediuItem.Content.ToString();
                    InsertHoliday(employeeId, nume, prenume, tipConcediu, dataInceput.Value.DateTime, dataSfarsit.Value.DateTime);
                    RefreshHolidays();
                }
                else
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Eroare",
                        Content = "Vă rugăm să completați toate câmpurile.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
        private void InsertHoliday(string employeeId, string nume, string prenume, string tipConcediu, DateTime dataInceput, DateTime dataSfarsit)
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    string query = "INSERT INTO employee.concedii (employeeid, nume, prenume, tip_concediu, data_inceput, data_sfarsit) " +
                                   "VALUES (@employeeId, @nume, @prenume, @tipConcediu, @dataInceput, @dataSfarsit);";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);
                        cmd.Parameters.AddWithValue("@nume", nume);
                        cmd.Parameters.AddWithValue("@prenume", prenume);
                        cmd.Parameters.AddWithValue("@tipConcediu", tipConcediu);
                        cmd.Parameters.AddWithValue("@dataInceput", dataInceput.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@dataSfarsit", dataSfarsit.ToString("yyyy-MM-dd"));

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Eroare",
                        Content = ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = errorDialog.ShowAsync();
                }
            }
        }
        private void RefreshHolidays()
        {
            if (EmployeeDataGrid.SelectedItem is Employee selectedEmployee && !string.IsNullOrEmpty(selectedEmployee.EmployeeId))
            {
                holidays = GetHolidays(selectedEmployee.EmployeeId);
            }
            else
            {
                holidays = GetHolidays(null, SearchBox.Text);
            }
            HolidayDataGrid.ItemsSource = holidays;
        }
        private async void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Excel.Application excel = new Excel.Application();
                Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

                worksheet.Name = "Raport";
                var columns = HolidayDataGrid.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = columns[i].Header;
                }
                var items = HolidayDataGrid.ItemsSource as ObservableCollection<Holiday>;
                for (int i = 0; i < items.Count; i++)
                {
                    worksheet.Cells[i + 2, 1] = items[i].EmployeeId;
                    worksheet.Cells[i + 2, 2] = items[i].Name1;
                    worksheet.Cells[i + 2, 3] = items[i].Name3;
                    worksheet.Cells[i + 2, 4] = items[i].TipConcediu;
                    worksheet.Cells[i + 2, 5] = items[i].DataInceput.ToString("d");
                    worksheet.Cells[i + 2, 6] = items[i].DataSfarsit.ToString("d");
                }
                string dateTimeString = DateTime.Now.ToString("MMddyy");
                string filePath = $@"D:\Mihai\MIHAI\1_univer\Doned\Licenta\Raport_Concedii_{dateTimeString}.xlsx";
                workbook.SaveAs(filePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                excel.Quit();
                KillExcel();
                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Succes",
                    Content = $"Raportul a fost salvat la: {filePath}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Eroare",
                    Content = $"A apărut o eroare la export: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void KillExcel()
        {
            Process[] excelProcesses = Process.GetProcessesByName("Excel");
            foreach (Process p in excelProcesses)
            {
                p.Kill();
            }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            SearchBox.Text = "";
            EmployeeDataGrid.SelectedItem = null;
        }
    }
}