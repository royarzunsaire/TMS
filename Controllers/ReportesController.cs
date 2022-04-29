using jsreport.MVC;
using jsreport.Types;
using SGC.CustomAuthorize;
using SGC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SGC.Controllers
{
    [Authorize]
    [CustomAuthorize(new string[] { "/Reportes/" })]
    public class ReportesController : Controller
    {
        private InsecapContext db = new InsecapContext();

        // GET: Reportes
        public ActionResult Index()
        {
            return View();
        }

        // GET: Reportes/ReporteList
        public ActionResult ReporteList()
        {
            var cursos = GetCursos();
            return View(cursos);
        }

        [EnableJsReport()]
        public ActionResult ReporteListExcel()
        {
            var cursos = GetCursos();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"cursos.xlsx\"");
            return View(cursos);
        }

        // GET: Reportes/ReporteRelatoresCurso
        public ActionResult ReporteRelatoresCurso()
        {
            //ViewBag.cursos = GetCursosList();
            //ViewBag.relatores = GetRelatores();
            return View(db.RelatorCurso
                .Where(x => x.softDelete == false)
                .ToList());
        }

        [EnableJsReport()]
        public ActionResult ReporteRelatoresCursoExcel()
        {
            var relatoresCursos = db.RelatorCurso
                .Where(x => x.softDelete == false)
                .ToList();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"instructores_cursos.xlsx\"");
            return View(relatoresCursos);
        }

        //[CustomAuthorize(new string[] { "/Curso/" })]
        //[EnableJsReport()]
        //public ActionResult R11s()
        //{
        //    var r11s = GetR11s();
        //    HttpContext
        //        .JsReportFeature()
        //        .Recipe(Recipe.HtmlToXlsx)
        //        .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"nuevos_r11.xlsx\"");
        //    return View(r11s);
        //}

        //private List<ViewModelCursoR11> GetR11s()
        //{
        //    return db.R11
        //        .Where(c => c.softDelete == false)
        //        .Join(
        //            db.Curso,
        //            r11 => r11.idCurso,
        //            curso => curso.idCurso,
        //            (r11, curso) => new ViewModelCursoR11()
        //            {
        //                r11 = r11,
        //                curso = curso
        //            }
        //        ).ToList();
        //}

        //[CustomAuthorize(new string[] { "/Curso/" })]
        //[EnableJsReport()]
        //public ActionResult CursosCompletos()
        //{
        //    var cursos = GetCursosCompletos();
        //    HttpContext
        //        .JsReportFeature()
        //        .Recipe(Recipe.HtmlToXlsx)
        //        .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"cursos_completos_.xlsx\"");
        //    return View(cursos);
        //}

        //private List<ViewModelCursoR51> GetCursosCompletos()
        //{
        //    return db.Curso
        //        .Where(c => c.softDelete == false)
        //        .Where(c => c.materialCompleto)
        //        .Join(
        //           db.R51,
        //           curso => curso.idCurso,
        //           r51 => r51.idCurso,
        //           (curso, r51) => new ViewModelCursoR51()
        //           {
        //               curso = curso,
        //               r51 = r51
        //           }
        //        )
        //        .ToList();
        //}

        private List<ViewModelCurso> GetCursos()
        {
            var cursos = db.Curso.Where(c => c.softDelete == false).ToList();
            var lista = new List<ViewModelCurso>();
            foreach (var item in cursos)
            {
                var cursoVM = new ViewModelCurso();
                cursoVM.curso = item;
                cursoVM.r11 = db.R11.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                var r12 = db.CostoCursoR12.Where(r => r.idCurso == item.idCurso).FirstOrDefault();
                if (r12 != null)
                {
                    r12.costoParticularCurso = db.CostoParticularCurso.Where(x => x.idCostoCursoR12 == r12.idCostoCursoR12).ToList();
                }
                cursoVM.r12 = r12;
                lista.Add(cursoVM);
            }
            return lista;
        }

        //public SelectList GetRelatores()
        //{
        //    List<Relator> relatores = db.Relators
        //        .Where(r => r.softDelete == false)
        //        .ToList();
        //    return new SelectList(relatores
        //        .Select(r => new SelectListItem
        //        {
        //            Text = "[" + r.contacto.run + "]" + " " + r.contacto.nombres + " " + r.contacto.apellidoPaterno + " " + r.contacto.apellidoMaterno,
        //            Value = r.idRelator.ToString()
        //        }).ToList(), "Value", "Text");
        //}

        //public SelectList GetCursosList()
        //{
        //    List<Curso> cursos = db.Curso
        //        .Where(c => c.softDelete == false)
        //        .ToList();
        //    return new SelectList(cursos
        //        .Select(c => new SelectListItem
        //        {
        //            Text = c.nombreCurso,
        //            Value = c.idCurso.ToString()
        //        }).ToList(), "Value", "Text");
        //}


        // GET: Reportes/ReporteRelatoresCurso

        public ActionResult RelatoresSenceMes()
        {
            return View(GetNuevosRelatoresSenceMensual());
        }

        //[EnableJsReport()]
        //public ActionResult RelatoresSenceMesExcel(string id)
        //{
        //    var hoy = DateTime.Now;
        //    var relatorCurso = GetNuevosRelatoresSenceMensual();
        //    HttpContext
        //        .JsReportFeature()
        //        .Recipe(Recipe.HtmlToXlsx)
        //        .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"relatores_sence_inscritos_mes.xlsx\"");
        //    return View(relatorCurso);
        //}

        private List<RelatorCurso> GetNuevosRelatoresSenceMensual()
        {
            var hoy = DateTime.Now;
            return db.RelatorCurso
                .Where(c => c.softDelete == false)
                .Where(c => c.validoSence)
                .Where(c => c.curso.softDelete == false)
                .Where(c => c.relator.softDelete == false)
                .Where(c => c.fechaValidoSence.Value.Month == hoy.Month && c.fechaValidoSence.Value.Year == hoy.Year)
                .ToList();
        }

        public ActionResult CursosSenceMes()
        {
            return View(GetCursosSenceMes());
        }

        [EnableJsReport()]
        public ActionResult CursosSenceMesExcel(string id)
        {
            var r11s = GetCursosSenceMes();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"cursos_sence_mes.xlsx\"");
            return View(r11s);
        }

        private List<ViewModelCursoR11> GetCursosSenceMes()
        {
            var hoy = DateTime.Now;
            return db.R11
                .Where(c => c.fechaCreacion.Month == hoy.Month && c.fechaCreacion.Year == hoy.Year)
                .Where(c => c.softDelete == false)
                .Where(c => c.codigoSence != null && c.codigoSence != "")
                .Join(
                    db.Curso,
                    r11 => r11.idCurso,
                    curso => curso.idCurso,
                    (r11, curso) => new ViewModelCursoR11()
                    {
                        r11 = r11,
                        curso = curso
                    }
                ).ToList();
        }

        public ActionResult MonitoreoCursos()
        {
            return View(GetR52());
        }

        [EnableJsReport()]
        public ActionResult MonitoreoCursosExcel(string id)
        {
            var r52s = GetR52();
            HttpContext
                .JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"monitoreo_cursos.xlsx\"");
            return View(r52s);
        }

        private List<R52> GetR52()
        {
            var r52 = db.R52.ToList();
            r52 = r52
                .Where(x => x.encuesta.respuestas.Count() != 0)
                .ToList();
            return r52;
        }
    }
}