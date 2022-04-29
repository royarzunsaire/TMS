using Microsoft.AspNet.Identity;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    public class PermissionsController : Controller
    {
        private InsecapContext db2 = new InsecapContext();

        [CustomAuthorize(new string[] { "/Permissions/" })]
        public ActionResult Index()
        {
            ViewBag.RoleID = new SelectList(db2.AspNetRoles, "Id", "Name");
            ViewBag.UserID = new SelectList(db2.AspNetUsers, "Id", "UserName");
            IEnumerable<PermissionMenu> menus = db2.Database.SqlQuery<PermissionMenu>("SP_GetMenu @UserId ='" + User.Identity.GetUserId() + "', @RoleId = NULL");
            ViewBag.Menus = from a in db2.Menu
                            where a.MenuID == 0
                            select a;
            var permission = db2.Permission.Include(r => r.Menu).Include(s => s.AspNetRoles);
            return View(permission.ToList());
        }

        [CustomAuthorize(new string[] { "/Permissions/" })]
        [HttpPost]
        public ActionResult Index(string RoleID, string UserID)
        {

            IEnumerable<PermissionMenu> menus = db2.Database.SqlQuery<PermissionMenu>("SP_GetMenu @UserId ='" + User.Identity.GetUserId() + "',@RoleId = NULL");
            if (!string.IsNullOrEmpty(UserID))
            {
                menus = null;
                var us = db2.AspNetUsers.Where(u => u.Id == UserID).FirstOrDefault();

                menus = db2.Database.SqlQuery<PermissionMenu>("SP_GetMenu @UserId ='" + UserID + "', @RoleId = NULL");
                IEnumerable<PermissionMenu> menusRoles = null;
                menusRoles = db2.Database.SqlQuery<PermissionMenu>("SP_GetMenu @UserId =NULL, @RoleId = '" + us.AspNetRoles.FirstOrDefault().Id + "'");
                var lroles = menusRoles.ToList();
                var Menus = menus.ToList();

                foreach (var m in menus.ToList())
                {

                    foreach (var mrole in menusRoles.ToList())
                    {
                        if (m.MenuID == mrole.MenuID & mrole.Permission == true)
                        {
                            Menus.Find(p => p.MenuID == mrole.MenuID).Permission = true;
                            Menus.Find(p => p.MenuID == mrole.MenuID).PermissionType = 0;
                        }
                    }

                }
                ViewBag.Menus = Menus;
                ViewBag.mode = "user";
            }
            if (!string.IsNullOrEmpty(RoleID))
            {
                menus = null;
                menus = db2.Database.SqlQuery<PermissionMenu>("SP_GetMenu @UserId =NULL, @RoleId = '" + RoleID + "'");
                ViewBag.mode = "role";
                ViewBag.Menus = menus.ToList();
            }

            ViewBag.RoleID = new SelectList(db2.AspNetRoles, "Id", "Name");
            ViewBag.UserID = new SelectList(db2.AspNetUsers, "Id", "UserName");


            return View();
        }

        [CustomAuthorize(new string[] { "/Permissions/" })]
        [HttpPost]
        public ActionResult SavePermission(string query, string qtype)
        {
            string fquery = "{\"PERMISSION\":" + query + "}";
            var data = Newtonsoft.Json.Linq.JObject.Parse(fquery);
            String roleId = Convert.ToString(data["PERMISSION"][0]["RoleID"]);
            String userId = Convert.ToString(data["PERMISSION"][0]["UserID"]);
            if (qtype == "r")
            {

                /*DELETING ROWS*/



                var rows = from r in db2.Permission
                           where r.RoleID == roleId
                           select r;
                foreach (var row in rows)
                {
                    db2.Permission.Remove(row);
                }
                db2.SaveChanges();

                int menu;
                string role;
                for (int i = 0; i < data["PERMISSION"].Count(); i++)
                {
                    menu = (int)data["PERMISSION"][i]["MenuID"];
                    role = (string)data["PERMISSION"][i]["RoleID"];

                    /* IEnumerable<CustomPermission> cp = db2.CustomPermission.Where(s => s.MenuID == menu );

                     foreach (CustomPermission _cp in cp) {
                         db2.CustomPermission.Remove(_cp);

                     }
                     db2.SaveChanges();*/


                    IEnumerable<CustomPermission> deleteQuery = db2.Database.SqlQuery<CustomPermission>(
                        "select CustomPermissionID, cp.UserID,MenuID " +
                        "from [DB_SGC].[dbo].[CustomPermission] cp " +
                        "join[DB_SGC].[dbo].[AspNetUserRoles] ur on ur.UserId = cp.UserID " +
                        "where RoleId = '7e43834d-6737-4a7b-bc0d-beea25c33d6d' and cp.MenuID = 1").ToList();
                    foreach (var delete2 in deleteQuery)
                    {
                        db2.CustomPermission.Attach(delete2);
                        db2.CustomPermission.Remove(delete2);
                        db2.SaveChanges();
                    }
                    db2.Permission.Add(new Permission { MenuID = menu, RoleID = role });

                }
                try
                {
                    db2.SaveChanges();
                    TempData["ResultMessage"] = "Los datos se guardaron correctamente";
                    TempData["ResultType"] = "S";
                }
                catch (Exception ex)
                {

                    TempData["ResultMessage"] = "Error al guardar los datos! " + ex.Message;
                    TempData["ResultType"] = "E";
                }
            }
            if (qtype == "u")
            {
                AspNetUsers us = db2.AspNetUsers.Find(userId);
                String roleID = us.AspNetRoles.First().Id;
                var rows = from r in db2.CustomPermission
                           where r.UserID == userId
                           select r;
                foreach (var row in rows)
                {
                    db2.CustomPermission.Remove(row);
                }


                db2.SaveChanges();


                int menu;
                string user;
                for (int i = 0; i < data["PERMISSION"].Count(); i++)
                {
                    menu = (int)data["PERMISSION"][i]["MenuID"];
                    user = (String)data["PERMISSION"][i]["UserID"];
                    if (db2.Permission.Where(s => s.RoleID == roleID && s.MenuID == menu).Count() == 0)
                    {
                        db2.CustomPermission.Add(new CustomPermission { MenuID = menu, UserID = user });
                    }
                }
                try
                {
                    db2.SaveChanges();
                    TempData["ResultMessage"] = "Los datos se guardaron correctamente";
                    TempData["ResultType"] = "S";
                }
                catch (Exception ex)
                {

                    TempData["ResultMessage"] = "Error al guardar los datos! " + ex.Message;
                    TempData["ResultType"] = "E";
                }

            }


            return RedirectToAction("Index");
        }

        // GET: Permisions
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/" })]
        public ActionResult MenuIndex()
        {
            return View(db2.Menu.ToList());
        }

        // GET: Permisions/Details/5
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/" })]
        public ActionResult MenuDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db2.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // GET: Permisions/Create
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/", "/Permissions/MenuCreate/" })]
        public ActionResult MenuCreate()
        {
            /* List<SelectListItem> items = new SelectList(CurrentViewSetups, "SetupId", "SetupName", setupid).ToList();
             items.Insert(0, (new SelectListItem { Text = "[None]", Value = "0" }));
             ViewData["SetupsSelectList"] = items;
             */
            List<SelectListItem> parentMenus = db2.Menu.Where(m => m.ParentMenuID == 0).Select(c => new SelectListItem
            {
                Text = c.DisplayName.ToString(),
                Value = c.MenuID.ToString()
            }).ToList();
            parentMenus.Insert(0, (new SelectListItem { Text = "Menu sin padre", Value = "0" }));

            ViewBag.ListParentMenus = new SelectList(parentMenus, "Value", "Text");
            ViewBag.urls = GetUrls();
            return View();
        }

        private SelectList GetUrls()
        {
            return new SelectList(db2.Url.Select(c => new SelectListItem
            {
                Text = c.url,
                Value = c.url
            }).ToList(), "Value", "Text");
        }

        // POST: Permisions/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/", "/Permissions/MenuCreate/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MenuCreate(Menu menu)
        {
            List<SelectListItem> parentMenus = db2.Menu.Where(m => m.ParentMenuID == 0).Select(c => new SelectListItem
            {
                Text = c.DisplayName.ToString(),
                Value = c.MenuID.ToString()
            }).ToList();

            parentMenus.Insert(0, (new SelectListItem { Text = "Menu sin padre", Value = "0" }));

            ViewBag.ListParentMenus = new SelectList(parentMenus, "Value", "Text");

            if (ModelState.IsValid)
            {
                db2.Menu.Add(menu);
                db2.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.urls = GetUrls();
            return View(menu);
        }

        // GET: Permisions/Edit/5
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/" })]
        public ActionResult MenuEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db2.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> parentMenus = db2.Menu.Where(m => m.ParentMenuID == 0).Select(c => new SelectListItem
            {
                Text = c.DisplayName.ToString(),
                Value = c.MenuID.ToString()
            }).ToList();
            parentMenus.Insert(0, (new SelectListItem { Text = "Menu sin padre", Value = "0" }));
            ViewBag.ListParentMenus = new SelectList(parentMenus, "Value", "Text", menu.ParentMenuID);
            ViewBag.urls = GetUrls();
            return View(menu);
        }

        // POST: Permisions/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/" })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MenuEdit([Bind(Include = "MenuID,DisplayName,ParentMenuID,OrderNumber,MenuURL,MenuIcon")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                db2.Entry(menu).State = EntityState.Modified;
                db2.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SelectListItem> parentMenus = db2.Menu.Where(m => m.ParentMenuID == 0).Select(c => new SelectListItem
            {
                Text = c.DisplayName.ToString(),
                Value = c.MenuID.ToString()
            }).ToList();
            parentMenus.Insert(0, (new SelectListItem { Text = "Menu sin padre", Value = "0" }));
            ViewBag.ListParentMenus = new SelectList(parentMenus, "Value", "Text");
            ViewBag.urls = GetUrls();
            return View(menu);
        }

        // GET: Permisions/Delete/5
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/" })]
        public ActionResult MenuDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db2.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: Permisions/Delete/5
        [CustomAuthorize(new string[] { "/Permissions/", "/Permissions/MenuIndex/" })]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult MenuDeleteConfirmed(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                IEnumerable<Permission> p = db2.Permission.Where(s => s.MenuID == id);
                foreach (Permission CurrentPermission in p)
                {
                    db2.Permission.Remove(db2.Permission.Find(CurrentPermission.PermissionID));
                }
                db2.SaveChanges();
                Menu menu = db2.Menu.Find(id);
                db2.Menu.Remove(menu);

                db2.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db2.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
