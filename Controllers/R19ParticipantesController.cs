namespace SGC.Controllers
{
    //public class R19ParticipantesController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();

    //    // GET: R19
    //    public ActionResult Index()
    //    {

    //        var cantidad = db.AspNetUsers.Where(x => x.Email == User.Identity.Name).ToList().Count();
    //        if (cantidad == 0)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        var usuario = db.AspNetUsers.Where(x => x.Email == User.Identity.Name).ToList().First();

    //        cantidad = db.Contacto.Where(x => x.usuario.Id == usuario.Id).ToList().Count();

    //        if (cantidad == 0)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        Contacto contacto = db.Contacto.Where(x => x.usuario.Id == usuario.Id).ToList()[0];
    //        var idComercializaciones = db.Participante.Where(x => x.contacto.idContacto == contacto.idContacto).Select(y => y.comercializacion.idComercializacion).ToList();
    //        List<ViewModelR19Participantes> listViewModel = new List<ViewModelR19Participantes>();
    //        var comercializaciones = db.Comercializacion.Where(x => idComercializaciones.Contains(x.idComercializacion)).ToList();

    //        foreach (var item in comercializaciones)
    //        {
    //            ViewModelR19Participantes viewModel = new ViewModelR19Participantes();
    //            viewModel.nombreCurso = item.cotizacion.curso.nombreCurso;
    //            viewModel.idComercializacion = item.idComercializacion;
    //            viewModel.idContacto = contacto.idContacto;

    //            //int idParticipante = db.Participante.Where(x => x.comercializacion.idComercializacion == item.idComercializacion).Where(y => y.contacto.idContacto == contacto.idContacto).First().idParticipante;
    //            //int idTipoFormulario = db.TipoFormulario.Where(y => y.tipo == "R19").ToList().First().idTipoFormulario;
    //            //var idPreguntaFormulario = db.Formulario.Where(x => x.tipoFormulario == TipoFormulario.R19).First().preguntasFormularios.First().idPreguntasFormulario;
    //            //var idsRespuestasFormularios = db.RespuestasFormulario.Where(x => x.idPreguntasFormulario == idPreguntaFormulario).Select(v => v.idRespuestasFormulario);

    //            //if (db.RespuestasContestadasFormulario.Where(x => idsRespuestasFormularios.Contains(x.idRespuestasFormulario)).ToList().Count > 0)
    //            //if (db.RespuestasContestadasFormulario.Where(x => idsRespuestasFormularios.Contains(x.idRespuestasFormulario)).ToList().Count > 0)
    //            //{
    //            //    viewModel.yaRespondida = true;
    //            //}
    //            //else
    //            //{
    //            //    viewModel.yaRespondida = false;
    //            //}

    //            listViewModel.Add(viewModel);
    //        }

    //        return View(listViewModel);
    //    }

    //    // GET: R19/Details/5
    //    public ActionResult Details(int id)
    //    {
    //        return View();
    //    }

    //    // GET: R19/Create
    //    public ActionResult Create(int idContacto, int idComercializacion)
    //    {

    //        ViewModelRespuestaFormulario viewModel = new ViewModelRespuestaFormulario();
    //        viewModel.idContacto = idContacto;
    //        //int idTipoFormulario = db.TipoFormulario.Where(y => y.tipo == "R19").ToList().First().idTipoFormulario;
    //        //viewModel.formulario = db.Formulario.Where(x => x.tipoFormulario == TipoFormulario.R19).First();
    //        //var idsPreguntasFormulario = db.PreguntasFormulario.Where(c=> c.idFormulario == viewModel.formulario.idFormulario).Select(x => x.idPreguntasFormulario);
    //        //viewModel.respuestas = db.RespuestasFormulario.Where(x => idsPreguntasFormulario.Contains(x.idPreguntasFormulario)).OrderBy(y => y.orden).ToList();
    //        return View(viewModel);
    //    }

    //    // POST: R19/Create
    //    [HttpPost]
    //    public ActionResult Create(ViewModelRespuestaFormulario vm)
    //    {
    //        try
    //        {
    //            // TODO: Add insert logic here
    //            foreach(var respuestaContestada in vm.respuestasContestadas)
    //            {
    //                respuestaContestada.contacto = db.Contacto.Find(vm.idContacto);
    //                db.RespuestasContestadasFormulario.Add(respuestaContestada);
    //                db.SaveChanges();
    //            }

    //            return RedirectToAction("Index");
    //        }
    //        catch
    //        {
    //            return View();
    //        }
    //    }

    //    // GET: R19/Edit/5
    //    public ActionResult Edit(int id)
    //    {
    //        return View();
    //    }

    //    // POST: R19/Edit/5
    //    [HttpPost]
    //    public ActionResult Edit(int id, FormCollection collection)
    //    {
    //        try
    //        {
    //            // TODO: Add update logic here

    //            return RedirectToAction("Index");
    //        }
    //        catch
    //        {
    //            return View();
    //        }
    //    }

    //    // GET: R19/Delete/5
    //    public ActionResult Delete(int id)
    //    {
    //        return View();
    //    }

    //    // POST: R19/Delete/5
    //    [HttpPost]
    //    public ActionResult Delete(int id, FormCollection collection)
    //    {
    //        try
    //        {
    //            // TODO: Add delete logic here

    //            return RedirectToAction("Index");
    //        }
    //        catch
    //        {
    //            return View();
    //        }
    //    }
    //}
}
