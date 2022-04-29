namespace SGC.Controllers
{
    //public class UploadController : Controller
    //{
    //    public ActionResult Index()
    //    {
    //        return View();
    //    }

    //    [HttpPost]
    //    public ActionResult UploadFiles(List<HttpPostedFileBase> postedFiles)
    //    {
    //        this.UploadFilesToAzureStorage(postedFiles);
    //        return RedirectToAction("Index", "FileAzures");   
    //            }

    //    private void UploadFilesToAzureStorage(IEnumerable<HttpPostedFileBase> files)
    //    {
    //        var connectionString = ConfigurationManager.AppSettings["ConnectionStringBlob"].ToString();
    //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

    //        CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
    //        CloudBlobContainer storageContainer = BlobClient.GetContainerReference("test");

    //        foreach (var file in files)
    //        {
    //            if (file?.ContentLength > 0)
    //            {
    //                string fileName = Path.GetFileName(file.FileName);

    //                // Azure Storage
    //                CloudBlockBlob blockBlob = storageContainer.GetBlockBlobReference(fileName);
    //                blockBlob.UploadFromStream(file.InputStream);
    //            }
    //        }
    //    }
    //}
}