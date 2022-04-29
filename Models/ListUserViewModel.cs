using System.Collections.Generic;

namespace SGC.Models
{
    public class ListUserViewModel
    {
        public IList<SGC.Models.ApplicationUser> users { get; set; }
        public IList<string> roles { get; set; }
    }
}