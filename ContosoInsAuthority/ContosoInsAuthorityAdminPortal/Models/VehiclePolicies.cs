using System;
using System.Collections.Generic;

namespace ContosoInsAuthorityAdminPortal.Models
{
    public partial class VehiclePolicies
    {
        public string Id { get; set; }
        public string Uidname { get; set; }
        public string Version { get; set; }
        public string Inscompany { get; set; }
        public string Policyno { get; set; }
        public string Vehicleno { get; set; }
        public string Userid { get; set; }
        public string Status { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime? Lastmod { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
    }
}
