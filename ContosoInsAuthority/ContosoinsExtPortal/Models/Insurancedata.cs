using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoinsExtPortal.Models
{
    public class InsuranceData
    {
        public string Id { get; set; }
        public string Inscompany { get; set; }
        public string Policyno { get; set; }
        public string Vehicleno { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        //   public string UserId { get; set; }
        public string qrcodeData { get; set; }
       
    }
}
