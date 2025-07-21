using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gestionarea_HR;
namespace Gestionarea_HR
{
    public class Employee
    {
        public Int64 employeeid { get; set; }
        public string name1 { get; set; }
        public string name2 { get; set; }
        public string name3 { get; set; }
        public string phone { get; set; }
        public DateTime birthdate { get; set; }
        public DateTime hiredate { get; set; }
        public string speciality { get; set; }
        public int salary { get; set; }
        public string education { get; set; }
        public int children { get; set; }
        public int married { get; set; }
        public string depart { get; set; }
        public DateTime faildate { get; set; }
        public string contract_nr { get; set; }
        public DateTime contract_date { get; set; }
        public string CNP { get; set; }
        public string CPAS { get; set; }
    }
}
