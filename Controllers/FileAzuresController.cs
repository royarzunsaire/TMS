namespace SGC.Controllers
{
    ////[CustomAuthorize(new int[] { 61 })]
    //public class FileAzuresController : Controller
    //{
    //    private InsecapContext db = new InsecapContext();
    //    private static List<FileAzure> lstBlobs;
    //    static readonly string[] suffixes = { "Bytes", "KB", "MB", "GB" };
    //    // GET: FileAzures
    //    public ActionResult Index()
    //    {
    //        //return View(db.FileAzure.ToList());
    //        return View(DisplayBlobFiles());
    //    }

    //    public ActionResult BorrarBlob(string nombre)
    //    {
    //        return View("Index");
    //    }

    //    private List<FileAzure> DisplayBlobFiles()
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
    //        lstBlobs = new List<FileAzure>();

    //        //string blobContainerSasUri = ConfigurationManager.AppSettings["BlobContainerSasUri"].ToString();
    //        string blobContainerSasUri = uri;
    //        Uri blobContainerUri = new Uri(blobContainerSasUri);

    //        //Uri blobContainerUri = new Uri(blobContainerSasUri);
    //        CloudBlobContainer blobContainer = new CloudBlobContainer(blobContainerUri);

    //        //var listOfBlobItems = blobContainer.ListBlobs().OfType<Microsoft.WindowsAzure.StorageClient.CloudBlockBlob>().Where(b => b.Name.ToLower().EndsWith(".pdf"))
    //        var listOfBlobItems = blobContainer.ListBlobs().OfType<CloudBlockBlob>().ToList();

    //        lstBlobs.AddRange(listOfBlobItems.Select(x => new FileAzure()
    //        {
    //            nombreArchivo = x.Name,

    //            fechaSubida = Convert.ToDateTime(x.Properties.LastModified.ToString()),
    //            tamañoArchivo = GetSizeType(x.Properties.Length)




    //        }));

    //        return lstBlobs;
    //    }

    //    private string GetSizeType(Int64 bytes)
    //    {
    //        int counter = 0;
    //        decimal number = (decimal)bytes;
    //        while (Math.Round(number / 1024) >= 1)
    //        {
    //            number = number / 1024;
    //            counter++;
    //        }
    //        return string.Format("{0:n1}{1}", number, suffixes[counter]);
    //    }

    //    public ActionResult DescargarBlob(string fileName)
    //    {
    //        var connectionString = ConfigurationManager.AppSettings["AccountKey"].ToString();
    //        CloudStorageAccount account = new CloudStorageAccount(new StorageCredentials("storageinsecap", connectionString), true);
    //        var blobClient = account.CreateCloudBlobClient();
    //        var container = blobClient.GetContainerReference("test");
    //        var blob = container.GetBlockBlobReference(fileName);
    //        var sasToken = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
    //        {
    //            Permissions = SharedAccessBlobPermissions.Read,
    //            SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(10),//assuming the blob can be downloaded in 10 miinutes
    //        }, new SharedAccessBlobHeaders()
    //        {
    //            ContentDisposition = "inline; filename=" + fileName
    //        });
    //        var blobUrl = string.Format("{0}{1}", blob.Uri, sasToken);

    //        var fileContent = new System.Net.WebClient().DownloadData(blobUrl); //byte[]

    //        return File(fileContent, "application/pdf", fileName);

    //    }
    //    // GET: FileAzures/Details/5
    //    public ActionResult Details(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FileAzure fileAzure = db.FileAzure.Find(id);
    //        if (fileAzure == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(fileAzure);
    //    }

    //    // GET: FileAzures/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: FileAzures/Create
    //    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create([Bind(Include = "idStorage,nombreArchivo,fechaSubida,tamañoArchivo,Vigencia")] FileAzure fileAzure)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.FileAzure.Add(fileAzure);
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }

    //        return View(fileAzure);
    //    }

    //    // GET: FileAzures/Edit/5
    //    public ActionResult Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FileAzure fileAzure = db.FileAzure.Find(id);
    //        if (fileAzure == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(fileAzure);
    //    }

    //    // POST: FileAzures/Edit/5
    //    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit([Bind(Include = "idStorage,nombreArchivo,fechaSubida,tamañoArchivo,Vigencia")] FileAzure fileAzure)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            db.Entry(fileAzure).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return RedirectToAction("Index");
    //        }
    //        return View(fileAzure);
    //    }

    //    // GET: FileAzures/Delete/5
    //    public ActionResult Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        }
    //        FileAzure fileAzure = db.FileAzure.Find(id);
    //        if (fileAzure == null)
    //        {
    //            return HttpNotFound();
    //        }
    //        return View(fileAzure);
    //    }

    //    // POST: FileAzures/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult DeleteConfirmed(int id)
    //    {
    //        FileAzure fileAzure = db.FileAzure.Find(id);
    //        db.FileAzure.Remove(fileAzure);
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
