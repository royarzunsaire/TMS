using static SGC.Services.AzureBlob;

namespace SGC.Services
{
    public interface IAzureBlob
    {
        MyFileClass GetFile(string identificador);
    }

}
