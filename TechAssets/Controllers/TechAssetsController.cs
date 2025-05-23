using TechAssets.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TechAssets.DAL;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Data;

namespace TechAssets.Controllers
{
    public class TechAssetsController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                users user = new users();
                user = TechAssetsModule.get_user_id(user);

                techassetsM techUser = TechAssetsModule.GetUserInformation(user.emp_id);

                return View(techUser);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving user information.";
                return View("Error");
            }
        }


        public ActionResult Devices(string status = null)
        {
            try
            {
                users user = TechAssetsModule.get_user_id(new users());
                techassetsM techUser = TechAssetsModule.GetUserInformation(user.emp_id);

                List<HardwareDevice> devices = GetHardwareDevices(techUser.get_emp_division, status);

                var message = TempData["Message"] as string;
                ViewBag.SuccessMessage = message != null ? message : null;

                var model = new DevicesViewModel
                {
                    TechUser = techUser,
                    HardwareDevices = devices,
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving device information.";
                return View("Error");
            }
        }





        private List<HardwareDevice> GetHardwareDevices(string get_emp_division, string status)
        {
            List<HardwareDevice> devices = new List<HardwareDevice>();

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    sqlcmd.Connection = conn;

                    if (string.IsNullOrEmpty(status))
                    {
                        sqlcmd.CommandText = "SELECT it.Item_Type, d.Invoice_Reference, d.IT_No, d.Serial_No, d.INV_No, d.QR, d.Current_Status,d.Authorized_Officer, ds.Status AS Current_Status_Text, d.activeStatus, sd.division AS Purchased_Division_Name, sd_loc.division AS Current_Location_Name, d.Current_LocationID, d.[HoIDst], d.[HoIDth], d.[CoID]" +
                                             "FROM [TechAssets].[dbo].[Hardware_Devices] d " +
                                             "JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode " +
                                             "INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code " +
                                             "INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID " +
                                             "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.Purchased_DivisionID = sd.div_index " +
                                             "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.Current_LocationID = sd_loc.div_index " +
                                             "WHERE d.activeStatus = 1 AND d.Current_LocationID = @get_emp_division AND d.Current_Status != 2 AND d.Current_Status = 1";
                    }
                    else if (status == "All")
                    {
                        sqlcmd.CommandText = "SELECT it.Item_Type, d.Invoice_Reference, d.IT_No, d.Serial_No, d.INV_No, d.QR, d.Current_Status, d.Authorized_Officer, ds.Status AS Current_Status_Text, d.activeStatus, sd.division AS Purchased_Division_Name, sd_loc.division AS Current_Location_Name, d.Current_LocationID, d.[HoIDst], d.[HoIDth], d.[CoID]" +
                                             "FROM [TechAssets].[dbo].[Hardware_Devices] d " +
                                             "JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode " +
                                             "INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code " +
                                             "INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID " +
                                             "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.Purchased_DivisionID = sd.div_index " +
                                             "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.Current_LocationID = sd_loc.div_index " +
                                             "WHERE d.activeStatus = 1 AND d.Current_LocationID = @get_emp_division AND d.Current_Status != 2";
                    }
                    else
                    {
                        var statusList = status.Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                        var statusPlaceholders = string.Join(",", statusList.Select((s, i) => $"@status{i}"));
                        sqlcmd.CommandText = $"SELECT it.Item_Type, d.Invoice_Reference, d.IT_No, d.Serial_No, d.INV_No, d.QR, d.Current_Status, d.Authorized_Officer, ds.Status AS Current_Status_Text, d.activeStatus, sd.division AS Purchased_Division_Name, sd_loc.division AS Current_Location_Name, d.Current_LocationID, d.[HoIDst], d.[HoIDth], d.[CoID] " +
                                             "FROM [TechAssets].[dbo].[Hardware_Devices] d " +
                                             "JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode " +
                                             "INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code " +
                                             "INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID " +
                                             "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.Purchased_DivisionID = sd.div_index " +
                                             "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.Current_LocationID = sd_loc.div_index " +
                                             $"WHERE d.activeStatus = 1 AND d.Current_LocationID = @get_emp_division AND d.Current_Status IN ({statusPlaceholders})";

                        for (int i = 0; i < statusList.Length; i++)
                        {
                            sqlcmd.Parameters.AddWithValue($"@status{i}", statusList[i]);
                        }
                    }

                    sqlcmd.Parameters.AddWithValue("@get_emp_division", get_emp_division);

                    using (SqlDataReader dr = sqlcmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            devices.Add(new HardwareDevice
                            {
                                IT_No = dr["IT_No"].ToString(),
                                Item_Type = dr["Item_Type"].ToString(),
                                Serial_No = dr["Serial_No"].ToString(),
                                INV_No = dr["INV_No"].ToString(),
                                QR = dr["QR"].ToString(),
                                Current_Status_Text = dr["Current_Status_Text"].ToString(),
                                Current_Status = dr["Current_Status"].ToString(),
                                Authorized_Officer = dr["Authorized_Officer"].ToString(),
                                HoIDst = dr["HoIDst"].ToString(),
                                HoIDth = dr["HoIDth"].ToString(),
                                CoID = dr["CoID"].ToString()

                            });
                        }
                    }
                }

                ViewBag.CurrentStatus = status;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving hardware devices.";
            }

            return devices;
        }





        public JsonResult getEmpName(string empNo)
        {
            try
            {
                string empName = TechAssetsModule.GetEmpName(empNo);
                return Json(new { empName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred while retrieving the employee name." }, JsonRequestBehavior.AllowGet);
            }
        }



        //[HttpPost]
        //public ActionResult RepairModal(string it, string feedback, string location, string contact, string employee, string sysuser)
        //{
        //    string insertSql = "INSERT INTO [TechAssets].[dbo].[Hardware_THandover] ([THandoverStatus], [IT_No], [THandoverType], [THandoverLocation], [DeviceUser], [Userfeedback], [ContactDetails], [THRequestDate], [SysTHReqUser], [SysTHReqDate]) VALUES (@THandoverStatus, @IT_No, @THandoverType ,@THandoverLocation, @DeviceUser, @Userfeedback, @ContactDetails, @THRequestDate, @SysTHReqUser, @SysTHReqDate)";

        //    string updateSql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 30 WHERE IT_No = @IT_No";

        //    try
        //    {
        //        using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
        //        {
        //            conn_uat.Open();

        //            using (SqlCommand insertCmd = new SqlCommand(insertSql, conn_uat))
        //            {
        //                insertCmd.Parameters.AddWithValue("@IT_No", it);
        //                insertCmd.Parameters.AddWithValue("@THandoverLocation", location);
        //                insertCmd.Parameters.AddWithValue("@DeviceUser", employee);
        //                insertCmd.Parameters.AddWithValue("@Userfeedback", feedback);
        //                insertCmd.Parameters.AddWithValue("@ContactDetails", contact);
        //                insertCmd.Parameters.AddWithValue("@THRequestDate", DateTime.Now);
        //                insertCmd.Parameters.AddWithValue("@SysTHReqUser", sysuser);
        //                insertCmd.Parameters.AddWithValue("@SysTHReqDate", DateTime.Now);
        //                insertCmd.Parameters.AddWithValue("@THandoverStatus", 30);
        //                insertCmd.Parameters.AddWithValue("@THandoverType", 41);

        //                insertCmd.ExecuteNonQuery();
        //            }

        //            using (SqlCommand updateCmd = new SqlCommand(updateSql, conn_uat))
        //            {
        //                updateCmd.Parameters.AddWithValue("@IT_No", it);

        //                updateCmd.ExecuteNonQuery();
        //            }
        //        }

        //        TempData["Message"] = "Repair Request Successfully submitted!";
        //    }
        //    catch (Exception ex)
        //    {

        //        TempData["ErrorMessage"] = "Error: " + ex.Message;
        //    }
        //    return RedirectToAction("Devices", "TechAssets", new { status = 30 });
        //}


        //==========================2025_05_09================================

        [HttpPost]
        public JsonResult RepairModal(string it, string feedback, string location, string contact, string employee, string sysuser)
        {
            string insertSql = "INSERT INTO [TechAssets].[dbo].[Hardware_THandover] ([THandoverStatus], [IT_No], [THandoverType], [THandoverLocation], [DeviceUser], [Userfeedback], [ContactDetails], [THRequestDate], [SysTHReqUser], [SysTHReqDate]) VALUES (@THandoverStatus, @IT_No, @THandoverType ,@THandoverLocation, @DeviceUser, @Userfeedback, @ContactDetails, @THRequestDate, @SysTHReqUser, @SysTHReqDate)";

            string updateSql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 30 WHERE IT_No = @IT_No";

            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand insertCmd = new SqlCommand(insertSql, conn_uat))
                    {
                        insertCmd.Parameters.AddWithValue("@IT_No", it);
                        insertCmd.Parameters.AddWithValue("@THandoverLocation", location);
                        insertCmd.Parameters.AddWithValue("@DeviceUser", employee);
                        insertCmd.Parameters.AddWithValue("@Userfeedback", feedback);
                        insertCmd.Parameters.AddWithValue("@ContactDetails", contact);
                        insertCmd.Parameters.AddWithValue("@THRequestDate", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@SysTHReqUser", sysuser);
                        insertCmd.Parameters.AddWithValue("@SysTHReqDate", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@THandoverStatus", 30);
                        insertCmd.Parameters.AddWithValue("@THandoverType", 41);

                        insertCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn_uat))
                    {
                        updateCmd.Parameters.AddWithValue("@IT_No", it);

                        updateCmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Repair request successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        //==========================2025_05_09================================

        public ActionResult RepairApproval(string status = null)
        {
            try
            {
                users user = TechAssetsModule.get_user_id(new users());
                techassetsM techUser = TechAssetsModule.GetUserInformation(user.emp_id);

                List<HardwareDevice> Appdevices = GetRepairAppDevices(techUser.get_emp_division, status);

                var message = TempData["Message"] as string;
                ViewBag.SuccessMessage = message ?? null;

                var model = new DevicesViewModel
                {
                    TechUser = techUser,
                    HardwareDevices = Appdevices,
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while loading the Repair Approval page: " + ex.Message;
                return View();
            }
        }



        private List<HardwareDevice> GetRepairAppDevices(string get_emp_division, string status)
        {
            List<HardwareDevice> devices = new List<HardwareDevice>();

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    SqlCommand sqlcmd = new SqlCommand
                    {
                        Connection = conn,
                        CommandText = "SELECT it.Item_Type, h.IT_No, h.INV_No, CONVERT(VARCHAR, ho.THRequestDate, 23) AS THRequestDate, ho.Userfeedback, ho.DeviceUser, ho.ContactDetails, ho.SysTHReqUser, ho.THandoverLocation, ho.id " +
                                      "FROM [TechAssets].[dbo].[Hardware_Devices] h " +
                                      "JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode " +
                                      "JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code " +
                                      "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index " +
                                      "LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index " +
                                      "LEFT JOIN [TechAssets].[dbo].[Hardware_THandover] ho ON h.IT_No = ho.IT_No " +
                                      "WHERE ho.THandoverStatus = '30' AND ho.THandoverLocation = @get_emp_division"
                    };
                    sqlcmd.Parameters.AddWithValue("@get_emp_division", get_emp_division);

                    using (SqlDataReader dr = sqlcmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            devices.Add(new HardwareDevice
                            {
                                IT_No = dr["IT_No"].ToString(),
                                Item_Type = dr["Item_Type"].ToString(),
                                INV_No = dr["INV_No"].ToString(),
                                THRequestDate = DateTime.Parse(dr["THRequestDate"].ToString()).Date,
                                Userfeedback = dr["Userfeedback"].ToString(),
                                DeviceUser = dr["DeviceUser"].ToString(),
                                ContactDetails = dr["ContactDetails"].ToString(),
                                SysTHReqUser = dr["SysTHReqUser"].ToString(),
                                THandoverid = dr["id"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving repair approval devices: " + ex.Message;
            }

            return devices;
        }



        [HttpPost]
        public ActionResult ApproveRepair(string IT_No, string THandoverid, string empname)
        {
            try
            {
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = '31', SysTHAppDate = @SysTHAppDate, SysTHAppUser = @SysTHAppUser WHERE id = @THandoverid";
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '31' WHERE IT_No = @IT_No";

                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (var sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        sqlcmd.ExecuteNonQuery();
                    }

                    using (var sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        sqlcmd1.Parameters.AddWithValue("@THandoverid", THandoverid);
                        sqlcmd1.Parameters.AddWithValue("@SysTHAppDate", DateTime.Now);
                        sqlcmd1.Parameters.AddWithValue("@SysTHAppUser", empname);
                        sqlcmd1.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, message = "Repair Request Successfully submitted!" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        [HttpPost]
        public ActionResult RejectRepair(string IT_No, string THandoverid, string empname, string CenterRejectReson)
        {
            try
            {
                string sqlUpdateHandover = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = '32', SysTHAppDate = @SysTHAppDate, SysTHAppUser = @SysTHAppUser, CenterRejectReson = @CenterRejectReson WHERE id = @THandoverid";
                string sqlUpdateDevice = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '1' WHERE IT_No = @IT_No";

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (var cmdDevice = new SqlCommand(sqlUpdateDevice, conn))
                    {
                        cmdDevice.Parameters.AddWithValue("@IT_No", IT_No);
                        cmdDevice.ExecuteNonQuery();
                    }

                    using (var cmdHandover = new SqlCommand(sqlUpdateHandover, conn))
                    {
                        cmdHandover.Parameters.AddWithValue("@THandoverid", THandoverid);
                        cmdHandover.Parameters.AddWithValue("@SysTHAppDate", DateTime.Now);
                        cmdHandover.Parameters.AddWithValue("@SysTHAppUser", empname);
                        cmdHandover.Parameters.AddWithValue("@CenterRejectReson", CenterRejectReson);
                        cmdHandover.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Repair Reject Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public JsonResult SentModal(string it, string ItemSentDate, string ItemSentType, string ItemSentRemaks, string empname, int? statusValueField)
        {
            // Check if the statusValueField is either "31" or "44"

            if (statusValueField == 31 || statusValueField == 44)
            {
                return UpdateHardwareData(it, ItemSentDate, ItemSentType, ItemSentRemaks, empname, statusValueField);
            }

            return Json(new { success = false, message = "Invalid status value." });
        }

        private JsonResult UpdateHardwareData(string it, string ItemSentDate, string ItemSentType, string ItemSentRemaks, string empname, int? statusValueField)
        {
            string updateHandoverSql = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = 33, ItemSentDate=@ItemSentDate , ItemSentType=@ItemSentType, ItemSentRemaks=@ItemSentRemaks, SysTSentUser  = @SysTSentUser , SysTSentDate=@SysTSentDate WHERE IT_No = @IT_No AND THandoverStatus=@THandoverStatus";
            string updateDevicesSql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 33 WHERE IT_No = @IT_No";

            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand updateHandoverCmd = new SqlCommand(updateHandoverSql, conn_uat))
                    {
                        updateHandoverCmd.Parameters.AddWithValue("@IT_No", it);
                        updateHandoverCmd.Parameters.AddWithValue("@ItemSentDate", ItemSentDate);
                        updateHandoverCmd.Parameters.AddWithValue("@ItemSentType", ItemSentType);
                        updateHandoverCmd.Parameters.AddWithValue("@ItemSentRemaks", ItemSentRemaks);
                        updateHandoverCmd.Parameters.AddWithValue("@SysTSentUser ", empname);
                        updateHandoverCmd.Parameters.AddWithValue("@SysTSentDate", DateTime.Now);
                        updateHandoverCmd.Parameters.AddWithValue("@THandoverStatus", statusValueField);

                        updateHandoverCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand updateDevicesCmd = new SqlCommand(updateDevicesSql, conn_uat))
                    {
                        updateDevicesCmd.Parameters.AddWithValue("@IT_No", it);
                        updateDevicesCmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Repair request successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }







        [HttpPost]
        public JsonResult PHOModal(string it, string THandoverRemaks, string location, string contact, string employee, string sysuser)
        {
            string insertSql = "INSERT INTO [TechAssets].[dbo].[Hardware_THandover] ([THandoverStatus], [IT_No], [THandoverType], [THandoverLocation], [DeviceUser], [Userfeedback], [ContactDetails], [THRequestDate], [SysTHReqUser], [SysTHReqDate]) " +
                "VALUES (@THandoverStatus, @IT_No, @THandoverType, @THandoverLocation, @DeviceUser, @THandoverRemaks, @ContactDetails, @PermenantlyHODate, @SysTHReqUser, @SysTHReqDate)";

            string updateSql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 43 WHERE IT_No = @IT_No";

            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand insertCmd = new SqlCommand(insertSql, conn_uat))
                    {
                        insertCmd.Parameters.AddWithValue("@THandoverStatus", 43);
                        insertCmd.Parameters.AddWithValue("@IT_No", it);
                        insertCmd.Parameters.AddWithValue("@THandoverType", 42);
                        insertCmd.Parameters.AddWithValue("@THandoverLocation", location);
                        insertCmd.Parameters.AddWithValue("@DeviceUser", employee);
                        insertCmd.Parameters.AddWithValue("@THandoverRemaks", THandoverRemaks);
                        insertCmd.Parameters.AddWithValue("@ContactDetails", contact);
                        insertCmd.Parameters.AddWithValue("@PermenantlyHODate", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@SysTHReqUser", sysuser);
                        insertCmd.Parameters.AddWithValue("@SysTHReqDate", DateTime.Now);

                        insertCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn_uat))
                    {
                        updateCmd.Parameters.AddWithValue("@IT_No", it);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Repair request successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }





        public ActionResult HandoverApproval(string status = null)
        {

            users user = TechAssetsModule.get_user_id(new users());
            techassetsM techUser = TechAssetsModule.GetUserInformation(user.emp_id);


            List<HardwareDevice> Appdevices = GetHandoverAppDevices(techUser.get_emp_division, status);


            var message = TempData["Message"] as string;
            ViewBag.SuccessMessage = message != null ? message : null;


            var model = new DevicesViewModel
            {
                TechUser = techUser,
                HardwareDevices = Appdevices,
            };

            return View(model);
        }


        private List<HardwareDevice> GetHandoverAppDevices(string get_emp_division, string status)
        {
            List<HardwareDevice> devices = new List<HardwareDevice>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();


                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = conn;


                sqlcmd.CommandText = "SELECT it.Item_Type, h.IT_No, h.INV_No, CONVERT(VARCHAR, ho.THRequestDate, 23) AS THRequestDate, ho.Userfeedback, ho.DeviceUser, ho.ContactDetails, ho.SysTHReqUser, ho.THandoverLocation, ho.id FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_THandover] ho ON h.IT_No = ho.IT_No WHERE ho.THandoverStatus = '43' AND ho.THandoverLocation = @get_emp_division";
                sqlcmd.Parameters.AddWithValue("@get_emp_division", get_emp_division);

                using (SqlDataReader dr = sqlcmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        devices.Add(new HardwareDevice
                        {
                            IT_No = dr["IT_No"].ToString(),
                            Item_Type = dr["Item_Type"].ToString(),
                            INV_No = dr["INV_No"].ToString(),
                            THRequestDate = DateTime.Parse(dr["THRequestDate"].ToString()).Date,
                            Userfeedback = dr["Userfeedback"].ToString(),
                            DeviceUser = dr["DeviceUser"].ToString(),
                            ContactDetails = dr["ContactDetails"].ToString(),
                            SysTHReqUser = dr["SysTHReqUser"].ToString(),
                            THandoverid = dr["id"].ToString()
                        });
                    }
                }
            }

            return devices;
        }




        [HttpPost]
        public ActionResult ApproveHandover(string IT_No, string THandoverid, string empname)
        {
            try
            {
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = '44', SysTHAppDate = @SysTHAppDate, SysTHAppUser = @SysTHAppUser WHERE id = @THandoverid";
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '44' WHERE IT_No = @IT_No";

                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (var sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        sqlcmd.ExecuteNonQuery();
                    }

                    using (var sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        sqlcmd1.Parameters.AddWithValue("@THandoverid", THandoverid);
                        sqlcmd1.Parameters.AddWithValue("@SysTHAppDate", DateTime.Now);
                        sqlcmd1.Parameters.AddWithValue("@SysTHAppUser", empname);
                        sqlcmd1.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, message = "Handover Request Successfully submitted!" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public ActionResult RejectHandover(string IT_No, string THandoverid, string empname, string CenterRejectReson)
        {
            try
            {
                string sqlUpdateHandover = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = '45', SysTHAppDate = @SysTHAppDate, SysTHAppUser = @SysTHAppUser, CenterRejectReson = @CenterRejectReson WHERE id = @THandoverid";
                string sqlUpdateDevice = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '1' WHERE IT_No = @IT_No";

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (var cmdDevice = new SqlCommand(sqlUpdateDevice, conn))
                    {
                        cmdDevice.Parameters.AddWithValue("@IT_No", IT_No);
                        cmdDevice.ExecuteNonQuery();
                    }

                    using (var cmdHandover = new SqlCommand(sqlUpdateHandover, conn))
                    {
                        cmdHandover.Parameters.AddWithValue("@THandoverid", THandoverid);
                        cmdHandover.Parameters.AddWithValue("@SysTHAppDate", DateTime.Now);
                        cmdHandover.Parameters.AddWithValue("@SysTHAppUser", empname);
                        cmdHandover.Parameters.AddWithValue("@CenterRejectReson", CenterRejectReson);
                        cmdHandover.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Handover Reject Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        public ActionResult UserChange(string status = null)
        {
            try
            {
                users user = TechAssetsModule.get_user_id(new users());
                techassetsM techUser = TechAssetsModule.GetUserInformation(user.emp_id);

                List<HardwareDevice> devices = GetUserChangeDevices(techUser.get_emp_division, status);

                var message = TempData["Message"] as string;
                ViewBag.SuccessMessage = message != null ? message : null;

                var model = new DevicesViewModel
                {
                    TechUser = techUser,
                    HardwareDevices = devices,
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving device information.";
                return View("Error");
            }
        }



        private List<HardwareDevice> GetUserChangeDevices(string get_emp_division, string status)
        {
            List<HardwareDevice> devices = new List<HardwareDevice>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();


                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = conn;

                sqlcmd.CommandText = "SELECT it.Item_Type, d.IT_No, d.Serial_No, d.INV_No, d.QR, ds.Status AS Current_Status_Text, d.Authorized_Officer FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.Purchased_DivisionID = sd.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.Current_LocationID = sd_loc.div_index WHERE d.activeStatus = 1 AND d.Current_LocationID = @get_emp_division AND d.Current_Status != 2";
                sqlcmd.Parameters.AddWithValue("@get_emp_division", get_emp_division);

                using (SqlDataReader dr = sqlcmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        devices.Add(new HardwareDevice
                        {

                            Item_Type = dr["Item_Type"].ToString(),
                            IT_No = dr["IT_No"].ToString(),
                            Serial_No = dr["Serial_No"].ToString(),
                            INV_No = dr["INV_No"].ToString(),
                            QR = dr["QR"].ToString(),
                            Current_Status = dr["Current_Status_Text"].ToString(),
                            Authorized_Officer = dr["Authorized_Officer"].ToString()

                        });
                    }
                }
            }

            return devices;
        }



        [HttpPost]
        public ActionResult UserChangeModal(string itNo2, string employee)
        {

            string updateSql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Authorized_Officer = @Authorized_Officer WHERE IT_No = @IT_No";

            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn_uat))
                    {
                        updateCmd.Parameters.AddWithValue("@IT_No", itNo2);
                        updateCmd.Parameters.AddWithValue("@Authorized_Officer", employee);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                TempData["Message"] = "Successfully submitted!";
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("UserChange", "TechAssets");
        }


        //New Changes




        [HttpPost]
        public JsonResult ReceivedConNew(string itNo, string hooneId, string empname, string ItemRecDate, string itemRecRemarks, string modalType)
        {
            if (modalType == "ConfirmModal")
            {
                string query = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET ConDdate = @ConDdate, HOComRemarks = @HOComRemarks, SysConDUser = @SysConDUser, SysConDate = @SysConDate, Current_Status = @Current_Status WHERE id = @id";
                string query1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Installation_Date = @Installation_Date, HoIDst = @HoIDst, Current_Status = @Current_Status WHERE IT_No = @IT_No";

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.Add("@ConDdate", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd.Parameters.Add("@HOComRemarks", SqlDbType.NVarChar).Value = itemRecRemarks;
                                cmd.Parameters.Add("@SysConDUser", SqlDbType.NVarChar).Value = empname;
                                cmd.Parameters.Add("@SysConDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@Current_Status", SqlDbType.Int).Value = 19;
                                cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = hooneId;

                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd1 = new SqlCommand(query1, conn, transaction))
                            {
                                cmd1.Parameters.Add("@Installation_Date", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd1.Parameters.Add("@HoIDst", SqlDbType.NVarChar).Value = DBNull.Value;
                                cmd1.Parameters.Add("@Current_Status", SqlDbType.Int).Value = 1;
                                cmd1.Parameters.Add("@IT_No", SqlDbType.NVarChar).Value = itNo;

                                cmd1.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }

                        return Json(new { success = true, message = "Update successful" });
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        return Json(new { success = false, message = "Error: " + ex.Message });
                    }
                }


            }
            else if (modalType == "ConfirmTModal")
            {
                string query = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET ConDdate = @ConDdate, ConDRemaks = @ConDRemaks, SysConDUser = @SysConDUser, SysConDate = @SysConDate, THandoverStatus = @THandoverStatus WHERE id = @id";
                string query1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET HoIDth = @HoIDth, Current_Status = @Current_Status WHERE IT_No = @IT_No";

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.Add("@ConDdate", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd.Parameters.Add("@ConDRemaks", SqlDbType.NVarChar).Value = itemRecRemarks;
                                cmd.Parameters.Add("@SysConDUser", SqlDbType.NVarChar).Value = empname;
                                cmd.Parameters.Add("@SysConDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@THandoverStatus", SqlDbType.Int).Value = 19;
                                cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = hooneId;

                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd1 = new SqlCommand(query1, conn, transaction))
                            {
                                cmd1.Parameters.Add("@HoIDth", SqlDbType.NVarChar).Value = DBNull.Value;
                                cmd1.Parameters.Add("@Current_Status", SqlDbType.Int).Value = 1;
                                cmd1.Parameters.Add("@IT_No", SqlDbType.NVarChar).Value = itNo;

                                cmd1.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }

                        return Json(new { success = true, message = "Update successful" });
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        return Json(new { success = false, message = "Error: " + ex.Message });
                    }
                
                }
            }          

            return Json(new { success = true, message = "Item received successfully!" });
        }




        [HttpPost]
        public JsonResult ReceivedConCou(string itNo, string hooneId, string empname, string ItemRecDate, string itemRecRemarks, string modalType, string cooneId)
        {
            if (modalType == "ConfirmNewModal")
            {
                string query = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET ConDdate = @ConDdate, HOComRemarks = @HOComRemarks, SysConDUser = @SysConDUser, SysConDate = @SysConDate, Current_Status = @Current_Status WHERE id = @id";
                string query1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Installation_Date = @Installation_Date, HoIDst = @HoIDst, CoID=@CoID, Current_Status = @Current_Status WHERE IT_No = @IT_No";
                string query3 = "UPDATE [TechAssets].[dbo].[Hardware_Courier] SET CompleteDate = @CompleteDate, CompleteRemaks = @CompleteRemaks, ConfirmUser = @ConfirmUser, ConfirmDate=@ConfirmDate, CourierStatus=@CourierStatus WHERE id = @id";

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.Add("@ConDdate", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd.Parameters.Add("@HOComRemarks", SqlDbType.NVarChar).Value = itemRecRemarks;
                                cmd.Parameters.Add("@SysConDUser", SqlDbType.NVarChar).Value = empname;
                                cmd.Parameters.Add("@SysConDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@Current_Status", SqlDbType.Int).Value = 19;
                                cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = hooneId;

                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd1 = new SqlCommand(query1, conn, transaction))
                            {
                                cmd1.Parameters.Add("@Installation_Date", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd1.Parameters.Add("@HoIDst", SqlDbType.NVarChar).Value = DBNull.Value;
                                cmd1.Parameters.Add("@CoID", SqlDbType.NVarChar).Value = DBNull.Value;
                                cmd1.Parameters.Add("@Current_Status", SqlDbType.Int).Value = 1;
                                cmd1.Parameters.Add("@IT_No", SqlDbType.NVarChar).Value = itNo;

                                cmd1.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd3 = new SqlCommand(query3, conn, transaction))
                            {
                                cmd3.Parameters.Add("@CompleteDate", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd3.Parameters.Add("@CompleteRemaks", SqlDbType.NVarChar).Value = itemRecRemarks;
                                cmd3.Parameters.Add("@ConfirmUser", SqlDbType.NVarChar).Value = empname;
                                cmd3.Parameters.Add("@ConfirmDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd3.Parameters.Add("@CourierStatus", SqlDbType.Int).Value = 14;
                                cmd3.Parameters.Add("@id", SqlDbType.NVarChar).Value = cooneId;

                                cmd3.ExecuteNonQuery();
                            }


                            transaction.Commit();
                        }

                        return Json(new { success = true, message = "Update successful" });
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        return Json(new { success = false, message = "Error: " + ex.Message });
                    }
                }


            }
            else if (modalType == "ConfirmReModal")
            {
                string query = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET ConDdate = @ConDdate, ConDRemaks = @ConDRemaks, SysConDUser = @SysConDUser, SysConDate = @SysConDate, THandoverStatus = @THandoverStatus WHERE id = @id";
                string query1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET HoIDth = @HoIDth, CoID=@CoID, Current_Status = @Current_Status WHERE IT_No = @IT_No";
                string query3 = "UPDATE [TechAssets].[dbo].[Hardware_Courier] SET CompleteDate = @CompleteDate, CompleteRemaks = @CompleteRemaks, ConfirmUser = @ConfirmUser, ConfirmDate=@ConfirmDate, CourierStatus=@CourierStatus WHERE id = @id";

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.Add("@ConDdate", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd.Parameters.Add("@ConDRemaks", SqlDbType.NVarChar).Value = itemRecRemarks;
                                cmd.Parameters.Add("@SysConDUser", SqlDbType.NVarChar).Value = empname;
                                cmd.Parameters.Add("@SysConDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd.Parameters.Add("@THandoverStatus", SqlDbType.Int).Value = 19;
                                cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = hooneId;

                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd1 = new SqlCommand(query1, conn, transaction))
                            {
                                cmd1.Parameters.Add("@HoIDth", SqlDbType.NVarChar).Value = DBNull.Value;
                                cmd1.Parameters.Add("@CoID", SqlDbType.NVarChar).Value = DBNull.Value;
                                cmd1.Parameters.Add("@Current_Status", SqlDbType.Int).Value = 1;
                                cmd1.Parameters.Add("@IT_No", SqlDbType.NVarChar).Value = itNo;

                                cmd1.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd3 = new SqlCommand(query3, conn, transaction))
                            {
                                cmd3.Parameters.Add("@CompleteDate", SqlDbType.DateTime).Value = ItemRecDate;
                                cmd3.Parameters.Add("@CompleteRemaks", SqlDbType.NVarChar).Value = itemRecRemarks;
                                cmd3.Parameters.Add("@ConfirmUser", SqlDbType.NVarChar).Value = empname;
                                cmd3.Parameters.Add("@ConfirmDate", SqlDbType.DateTime).Value = DateTime.Now;
                                cmd3.Parameters.Add("@CourierStatus", SqlDbType.Int).Value = 14;
                                cmd3.Parameters.Add("@id", SqlDbType.NVarChar).Value = cooneId;

                                cmd3.ExecuteNonQuery();
                            }


                            transaction.Commit();
                        }

                        return Json(new { success = true, message = "Update successful" });
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        return Json(new { success = false, message = "Error: " + ex.Message });
                    }

                }
            }

            return Json(new { success = true, message = "Item received successfully!" });
        }










    }
}


