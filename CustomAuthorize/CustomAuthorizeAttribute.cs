using SGC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SGC.CustomAuthorize
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public string ViewName { get; set; }
        public string[] urls { get; set; }
        //public int[] menuId { get; set; }
        private InsecapContext db = new InsecapContext();

        public CustomAuthorizeAttribute(string[] recivirUrls)
        {
            ViewName = "AuthorizeFailed";
            urls = recivirUrls;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            IsUserAuthorized(filterContext);
        }

        void IsUserAuthorized(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                if (!ValidarAcceso(filterContext))
                {
                    ViewDataDictionary dic = new ViewDataDictionary();
                    dic.Add("Message", "");
                    var result = new ViewResult() { ViewName = this.ViewName, ViewData = dic };
                    filterContext.Result = result;
                }
            }
        }

        private bool ValidarAcceso(AuthorizationContext filterContext)
        {
            db = new InsecapContext();
            AspNetUsers user = db.AspNetUsers.Where(u => u.UserName == filterContext.HttpContext.User.Identity.Name).FirstOrDefault();
            var ok = false;
            foreach (AspNetRoles role in user.AspNetRoles)
            {
                foreach (Permission permission in role.Permission)
                {
                    foreach (var url in urls)
                    {
                        if (permission.Menu.MenuURL == url)
                        {
                            ok = true;
                        }
                    }
                }
            }
            foreach (CustomPermission customPermission in user.CustomPermission)
            {
                foreach (var url in urls)
                {
                    if (customPermission.Menu.MenuURL == url)
                    {
                        ok = true;
                    }
                }
            }
            return ok;
        }
    }
}