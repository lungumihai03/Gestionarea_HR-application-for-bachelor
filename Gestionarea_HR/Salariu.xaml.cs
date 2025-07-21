using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using System.IO;
using WinRT.Interop;
using Windows.Storage.Pickers;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Gestionarea_HR
{
    public sealed partial class Salariu : Page
    {
        int oll;
        int olt;
        int opt;
        int month;
        int year;
        salarii salariu = new salarii();
        public static class global
        {
            public static string sal_real;
        }

        public Salariu()
        {
            this.InitializeComponent();
        }

        private async void Button_Pontaj_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".xls");
            openPicker.FileTypeFilter.Add(".xlsx");
            var currentWindow = App.CurrentWindow;
            if (currentWindow == null)
            {
                Debug.WriteLine("Current window is null!");
                return;
            }

            var hwnd = WindowNative.GetWindowHandle(currentWindow);
            if (hwnd == IntPtr.Zero)
            {
                Debug.WriteLine("Window handle is invalid!");
                return;
            }
            InitializeWithWindow.Initialize(openPicker, hwnd);
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                textBox_pontaj.Text = file.Path;
            }
        }

        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Excel.Workbook wb = excel.Workbooks.Open(textBox_pontaj.Text);
            Excel.Worksheet excelSheet = (Excel.Worksheet)wb.ActiveSheet;
            var cellCnp = excelSheet.Cells[2, 2] as Excel.Range;
            var cellOll = excelSheet.Cells[5, 2] as Excel.Range;
            var cellOlt = excelSheet.Cells[6, 5] as Excel.Range;
            var cellOpt = excelSheet.Cells[6, 6] as Excel.Range;
            var cellMonth = excelSheet.Cells[3, 2] as Excel.Range;
            var cellYear = excelSheet.Cells[4, 2] as Excel.Range;

            string cnp = cellCnp?.Value?.ToString();
            oll = cellOll?.Value != null ? Convert.ToInt32(cellOll.Value) : 0;
            olt = cellOlt?.Value != null ? Convert.ToInt32(cellOlt.Value) : 0;
            opt = cellOpt?.Value != null ? Convert.ToInt32(cellOpt.Value) : 0;
            month = cellMonth?.Value != null ? Convert.ToInt32(cellMonth.Value) : 0;
            year = cellYear?.Value != null ? Convert.ToInt32(cellYear.Value) : 0;

            NAR(excelSheet.Cells);
            NAR(excelSheet);
            NAR(excel.Worksheets);
            wb.Close(0);
            NAR(wb);
            NAR(excel.Workbooks);
            excel.Application.Quit();
            NAR(excel);
            GC.Collect();
            KillExcel();
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            MySqlConnection connect = new MySqlConnection(connectionString);
            connect.Open();
            try
            {
                MySqlCommand cmd = connect.CreateCommand();
                cmd.CommandText = "SELECT EMPLOYEEID, NAME1, NAME3, SALARY FROM employee.employee WHERE IDCOD='" + cnp + "'";
                MySqlDataAdapter adap = new MySqlDataAdapter(cmd);
                System.Data.DataTable ds = new System.Data.DataTable();
                adap.Fill(ds);
                for (int i = 0; i < ds.Rows.Count; i++)
                {
                    salariu.employeeid = Convert.ToInt32(ds.Rows[i]["EMPLOYEEID"]);
                    salariu.nume = ds.Rows[i]["NAME3"].ToString();
                    salariu.prenume = ds.Rows[i]["NAME1"].ToString();
                    salariu.salary = Convert.ToInt32(ds.Rows[i]["SALARY"]);
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                dialog.ShowAsync();
            }
            connect.Close();
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Pontaj importat",
                Content = "Pontaj importat pentru " + salariu.nume + " " + salariu.prenume,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            successDialog.ShowAsync();
        }

        private async void Button_Calcul_Click(object sender, RoutedEventArgs e)
        {
            salariu.calcule(oll, opt);
            double tot = salariu.iv + salariu.med;
            salariu.sn = Math.Round(salariu.sn, 2);
            global.sal_real = Math.Round(salariu.sn, 2).ToString();

            label_SP.Text = "Scutire Personala (SP): " + Math.Round(salariu.sp, 2).ToString();
            label_SC.Text = "Scutire pe copii (SC): " + Math.Round(salariu.sc, 2).ToString();
            label_ST.Text = "Scutiri total (ST): " + Math.Round(salariu.st, 2).ToString();
            label_IV.Text = "Impozit pe Venit (IV): " + Math.Round(salariu.iv, 2).ToString();
            label_SN.Text = "Salariul net: " + Math.Round(salariu.sn, 2).ToString();
            label_SB.Text = "Salariul brut: " + Math.Round(salariu.sb, 2).ToString();
            label_TCS.Text = "Total Contributii Salariat :" + Math.Round(tot, 2).ToString();
            label_MED.Text = "Impozit pe medicina (MED): " + Math.Round(salariu.med, 2).ToString();
            label_salary.Text = "Salariul de baza: " + (salariu.salary).ToString();

            label_10.Visibility = Visibility.Visible;
            label_10.Text = "Salar calculat pentru : " + salariu.nume + " " + salariu.prenume;

            await ImportDataAsync(this.XamlRoot);
        }

        private async Task ImportDataAsync(XamlRoot xamlRoot)
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            MySqlConnection connect = new MySqlConnection(connectionString);
            await connect.OpenAsync();
            try
            {
                string query = "INSERT INTO employee.salariu (employeeid, nume, prenume, salary, sp, sc, st, med, iv, sb, month, year, ore_t, ore_l, sn) VALUES (@employeeid, @nume, @prenume, @salary, @sp, @sc, @st, @med, @iv, @sb, @month, @year, @ore_t, @ore_l, @sn)";
                MySqlCommand cmd = new MySqlCommand(query, connect);
                cmd.Parameters.AddWithValue("@employeeid", salariu.employeeid);
                cmd.Parameters.AddWithValue("@nume", salariu.nume);
                cmd.Parameters.AddWithValue("@prenume", salariu.prenume);
                cmd.Parameters.AddWithValue("@salary", salariu.salary);
                cmd.Parameters.AddWithValue("@sp", salariu.sp);
                cmd.Parameters.AddWithValue("@sc", salariu.sc);
                cmd.Parameters.AddWithValue("@st", salariu.st);
                cmd.Parameters.AddWithValue("@med", salariu.med);
                cmd.Parameters.AddWithValue("@iv", salariu.iv);
                cmd.Parameters.AddWithValue("@sb", salariu.sb);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@ore_t", salariu.ore_t);
                cmd.Parameters.AddWithValue("@ore_l", salariu.ore_l);
                cmd.Parameters.AddWithValue("@sn", salariu.sn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                connect.Close();
            }
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

        private void KillExcel()
        {
            Process[] excelProcesses = Process.GetProcessesByName("Excel");
            foreach (Process p in excelProcesses)
            {
                p.Kill();
            }
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
