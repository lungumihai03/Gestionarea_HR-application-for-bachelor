using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Gestionarea_HR
{
    class salarii
    {
        
        public int employeeid { get; set; }
        public string cnp { get; set; }
        public string nume { get; set; }
        public string prenume { get; set; }
        public int salary { get; set; }
        public double sn { get; set; }
        public double sp { get; set; }
        public double sc { get; set; }
        public double st { get; set; }
        public double med { get; set; }
        public double iv { get; set; }
        public double sb { get; set; }
        public int children { get; set; }
        public int ore_t { get; set; }
        public int ore_l { get; set; }
        public int month { get; set; }
        double iv_p, med_p;
        public void load_data()
        {
            string connectionString = "SERVER=localhost; DATABASE=employee;UID=root;PASSWORD=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM employee.calc_salariu";
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sp = Convert.ToDouble(reader["sp"]);
                        sc = Convert.ToDouble(reader["sc"]);
                        iv_p = Convert.ToDouble(reader["iv"]);
                        med_p = Convert.ToDouble(reader["im"]);
                    }
                }
            }
        }
        public void calcule(int oll, int opt)
        {
            load_data();
            sc = sc * children;
            double sal_scut;
            ore_l = opt;
            ore_t = oll;
            sn = (Convert.ToDouble(salary) / Convert.ToDouble(oll)) * Convert.ToDouble(opt);
            sal_scut = sn - sp - sc;
            iv = sal_scut / (1 - iv_p / 100) - sal_scut;
            med = (sn + iv) / (1 - med_p / 100) - (sn + iv);
            sb = sn + iv + med;
            st = sp + sc;
        }
    }
}
