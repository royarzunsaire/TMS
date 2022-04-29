using Amazon.S3;
using Amazon.S3.Model;
using SGC.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SGC.Utils
{
    public class Files
    {
        private static readonly string bucketName = ConfigurationManager.AppSettings["BucketName"];
        //private static readonly RegionEndpoint bucketRegion = RegionEndpoint.SAEast1;
        private static readonly string accesskey = ConfigurationManager.AppSettings["AWSAccessKey"];
        private static readonly string secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
        private static readonly string endpointURL = ConfigurationManager.AppSettings["endpointURL"];
        private static readonly string directory = HttpContext.Current.Server.MapPath("~/Files");
        public static IAmazonS3 s3Client;

        public static async Task<Storage> RemplazarArchivoAsync(Storage archivoAntiguo, HttpPostedFileBase file, string carpetaGuardar)
        {
            if (archivoAntiguo != null)
            {
                BorrarArchivo(archivoAntiguo);
            }
            return await CrearArchivoAsync(file, carpetaGuardar);
        }

        public static async Task<Storage> RemplazarArchivoPublicoAsync(Storage archivoAntiguo, HttpPostedFileBase file, string carpetaGuardar)
        {
            if (archivoAntiguo != null)
            {
                BorrarArchivo(archivoAntiguo);
            }
            return await CrearArchivoPublicoAsync(file, carpetaGuardar);
        }

        public static async Task<Storage> CrearArchivoAsync(HttpPostedFileBase file, string carpetaGuardar)
        {
            // guardar los datos del archivo para retornarlos
            var archivo = CrearArchivoSinSubir(file, carpetaGuardar);
            return await SubirArchivoAsync(archivo, file);
        }

        public static async Task<Storage> CrearArchivoPublicoAsync(HttpPostedFileBase file, string carpetaGuardar)
        {
            // guardar los datos del archivo para retornarlos
            var archivo = CrearArchivoSinSubir(file, carpetaGuardar);
            return await SubirArchivoPublicoAsync(archivo, file);
        }

        public static Storage CrearArchivoSinSubir(HttpPostedFileBase file, string carpetaGuardar)
        {
            // guardar los datos del archivo para retornarlos
            var archivo = new Storage();
            archivo.nombreArchivo = Path.GetFileName(file.FileName);
            archivo.fechaSubido = DateTime.Now;
            archivo.tamanioArchivo = file.ContentLength;
            archivo.tipoArchivo = file.ContentType;
            archivo.key = carpetaGuardar + $@"{Guid.NewGuid()}" + Path.GetExtension(file.FileName).ToLower();
            archivo.file = file;
            archivo.urlArchivo = "https://" + bucketName + "." + endpointURL + "/" + archivo.key;
            return archivo;
        }

        public static string ArchivoValido(HttpPostedFileBase file, string[] extencionesPermitidas, int tamanioMaximoKB)
        {
            if (file.ContentLength > (tamanioMaximoKB * 1024))
            {
                return "El tamaño del archivo no puede ser mayor a " + tamanioMaximoKB + " KB.";
            }
            // validar extencion
            if (!extencionesPermitidas.Contains(Path.GetExtension(file.FileName).ToLower()))
            {
                var extencionesMensaje = "";
                foreach (var item in extencionesPermitidas)
                {
                    extencionesMensaje = extencionesMensaje + " " + item;
                }
                extencionesMensaje = extencionesMensaje + ".";
                return "El archivo seleccionado debe ser: " + extencionesMensaje;
            }
            //// mime_types por extencion
            //Dictionary<string, string> mimeTypes = new Dictionary<string, string>();
            //mimeTypes.Add(".doc", "application/msword");
            //mimeTypes.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            //mimeTypes.Add(".jpeg", "image/jpeg");
            //mimeTypes.Add(".jpg", "image/jpeg");
            //mimeTypes.Add(".odt", "application/vnd.oasis.opendocument.text");
            //mimeTypes.Add(".ods", "application/vnd.oasis.opendocument.spreadsheet");
            //mimeTypes.Add(".odp", "application/vnd.oasis.opendocument.presentation");
            //mimeTypes.Add(".png", "image/png");
            //mimeTypes.Add(".pdf", "application/pdf");
            //mimeTypes.Add(".ppt", "application/vnd.ms-powerpoint");
            //mimeTypes.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            //mimeTypes.Add(".txt", "text/plain");
            //mimeTypes.Add(".xls", "application/vnd.ms-excel");
            //mimeTypes.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            //// validar mimetype
            //foreach (var item in extencionesPermitidas)
            //{
            //    if (mimeTypes[item] == file.ContentType)
            //    {
            //        return "";
            //    }
            //}
            //return "El contenido del archivo no coincide con su extensión";
            return "";
        }

        //public static byte[] BajarArchivo(Storage archivo)
        //{
        //    // conexion
        //    var s3ClientConfig = new AmazonS3Config
        //    {
        //        ServiceURL = "https://" + endpointURL
        //    };
        //    s3Client = new AmazonS3Client(accesskey, secretkey, s3ClientConfig);

        //    var fileRequest = new GetObjectRequest
        //    {
        //        BucketName = bucketName,
        //        Key = archivo.key
        //    };

        //    var response = s3Client.GetObject(fileRequest);
        //    var memoryStream = new MemoryStream();
        //    response.ResponseStream.CopyTo(memoryStream);
        //    return memoryStream.ToArray();
        //}

        public async static Task<string> BajarArchivoADirectorioLocalAsync(Storage archivo)
        {
            if (archivo == null)
            {
                return "";
            }
            var localRoute = Path.Combine(directory, archivo.nombreArchivo);

            var response = await BajarArchivoAsync(archivo);

            using (var fileObject = response)
            {
                if (fileObject == null)
                {
                    return "";
                }
                if (fileObject.HttpStatusCode == HttpStatusCode.OK)
                {
                    fileObject.WriteResponseStreamToFile(localRoute);
                    return localRoute;
                }
                else
                {
                    return "";
                }
            }
        }

        public static async Task<byte[]> BajarArchivoBytesAsync(Storage archivo)
        {
            if (archivo == null)
            {
                return null;
            }
            var localRoute = Path.Combine(directory, archivo.nombreArchivo);

            var response = await BajarArchivoAsync(archivo);

            if (response == null)
            {
                return new byte[0];
            }

            using (Stream responseStream = response.ResponseStream)
            {
                var bytes = ReadStream(responseStream);
                return bytes;
                //var download = new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                //download.FileDownloadName = archivo.nombreArchivo;
                //return download;
            }
        }

        public static async Task<FileContentResult> BajarArchivoDescargarAsync(Storage archivo)
        {
            if (archivo == null)
            {
                return null;
            }
            var localRoute = Path.Combine(directory, archivo.nombreArchivo);

            var response = await BajarArchivoAsync(archivo);

            if (response == null)
            {
                return null;
            }

            using (Stream responseStream = response.ResponseStream)
            {
                var bytes = ReadStream(responseStream);
                var download = new FileContentResult(bytes, archivo.tipoArchivo);
                download.FileDownloadName = archivo.nombreArchivo;
                return download;
            }
        }

        public static byte[] ReadStream(Stream responseStream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static async Task<Storage> SubirArchivoAsync(Storage archivo, HttpPostedFileBase file)
        {
            // conexion
            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = "https://" + endpointURL
            };
            s3Client = new AmazonS3Client(accesskey, secretkey, s3ClientConfig);
            var filePath = Path.Combine(directory, archivo.nombreArchivo);
            FileInfo fileInfo = new FileInfo(filePath);
            try
            {
                file.SaveAs(filePath);
                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = fileInfo.OpenRead(),
                    BucketName = bucketName,
                    Key = archivo.key
                };
                PutObjectResponse response = await s3Client.PutObjectAsync(request);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return archivo;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when uploading an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when uploading an object", e.Message);
            }
            return null;
        }

        public static async Task<Storage> SubirArchivoPublicoAsync(Storage archivo, HttpPostedFileBase file)
        {
            // conexion
            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = "https://" + endpointURL
            };
            s3Client = new AmazonS3Client(accesskey, secretkey, s3ClientConfig);
            var filePath = Path.Combine(directory, archivo.nombreArchivo);
            FileInfo fileInfo = new FileInfo(filePath);
            try
            {
                file.SaveAs(filePath);
                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = fileInfo.OpenRead(),
                    BucketName = bucketName,
                    Key = archivo.key,
                    CannedACL = S3CannedACL.PublicRead
                };
                PutObjectResponse response = await s3Client.PutObjectAsync(request);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return archivo;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when uploading an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when uploading an object", e.Message);
            }
            return null;
        }

        public static async Task<GetObjectResponse> BajarArchivoAsync(Storage archivo)
        {
            // conexion
            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = "https://" + endpointURL
            };
            s3Client = new AmazonS3Client(accesskey, secretkey, s3ClientConfig);
            try
            {
                var fileRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = archivo.key
                };
                Console.WriteLine("Downloading an object");
                return await s3Client.GetObjectAsync(fileRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when downloading an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when downloading an object", e.Message);
            }
            return null;
        }

        public static async Task BorrarArchivoAsync(Storage archivo)
        {
            // conexion
            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = "https://" + endpointURL
            };
            s3Client = new AmazonS3Client(accesskey, secretkey, s3ClientConfig);
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = archivo.key
                };
                Console.WriteLine("Deleting an object");
                await s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
        }

        public static void BorrarArchivo(Storage archivo)
        {
            // conexion
            var s3ClientConfig = new AmazonS3Config
            {
                ServiceURL = "https://" + endpointURL
            };
            s3Client = new AmazonS3Client(accesskey, secretkey, s3ClientConfig);
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = archivo.key
                };
                Console.WriteLine("Deleting an object");
                s3Client.DeleteObject(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
        }

        public static void borrarArchivosLocales()
        {
            // borrar archivos locales anteriores
            foreach (FileInfo file in new DirectoryInfo(directory).GetFiles())
            {
                file.Delete();
            }
        }
    }
}