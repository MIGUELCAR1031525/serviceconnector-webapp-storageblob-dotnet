using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace WebStorageSample
{
    public class StorageHelper
    {
        private static BlobContainerClient GetContainerClient(string containerEndpoint, string containerName)
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);
            }

            var blobContainerUri = new Uri(new Uri(containerEndpoint), containerName);
            return new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());
        }

        static public async Task UploadBlob(string containerEndpoint, string containerName, string blobName, string blobContents)
        {
            BlobContainerClient containerClient = GetContainerClient(containerEndpoint, containerName);

            // Create the container if it does not exist.
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Upload text to a new block blob.
            byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
        }

        static public async Task<string> GetBlob(string containerEndpoint, string containerName, string blobName)
        {
            BlobContainerClient containerClient = GetContainerClient(containerEndpoint, containerName);

            // Create the container if it does not exist.
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                using (var streamReader = new StreamReader(response.Value.Content))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = await streamReader.ReadLineAsync();
                        Console.WriteLine(line);
                        return line;
                    }
                }
            }
            return "";
        }
    }
}
