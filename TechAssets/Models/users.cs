using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TechAssets.Models
{
    public class users
    {
        public static string userName = "wasana";
        public string login_ok { get; set; }
        public string emp_id { get; set; }
        public string user_domain { get; set; }
        public string login_status { get; set; }
        public string get_emp_division { get; set; }
        public string get_emp_location_code { get; set; }
        public string emp_grade { get; set; }
        public string get_emp_name { get; set; }
        public string get_emp_designation { get; set; }

        public static string emp_division = "UserNotLogin";

    }

}