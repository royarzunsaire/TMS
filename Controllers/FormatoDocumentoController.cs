namespace SGC.Controllers
{
    //public class FormatoDocumentoController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();
    //    private static List<FormatoDocumento> lstBlobs;
    //    // GET: FormatoDocumento
    //    public ActionResult Index()
    //    {
    //        var jksjdk = DisplayBlobFiles();
    //        return View(DisplayBlobFiles());
    //    }

    //    private List<FormatoDocumento> DisplayBlobFiles()
    //    {
    //        var policyName = "testPolicy";
    //        var containerName = "test";

    //        //Conectar nuestra cuenta de storage y crear un nuevo cliente blob
    //        var connectionString = ConfigurationManager.AppSettings["ConnectionStringBlob"].ToString();
    //        var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
    //        var blobClient = storageAccount.CreateCloudBlobClient();
    //        // Obtener referencia al contenedor.
    //        var blobContainerRef = blobClient.GetContainerReference(containerName);
    //        blobContainerRef.CreateIfNotExists();
    //        //Se crea politica con tiempos y permisos necesarios.
    //        var storedPolicy = new SharedAccessBlobPolicy()
    //        {
    //            SharedAccessExpiryTime = DateTime.UtcNow.AddHours(10),
    //            Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
    //        };
    //        var permissions = blobContainerRef.GetPermissions();

    //        //Limpia cualquier politica existente del contenedor.
    //        permissions.SharedAccessPolicies.Clear();
    //        //Agregar nuevo permisos
    //        permissions.SharedAccessPolicies.Add(policyName, storedPolicy);
    //        // guardar nuevamente el contenedor.
    //        blobContainerRef.SetPermissions(permissions);
    //        var containerSignature = blobContainerRef.GetSharedAccessSignature(null, policyName);
    //        var uri = blobContainerRef.Uri + containerSignature;

    //        // ------------------------------------------------------------
    //        lstBlobs = new List<FormatoDocumento>();

    //        //string blobContainerSasUri = ConfigurationManager.AppSettings["BlobContainerSasUri"].ToString();
    //        string blobContainerSasUri = uri;
    //        Uri blobContainerUri = new Uri(blobContainerSasUri);

    //        //Uri blobContainerUri = new Uri(blobContainerSasUri);
    //        CloudBlobContainer blobContainer = new CloudBlobContainer(blobContainerUri);

    //        //var listOfBlobItems = blobContainer.ListBlobs().OfType<Microsoft.WindowsAzure.StorageClient.CloudBlockBlob>().Where(b => b.Name.ToLower().EndsWith(".pdf"))
    //        var listOfBlobItems = blobContainer.ListBlobs().OfType<CloudBlockBlob>().ToList();

    //        lstBlobs.AddRange(listOfBlobItems.Where(z => db.FormatoDocumento.Select(y => y.nombreDocumento).Contains(z.Name)).Select(x => new FormatoDocumento()
    //        {
    //            nombreDocumento = x.Name,
    //            fechaSubida = Convert.ToDateTime(x.Properties.LastModified.ToString()),
    //            tipoArchivo = x.Name.Split('.')[0],
    //            idFormatoDocumento = db.FormatoDocumento.Where(z => z.nombreDocumento == x.Name).Select(c => c.idFormatoDocumento).First()

    //        }));

    //        return lstBlobs;
    //    }

    //    // GET: FormatoDocumento/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FormatoDocumento formatoDocumento = db.FormatoDocumento.Find(id);
    //        if (formatoDocumento == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(formatoDocumento);
    //    }

    //    // GET: FormatoDocumento/Create
    //    public ActionResult Create()
    //    {
    //        //lista de los tipos que estan vigentes
    //        var listItemTipoDocumento = db.TipoFormatoDocumento.Where(x => x.vigencia == 1 && x.utilizado == 0).Select(y => y.tipo).ToList();
    //        ViewModelFormatoDocumento vm = new ViewModelFormatoDocumento();
    //        vm.tiposArchivos = listItemTipoDocumento;
    //        return View(vm);
    //    }

    //    // POST: FormatoDocumento/Create
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create( FormatoDocumento formatoDocumento, List<HttpPostedFileBase> postedFiles)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            string nombreArchivo = formatoDocumento.tipoArchivo +'.' + postedFiles[0].FileName.Split('.')[1];
    //            formatoDocumento.nombreDocumento = nombreArchivo;
    //            this.UploadFilesToAzureStorage(postedFiles, nombreArchivo);
    //            formatoDocumento.fechaSubida = DateTime.Now;
    //            db.FormatoDocumento.Add(formatoDocumento);
    //            db.SaveChanges();
    //            var tipo = db.TipoFormatoDocumento.Where(x => x.tipo == formatoDocumento.tipoArchivo).First();
    //            tipo.utilizado = 1;
    //            db.Entry(tipo).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(formatoDocumento);
    //    }

    //    private void UploadFilesToAzureStorage(IEnumerable<HttpPostedFileBase> files, string nombre)
    //    {
    //        var connectionString = ConfigurationManager.AppSettings["ConnectionStringBlob"].ToString();
    //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

    //        CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
    //        CloudBlobContainer storageContainer = BlobClient.GetContainerReference("test");

    //        foreach (var file in files)
    //        {
    //            if (file?.ContentLength > 0)
    //            {
    //                string fileName = Path.GetFileName(nombre);

    //                // Azure Storage
    //                CloudBlockBlob blockBlob = storageContainer.GetBlockBlobReference(fileName);
    //                blockBlob.UploadFromStream(file.InputStream);
    //            }
    //        }
    //    }

    //    // GET: FormatoDocumento/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FormatoDocumento formatoDocumento = db.FormatoDocumento.Find(id);
    //        if (formatoDocumento == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(formatoDocumento);
    //    }

    //    // POST: FormatoDocumento/Edit/5
    //    // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
    //    // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "idFormatoDocumento,nombreDocumento,urlDocumento,tipoController,idController")] FormatoDocumento formatoDocumento)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(formatoDocumento).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(formatoDocumento);
    //    }

    //    // GET: FormatoDocumento/Delete/5
    //    public ActionResult Delete(string id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FormatoDocumento formatoDocumento = db.FormatoDocumento.Where(x => x.nombreDocumento == id).First();
    //        if (formatoDocumento == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(formatoDocumento);
    //    }

    //    // POST: FormatoDocumento/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        FormatoDocumento formatoDocumento = db.FormatoDocumento.Find(id);

    //        TipoFormatoDocumento tipo = db.TipoFormatoDocumento.Where(x => x.tipo == formatoDocumento.tipoArchivo).First();
    //        tipo.utilizado = 0;
    //        db.Entry(tipo).State = EntityState.Modified;
    //        db.SaveChanges();
    //        db.FormatoDocumento.Remove(formatoDocumento);
    //        db.SaveChanges();

    //        return RedirectToAction("Index");
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (disposing)
    //        {
    //            db.Dispose();
    //        }
    //        base.Dispose(disposing);
    //    }
    //}
}
