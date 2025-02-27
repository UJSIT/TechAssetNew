using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace TechAssets.Models
{
    public class DevicesViewModel
    {
        public techassetsM TechUser { get; set; }
        public List<HardwareDevice> HardwareDevices { get; set; }
        public string Message { get; set; }

        public string RepairApprovalTBL { get; set; }

        internal static string Get_Any_DT(string sql)
        {
            throw new NotImplementedException();
        }
    }

    public class HardwareDevice
    {
       
        public string IT_No { get; set; }
        public string Item_Type { get; set; }
        public string Serial_No { get; set; }
        public string INV_No { get; set; }
        public string QR { get; set; }
        public string Purchased_Division { get; set; }
        public string Current_Location { get; set; }
        public string Current_Status { get; set; }
        public string Authorized_Officer { get; set; }
        public DateTime? Installation_Date { get; set; }
        public string Current_Status_Text { get; set; }

        public string get_emp_division { get; set; }

        public string id { get; set; }

        public string THandoverLocation { get; set; }
        public string THandoverRemaks { get; set; }
        public DateTime PermenantlyHODate { get; set; }
        public string PermenantlyHOUser { get; set; }
        public DateTime THRequestDate { get; set; }
        public string DeviceUser { get; set; }
        public string Userfeedback { get; set; }
        public string ContactDetails { get; set; }
        public DateTime SysTHReqDate { get; set; }
        public string SysTHAppUser { get; set; }

        public string SysTHReqUser { get; set; }

        public string RepairRequestDate1 { get; set; }

        public string THandoverid { get; set; }

        public string CenterRejectReson { get; set; }

        public DateTime ItemSentDate { get; set; }

        public string ItemSentType { get; set; }

        public string ItemSentRemaks { get; set; }

        public int statusValueField { get; set; }


        //NewChanges
        public string HoIDst { get; set; }
        public string HoIDth { get; set; }
        public string CoID { get; set; }

        public string itNo { get; set; }
        public string hooneId { get; set; }

        public string ItemRecDate { get; set; }

        public string itemRecRemarks { get; set; }
    }
}
