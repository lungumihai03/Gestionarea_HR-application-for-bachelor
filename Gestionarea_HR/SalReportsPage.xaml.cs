using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;

namespace Gestionarea_HR
{
    public sealed partial class SalReportsPage : Page
    {
        string nume, prenume, startMonth, endMonth;
        public SalReportsViewModel ViewModel { get; set; }
        private Excel.Application excel;
        private Excel.Workbook worKbooK;
        private Excel.Worksheet worKsheet;

        public SalReportsPage()
        {
            this.InitializeComponent();
            ViewModel = new SalReportsViewModel();
            this.DataContext = ViewModel;

            ViewModel.Months = new ObservableCollection<string>
            {
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"
            };
        }

        private async void button_generate_Click(object sender, RoutedEventArgs e)
        {
            nume = textBox_nume.Text;
            prenume = textBox_prenume.Text;
            startMonth = comboBox_monthstart.SelectedItem.ToString();
            endMonth = comboBox_monthend.SelectedItem.ToString();

            try
            {
                DataTable dt = await Task.Run(() => Report());
                ViewModel.ReportData = dt;
                ViewModel.UpdateReportItems();
                button_export.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Eroare",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void TestForFill()
        {
            if (!string.IsNullOrEmpty(textBox_nume.Text) && !string.IsNullOrEmpty(textBox_prenume.Text) &&
                comboBox_monthstart.SelectedItem != null && comboBox_monthend.SelectedItem != null)
            {
                button_generate.IsEnabled = true;
            }
            else
            {
                button_generate.IsEnabled = false;
            }
        }

        private DataTable Report()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            DataTable dt = new DataTable();
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                connect.Open();
                MySqlCommand cmd = connect.CreateCommand();
                cmd.CommandText = "SELECT * FROM salariu WHERE nume = @nume AND prenume = @prenume AND month BETWEEN @monthstart AND @monthend";
                cmd.Parameters.AddWithValue("@nume", nume);
                cmd.Parameters.AddWithValue("@prenume", prenume);
                cmd.Parameters.AddWithValue("@monthstart", startMonth);
                cmd.Parameters.AddWithValue("@monthend", endMonth);
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        private void textBox_nume_TextChanged(object sender, RoutedEventArgs e)
        {
            TestForFill();
        }

        private void textBox_prenume_TextChanged(object sender, RoutedEventArgs e)
        {
            TestForFill();
        }

        private void comboBox_monthstart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TestForFill();
        }

        private void comboBox_monthend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TestForFill();
        }

        private async void button_export_Click(object sender, RoutedEventArgs e)
        {
            excel = new Excel.Application();
            if (excel == null)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Eroare",
                    Content = "Excel nu este instalat corect!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            worKbooK = excel.Workbooks.Add(Type.Missing);
            worKsheet = (Excel.Worksheet)worKbooK.Sheets["Sheet1"];
            worKsheet = (Excel.Worksheet)worKbooK.ActiveSheet;
            worKsheet.Name = "Raport";
            for (int i = 1; i <= ViewModel.ReportData.Columns.Count; i++)
            {
                worKsheet.Cells[1, i] = ViewModel.ReportData.Columns[i - 1].ColumnName;
            }

            for (int i = 0; i < ViewModel.ReportData.Rows.Count; i++)
            {
                for (int j = 0; j < ViewModel.ReportData.Columns.Count; j++)
                {
                    worKsheet.Cells[i + 2, j + 1] = ViewModel.ReportData.Rows[i][j].ToString();
                }
            }

            worKbooK.SaveAs($"D:\\Mihai\\MIHAI\\1_univer\\Doned\\Licenta\\Raport_Salarial_{textBox_nume.Text}_{textBox_prenume.Text}.xlsx",
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            worKbooK.Close();
            excel.Quit();
            NAR(worKsheet);
            NAR(worKbooK);
            NAR(excel);
            GC.Collect();

            ContentDialog successDialog = new ContentDialog
            {
                Title = "Succes",
                Content = $"Fișierul Excel Raport Salarial {textBox_nume.Text} {textBox_prenume.Text} a fost creat",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();
        }

        private void NAR(object o)
        {
            try
            {
                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(o) > 0) ;
            }
            catch { }
            finally
            {
                o = null;
            }
        }
    }

    public class SalReportsViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private ObservableCollection<string> _months;
        private DataTable _reportData;
        private ObservableCollection<ReportItem> _reportItems;

        public ObservableCollection<string> Months
        {
            get => _months;
            set
            {
                _months = value;
                OnPropertyChanged();
            }
        }

        public DataTable ReportData
        {
            get => _reportData;
            set
            {
                _reportData = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ReportItem> ReportItems
        {
            get => _reportItems;
            set
            {
                _reportItems = value;
                OnPropertyChanged();
            }
        }

        public void UpdateReportItems()
        {
            ReportItems = new ObservableCollection<ReportItem>();
            if (ReportData != null)
            {
                foreach (DataRow row in ReportData.Rows)
                {
                    ReportItems.Add(new ReportItem
                    {
                        ID = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0,
                        EmployeeID = row["employeeid"] != DBNull.Value ? Convert.ToInt32(row["employeeid"]) : 0,
                        Nume = row["nume"] != DBNull.Value ? row["nume"].ToString() : string.Empty,
                        Prenume = row["prenume"] != DBNull.Value ? row["prenume"].ToString() : string.Empty,
                        Salary = row["salary"] != DBNull.Value ? Convert.ToInt32(row["salary"]) : 0,
                        SP = row["sp"] != DBNull.Value ? Convert.ToDouble(row["sp"]) : 0,
                        SC = row["sc"] != DBNull.Value ? Convert.ToDouble(row["sc"]) : 0,
                        ST = row["st"] != DBNull.Value ? Convert.ToDouble(row["st"]) : 0,
                        MED = row["med"] != DBNull.Value ? Convert.ToDouble(row["med"]) : 0,
                        IV = row["iv"] != DBNull.Value ? Convert.ToDouble(row["iv"]) : 0,
                        SB = row["sb"] != DBNull.Value ? Convert.ToDouble(row["sb"]) : 0,
                        Month = row["month"] != DBNull.Value ? row["month"].ToString() : string.Empty,
                        Year = row["year"] != DBNull.Value ? row["year"].ToString() : string.Empty,
                        Ore_t = row["ore_t"] != DBNull.Value ? Convert.ToInt32(row["ore_t"]) : 0,
                        Ore_l = row["ore_l"] != DBNull.Value ? Convert.ToInt32(row["ore_l"]) : 0,
                        SN = row["sn"] != DBNull.Value ? Convert.ToDouble(row["sn"]) : 0
                    });
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }

    public class ReportItem
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public int Salary { get; set; }
        public double SP { get; set; }
        public double SC { get; set; }
        public double ST { get; set; }
        public double MED { get; set; }
        public double IV { get; set; }
        public double SB { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public int Ore_t { get; set; }
        public int Ore_l { get; set; }
        public double SN { get; set; }
    }
}