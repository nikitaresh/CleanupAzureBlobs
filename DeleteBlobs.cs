using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Azure.Storage.Blobs;

namespace CleanupAzureBlobs
{
    public static class CleanupBlobs
    {
        private const string connectionStringEnvVar = "AzureWebJobsStorage";
        private const string containerNameEnvVar = "CONTAINER_NAME";
        private const string blobExpirationMinutesEnvVar = "BLOB_EXPIRATION_MINUTES";

        [FunctionName("DeleteBlobs")]
        public static void Run([TimerTrigger("0 0 */1 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            const EnvironmentVariableTarget envVarTarget = EnvironmentVariableTarget.Process;
            string connectionString = System.Environment.GetEnvironmentVariable(connectionStringEnvVar, envVarTarget);
            string containerName = System.Environment.GetEnvironmentVariable(containerNameEnvVar, envVarTarget);
            string blobExpirationString = System.Environment.GetEnvironmentVariable(blobExpirationMinutesEnvVar, envVarTarget);

            double blobExpirationMinutes = 0;
            if (!Double.TryParse(blobExpirationString, out blobExpirationMinutes))
            {
                log.Info($"Failed to parse {blobExpirationMinutesEnvVar} variable with value {blobExpirationString}");
                return;
            }

            string logMessage = $"The function executed at: {DateTime.Now} to delete blobs in container"
                              + $" {containerName} with espirtion time {blobExpirationMinutes} minutes";
            log.Info(logMessage);

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = containerClient.GetBlobs();
            DateTimeOffset currentTime = DateTimeOffset.Now;
            var tasks = new List<Task<Azure.Response>>();
            int numDeletedBlobs = 0, totalBlobs = 0;
            foreach (var blob in blobs)
            {
                if ((currentTime - blob.Properties.CreatedOn).Value.TotalMinutes > blobExpirationMinutes)
                {
                    ++numDeletedBlobs;
                    tasks.Add(containerClient.DeleteBlobAsync(blob.Name));
                }
                ++totalBlobs;
            }
            Task.WaitAll(tasks.ToArray());
            log.Info($"Deleted {numDeletedBlobs} blob(s) out of {totalBlobs}.");
        }
    }
}
