using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApi.Models
{
   public class OwnerDetailsModel
    {
        public string Odatacontext { get; set; }
        public object[] BusinessPhones { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public object JobTitle { get; set; }
        public string Mail { get; set; }
        public object MobilePhone { get; set; }
        public object OfficeLocation { get; set; }
        public object PreferredLanguage { get; set; }
        public string Surname { get; set; }
        public string UserPrincipalName { get; set; }
        public string Id { get; set; }
    }
}
