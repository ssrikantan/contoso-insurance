using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoInsAuthorityAdminPortal.Models
{
    /// <summary>
    /// This data goes into the Secret stored in Azure Key Vault
    /// </summary>
    public class Insdata
    {
      
        public string Id  { get;set; }
        public string Inscompany { get; set; }
        public string Policyno { get; set; }
        public string Vehicleno { get; set; }
        public string Userid { get; set; }

    }
}
