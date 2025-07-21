using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using System.Data;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace Gestionarea_HR
{
    public sealed partial class ButterflyPage : Page
    {
        salarii salar = new salarii();
        Excel.Application excel;
        Excel.Workbook worKbooK;
        Excel.Worksheet worKsheet;
        DataTable ds = new DataTable();

        public ButterflyPage()
        {
            this.InitializeComponent();
            for (int i = 1; i <= 12; i++)
            {
                comboBox_month.Items.Add(i.ToString());
            }

            for (int i = 2024; i <= DateTime.Now.Year; i++)
            {
                comboBox_year.Items.Add(i.ToString());
            }
        }

        private async void button_do_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_nume.Text) || string.IsNullOrEmpty(textBox_prenume.Text) ||
                comboBox_month.SelectedItem == null || comboBox_year.SelectedItem == null)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Eroare",
                    Content = "Vă rugăm să completați toate câmpurile!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            MySqlConnection connect = new MySqlConnection(connectionString);
            await connect.OpenAsync();
            try
            {
                MySqlCommand cmd = connect.CreateCommand();
                cmd.CommandText = "SELECT * FROM employee.salariu WHERE nume = @nume AND prenume = @prenume AND month = @month AND year = @year";
                cmd.Parameters.AddWithValue("@nume", textBox_nume.Text);
                cmd.Parameters.AddWithValue("@prenume", textBox_prenume.Text);
                cmd.Parameters.AddWithValue("@month", comboBox_month.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@year", comboBox_year.SelectedItem.ToString());
                MySqlDataAdapter adap = new MySqlDataAdapter(cmd);

                ds.Clear();
                adap.Fill(ds);
                if (ds.Rows.Count == 0)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Eroare",
                        Content = "Nu s-au găsit date pentru angajatul specificat în luna și anul selectate!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }

                for (int i = 0; i < ds.Rows.Count; i++)
                {
                    salar.nume = ds.Rows[i]["nume"].ToString();
                    salar.prenume = ds.Rows[i]["prenume"].ToString();
                    salar.salary = Convert.ToInt32(ds.Rows[i]["salary"]);
                    salar.sb = Convert.ToDouble(ds.Rows[i]["sb"]);
                    salar.sp = Convert.ToDouble(ds.Rows[i]["sp"]);
                    salar.sc = Convert.ToDouble(ds.Rows[i]["sc"]);
                    salar.st = Convert.ToDouble(ds.Rows[i]["st"]);
                    salar.med = Convert.ToDouble(ds.Rows[i]["med"]);
                    salar.iv = Convert.ToDouble(ds.Rows[i]["iv"]);
                    salar.sn = Convert.ToDouble(ds.Rows[i]["sn"]);
                    salar.ore_t = Convert.ToInt32(ds.Rows[i]["ore_t"]);
                    salar.ore_l = Convert.ToInt32(ds.Rows[i]["ore_l"]);
                    salar.month = Convert.ToInt32(ds.Rows[i]["month"]);
                }
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
                return;
            }
            finally
            {
                connect.Close();
            }

            double tot = salar.iv + salar.med;

            label_SP.Text = Math.Round(salar.sp, 2).ToString();
            label_SC.Text = Math.Round(salar.sc, 2).ToString();
            label_ST.Text = Math.Round(salar.st, 2).ToString();
            label_IV.Text = Math.Round(salar.iv, 2).ToString();
            label_SN.Text = Math.Round(salar.sn, 2).ToString();
            label_SB.Text = Math.Round(salar.sb, 2).ToString();
            label_TCS.Text = Math.Round(tot, 2).ToString();
            label_MED.Text = Math.Round(salar.med, 2).ToString();
            label_salary.Text = salar.salary.ToString();

            button_export.IsEnabled = true;
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

            string[] luna = new string[] { "Ianuarie", "Februarie", "Martie", "Aprilie", "Mai", "Iunie", "Iulie", "August", "Septembrie", "Octombrie", "Noiembrie", "Decembrie" };
            excel.Visible = false;
            excel.DisplayAlerts = false;
            worKbooK = excel.Workbooks.Add(Type.Missing);
            worKsheet = (Excel.Worksheet)worKbooK.ActiveSheet;
            worKsheet.Name = "Fluturas";

            worKsheet.Range[worKsheet.Cells[1, 1], worKsheet.Cells[1, 2]].Merge();
            string data = DateTime.Now.ToString("dd/MM/yyyy");
            worKsheet.Cells[1, 1] = "Fluturas " + salar.nume + " " + salar.prenume + "       " + data;
            worKsheet.Cells.Font.Size = 15;

            worKsheet.Range[worKsheet.Cells[2, 1], worKsheet.Cells[2, 2]].Merge();
            worKsheet.Cells[2, 1] = "Salar net stabilit: " + salar.salary;
            worKsheet.Range[worKsheet.Cells[3, 1], worKsheet.Cells[3, 2]].Merge();
            worKsheet.Cells[3, 1] = "Ore lucrătoare total: " + salar.ore_t;
            worKsheet.Range[worKsheet.Cells[4, 1], worKsheet.Cells[4, 2]].Merge();
            worKsheet.Cells[4, 1] = "Ore lucrate: " + salar.ore_l;
            worKsheet.Cells[4, 2] = Math.Round(salar.sn, 2);
            worKsheet.Cells[5, 1] = "Asigurarea Medicală: ";
            worKsheet.Cells[5, 2] = Math.Round(salar.med, 2);
            worKsheet.Cells[6, 1] = "Impozit pe venit: ";
            worKsheet.Cells[6, 2] = Math.Round(salar.iv, 2);
            worKsheet.Range[worKsheet.Cells[7, 1], worKsheet.Cells[7, 2]].Merge();
            worKsheet.Cells[7, 1] = "Salar brut: " + Math.Round(salar.sb, 2);

            Excel.Range range6_2 = worKsheet.Cells[6, 2] as Excel.Range;
            if (range6_2 != null)
            {
                range6_2.EntireColumn.AutoFit();
                range6_2.EntireRow.AutoFit();
            }

            Excel.Range range8_1 = worKsheet.Cells[8, 1] as Excel.Range;
            if (range8_1 != null)
            {
                range8_1.EntireColumn.AutoFit();
                double currentWidth = Convert.ToDouble(range8_1.EntireColumn.ColumnWidth);
                range8_1.EntireColumn.ColumnWidth = currentWidth + 10;
                range8_1.EntireRow.AutoFit();
            }

            worKsheet.Range[worKsheet.Cells[8, 1], worKsheet.Cells[8, 2]].Merge();
            worKsheet.Cells[8, 1] = "Total pe luna " + luna[salar.month - 1] + " : " + salar.sn;

            Excel.Range tableRange = worKsheet.Range[worKsheet.Cells[1, 1], worKsheet.Cells[8, 2]];
            tableRange.Borders.LineStyle = 1;
            tableRange.Borders.Weight = 2;

            Marshal.ReleaseComObject(tableRange);

            worKbooK.SaveAs("D:\\Mihai\\MIHAI\\1_univer\\Doned\\Licenta\\Fluturas_" + salar.nume + "_" + salar.prenume + ".xlsx");
            worKbooK.Close();
            excel.Quit();
            NAR(tableRange);
            NAR(worKsheet);
            NAR(worKbooK);
            NAR(excel);
            GC.Collect();

            ContentDialog successDialog = new ContentDialog
            {
                Title = "Succes",
                Content = "Fișierul Excel Fluturas " + salar.nume + " " + salar.prenume + " a fost creat",
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
}