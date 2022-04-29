using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace SGC.Services
{
    public class AzureBlob : IAzureBlob
    {
        public AzureBlob()
        {

        }
        public class MyFileClass
        {
            private Stream blobStream;

            public MyFileClass(Stream blobStream, string contentType, string name)
            {
                this.blobStream = blobStream;
                this.contentType = contentType;
                this.name = name;
            }

            public Stream stream { get; set; }
            public string contentType { get; set; }
            public string name { get; set; }
        }


        public MyFileClass GetFile(string identificador)
        {
            MemoryStream ms = new MemoryStream();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storageinsecap;AccountKey=BKxl/Mf5BVdR//yF1Ui9An5pFM4bDuRHue5iypm9nJ8ucF2OsjjZBFozXuUAbseyZCxoKkMTjFqT5ymILPaLrA==;EndpointSuffix=core.windows.net");

            CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer c1 = BlobClient.GetContainerReference("test");

            if (c1.Exists())
            {
                CloudBlob file = c1.GetBlobReference(identificador);

                if (file.Exists())
                {
                    file.DownloadToStreamAsync(ms);
                    System.IO.Stream blobStream = file.OpenReadAsync().Result;
                    MyFileClass myFile = new MyFileClass(blobStream, file.Properties.ContentType, file.Name);
                    return myFile;

                }
                else
                {
                    //return Content("File does not exist");
                    return null;
                }
            }
            else
            {
                //return Content("Container does not exist");
                return null;
            }
        }
    }
}