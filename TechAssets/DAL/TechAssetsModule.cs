using TechAssets.Models;
using TechAssets.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TechAssets.DAL
{
    public class TechAssetsModule
    {

        public static users get_user_id(users data)
        {
            using (var conn2 = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn2.Open();
                SqlCommand sqlcmd = new SqlCommand("SELECT a.emp_id, a.user_domain, a.status, b.division, b.grade, c.location_id, c.cn, q.Name_with_ini, p.Designation FROM [EMMSDB].[dbo].[user_credentials] AS a INNER JOIN [EMMSDB].[dbo].[Staff_Employee_Designation] AS b ON a.emp_id = b.emp_no INNER JOIN [EMMSDB].[dbo].[Staff_Division] AS c ON b.division = c.div_index INNER JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS q ON b.emp_no = q.emp_no INNER JOIN [EMMSDB].[dbo].[Staff_Designation] AS p ON b.Designation = p.status WHERE a.user_name = '" + users.userName + "' AND (a.Status = '1' OR a.status = '0' OR a.status = '2')", conn2);

                using (SqlDataReader dr = sqlcmd.ExecuteReader())
                {
                    if (dr.Read()) 
                    {
                        data.emp_id = dr[0].ToString();
                        data.user_domain = dr[1].ToString();
                        data.login_status = dr[2].ToString();
                        data.get_emp_division = dr[3].ToString();
                        data.get_emp_location_code = dr[5].ToString();
                        data.emp_grade = dr[4].ToString();
                        data.get_emp_name = dr[7].ToString();
                        data.get_emp_designation = dr[8].ToString();
                    }
                }

            }
           
            return data;
        }


        public static techassetsM GetUserInformation(string userId)
        {
            techassetsM data = new techassetsM();

            using (var conn2 = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn2.Open();

                SqlCommand sqlcmd = new SqlCommand("SELECT a.emp_id, a.user_domain, a.status, b.division, b.grade, c.location_id, c.cn, q.Name_with_ini, p.Designation, c.[division] AS divisionName FROM [EMMSDB].[dbo].[user_credentials] AS a INNER JOIN [EMMSDB].[dbo].[Staff_Employee_Designation] AS b ON a.emp_id = b.emp_no INNER JOIN [EMMSDB].[dbo].[Staff_Division] AS c ON b.division = c.div_index INNER JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS q ON b.emp_no = q.emp_no INNER JOIN [EMMSDB].[dbo].[Staff_Designation] AS p ON b.Designation = p.status WHERE a.emp_id = @userId AND (a.Status = '1' OR a.Status = '0' OR a.Status = '2')", conn2);

                sqlcmd.Parameters.AddWithValue("@userId", userId);

                using (SqlDataReader dr = sqlcmd.ExecuteReader())
                {
                    if (dr.Read()) 
                    {
                        data.emp_id = dr["emp_id"].ToString();
                        data.user_domain = dr["user_domain"].ToString();
                        data.login_status = dr["status"].ToString();
                        data.get_emp_division = dr["division"].ToString();
                        data.get_emp_location_code = dr["location_id"].ToString();
                        data.emp_grade = dr["grade"].ToString();
                        data.get_emp_name = dr["Name_with_ini"].ToString();
                        data.get_emp_designation = dr["Designation"].ToString();
                        data.divisionName = dr["divisionName"].ToString().Trim();
                        users.emp_division = dr["division"].ToString();
                    }
                }
            }

            return data;
        }

        public static string GetEmpName(string empNo)
        {
            string name = "";

            string sql1 = "SELECT a.Name_with_ini FROM [EMMSDB].[dbo].[Staff_employee_Details] AS a INNER JOIN [EMMSDB].[dbo].[Staff_Employee_Designation] AS b ON a.Emp_no = b.Emp_no WHERE b.Division = '" + users.emp_division + "' AND a.Emp_no = @empNo";

            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        sqlcmd.Parameters.AddWithValue("@empNo", empNo);

                        object result = sqlcmd.ExecuteScalar();

                        if (result != null)
                        {
                            name = result.ToString();
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {                
                Console.WriteLine("SQL Error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {     
                Console.WriteLine("Error: " + ex.Message);
            }

            return name;
        }







    }
}