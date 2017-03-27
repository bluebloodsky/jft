using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JFT.Entity
{
    public class EntProject
    {
        public string pro_code { get; set; }
        public string client { get; set; }
        public string client_addr { get; set; }
        public string factory { get; set; }
        public string fac_addr { get; set; }
        public string linkman { get; set; }
        public string tel { get; set; }
        public string post_code { get; set; }
        public string email { get; set; }
        public string pro_name { get; set; }
        public string pro_type { get; set; }
        public string pro_sn { get; set; }
        public string pro_num { get; set; }
        public string test_type { get; set; }
        public string report_type { get; set; }
        public string report_format { get; set; }
        public string report_num { get; set; }
        public string seal_type { get; set; }
        public DateTime plan_comp_time { get; set; }
        public string test_money { get; set; }
        public string device_type { get; set; }
        public string instruments { get; set; }
        public DateTime test_date_start { get; set; }
        public DateTime test_date_end { get; set; }
        public string dev_info { get; set; }
        public string test_result { get; set; }
        public DateTime create_time { get; set; }
        public string contract_code { get; set; }
        public string in_money { get; set; }
        public string test_status { get; set; }
        public string report_status { get; set; }
        public DateTime comp_time { get; set; }

        public string pro_status { get; set; }
        public DateTime get_date { get; set; }
        public string come_type { get; set; }
        public string comp_do { get; set; }
        public string attach_file { get; set; }
        public string org_name { get; set; }

        public Type GetPropType(string propertyName)
        {
            return this.GetType().GetProperty(propertyName).PropertyType;
        }
        public object GetValue(string propertyName)
        {
            return this.GetType().GetProperty(propertyName).GetValue(this, null);
        }  
    }
}
