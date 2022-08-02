namespace Poison.Services.Interfaces
{
    public interface IPoisonFileService
    {       
            public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

            public string ConvertByteArrayToFile(byte[] fileData, string extension);

            public string GetFileIcon(string file);

            public string FormatFileSize(long bytes);
        
    }
}
