using System.Collections.Generic;

namespace SGC.Models
{
    public class RolesUsersViewModels
    {

        public IList<SGC.Models.ApplicationUser> users { get; set; }
        public string role { get; set; }

    }
}