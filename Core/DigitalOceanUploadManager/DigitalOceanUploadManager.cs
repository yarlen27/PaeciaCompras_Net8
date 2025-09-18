using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using MimeMapping;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.S3.Model;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DigitalOceanUploader.Shared
{
    /// <summary>
    /// Digital ocean upload manager.
    /// </summary>
    public class DigitalOceanUploadManager : IDisposable
    {
        KeyManager _keyManager;
        private ILogger _logger;
        string _serviceUrl;
        string _spaceName;
        string _region;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DigitalOceanUploader.Shared.DigitalOceanUploadManager"/> class.
        /// </summary>
        /// <param name="accessKey">Access key.</param>
        /// <param name="secretKey">Secret key.</param>
        /// <param name="spaceName">Space name.</param>
        /// <param name="serviceUrl">Service URL.</param>
        public DigitalOceanUploadManager(IConfiguration configuration, ILogger logger)
        {

            var accessKey = configuration.GetSection("Spaces")["accessKey"];
            var secretKey = configuration.GetSection("Spaces")["secretKey"];
            _serviceUrl = configuration.GetSection("Spaces")["serviceUrl"];
            _spaceName = configuration.GetSection("Spaces")["spaceName"];
            _region = configuration.GetSection("Spaces")["regionName"];
            _keyManager = new KeyManager(accessKey, secretKey);
            this._logger = logger;
        }






        /// <summary>
        /// Cleans up previous attempts.
        /// </summary>
        /// <returns>The up previous attempts.</returns>
        public async Task CleanUpPreviousAttempts()
        {
            using (var client = CreateNewClient())
            {
                var currentMultiParts = await client.ListMultipartUploadsAsync(_spaceName);
                foreach (var multiPart in currentMultiParts.MultipartUploads)
                {
                    await client.AbortMultipartUploadAsync(currentMultiParts.BucketName, multiPart.Key, multiPart.UploadId);
                }
            }
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <returns>The upload id.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="uploadName">Upload name.</param>
        /// <param name="maxPartRetry">Max part retry.</param>
        public async Task<Guid> UploadFile(Stream fileStream, string uploadName, int maxPartRetry = 3, long maxPartSize = 6000000L)
        {


            uploadName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(uploadName));
            uploadName = uploadName.Replace(" ", string.Empty);


            if (string.IsNullOrWhiteSpace(uploadName))
                throw new ArgumentNullException(nameof(uploadName));
            if (maxPartRetry < 1)
                throw new ArgumentException("Max Part Retry needs to be greater than or equal to 1", nameof(maxPartRetry));
            if (maxPartSize < 1)
                throw new ArgumentException("Max Part Size needs to be greater than 0", nameof(maxPartSize));

            //var fileInfo = new FileInfo(filePath);
            var contentType = MimeUtility.GetMimeMapping(uploadName) ?? "application/octet-stream";
            Amazon.S3.Model.InitiateMultipartUploadResponse multiPartStart;
            var fileId = Guid.NewGuid();
            var key = fileId.ToString();

            using (var client = CreateNewClient())
            {

                try
                {
             



                    var uploadObj = new Amazon.S3.Model.InitiateMultipartUploadRequest()
                    {
                        BucketName = _spaceName,
                        ContentType = contentType,
                        Key = key,

                    };
                    uploadObj.Metadata.Add("fileName", uploadName);


                    multiPartStart = await client.InitiateMultipartUploadAsync(uploadObj);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "UploadFile");
                    throw;
                }


                var estimatedParts = (int)(fileStream.Length / maxPartSize);
                if (estimatedParts == 0)
                    estimatedParts = 1;

                UploadStatusEvent?.Invoke(this, new UploadStatus(0, estimatedParts, 0, fileStream.Length));

                var tempFile = Path.GetTempFileName();


                using (var stream = new FileStream(tempFile, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }



                try
                {
                    var i = 0L;
                    var n = 1;
                    Dictionary<string, int> parts = new Dictionary<string, int>();
                    while (i < fileStream.Length)
                    {
                        long partSize = maxPartSize;
                        var lastPart = (i + partSize) >= fileStream.Length;
                        if (lastPart)
                            partSize = fileStream.Length - i;
                        bool complete = false;
                        int retry = 0;
                        Amazon.S3.Model.UploadPartResponse partResp = null;
                        do
                        {
                            retry++;
                            try
                            {
                                partResp = await client.UploadPartAsync(new Amazon.S3.Model.UploadPartRequest()
                                {
                                    BucketName = _spaceName,
                                    FilePath = tempFile,

                                    FilePosition = i,
                                    IsLastPart = lastPart,
                                    PartSize = partSize,
                                    PartNumber = n,
                                    UploadId = multiPartStart.UploadId,
                                    Key = key
                                });
                                complete = true;
                            }
                            catch (Exception ex)
                            {
                                UploadExceptionEvent?.Invoke(this, new UploadException($"Failed to upload part {n} on try #{retry}", ex));
                            }
                        } while (!complete && retry <= maxPartRetry);

                        if (!complete || partResp == null)
                            throw new Exception($"Unable to upload part {n}");

                        parts.Add(partResp.ETag, n);
                        i += partSize;
                        UploadStatusEvent?.Invoke(this, new UploadStatus(n, estimatedParts, i, fileStream.Length));
                        n++;
                    }

                    // upload complete
                    var completePart = await client.CompleteMultipartUploadAsync(new Amazon.S3.Model.CompleteMultipartUploadRequest()
                    {
                        UploadId = multiPartStart.UploadId,
                        BucketName = _spaceName,
                        Key = key,
                        PartETags = parts.Select(p => new Amazon.S3.Model.PartETag(p.Value, p.Key)).ToList()
                    });
                }
                catch (Exception ex)
                {

                    this._logger.LogError(ex, "UploadFile 2");
                     
                    var abortPart = await client.AbortMultipartUploadAsync(_spaceName, key, multiPartStart.UploadId);
                    UploadExceptionEvent?.Invoke(this, new UploadException("Something went wrong upload file and it was aborted", ex));
                }
            }

            return fileId;//multiPartStart?.UploadId;
        }

        /// <summary>
        /// Delete the file.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="uploadName">Upload name.</param>
        public async Task<bool> DeleteFile(string uploadName)
        {
            if (string.IsNullOrWhiteSpace(uploadName))
                throw new ArgumentNullException(nameof(uploadName));

            using (var client = CreateNewClient())
            {
                var objectResponse = await client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest()
                {
                    BucketName = _spaceName,
                    Key = uploadName
                });
                return true;
            }
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="uploadName">Upload name.</param>
        public async Task<FileDonwnload> DownloadFile(string uploadName)
        {
            if (string.IsNullOrWhiteSpace(uploadName))
                throw new ArgumentNullException(nameof(uploadName));

            using (var client = CreateNewClient())
            {
                var objectResponse = await client.GetObjectAsync(new Amazon.S3.Model.GetObjectRequest()
                {
                    BucketName = _spaceName,
                    Key = uploadName
                });


                var result = new FileDonwnload();

                result.filename = objectResponse.Metadata["filename"];




                using (var ms = new MemoryStream())
                {
                    objectResponse.ResponseStream.CopyTo(ms);
                    ms.Position = 0; // Reset stream position to beginning

                    result.file =   ms.ToArray();
                }

                return result;
            }
        }

        public class FileDonwnload {

            public string filename { get; set; }
            public byte[] file { get; set; }

        }

        /// <summary>
        /// Occurs when upload exception event.
        /// </summary>
        public event EventHandler<UploadException> UploadExceptionEvent = delegate { };

        /// <summary>
        /// Occurs when upload status event.
        /// </summary>
        public event EventHandler<UploadStatus> UploadStatusEvent = delegate { };

        private AmazonS3Client CreateNewClient()
        {
            //var amazonClient = new AmazonS3Client()

            var result = new AmazonS3Client(KeyManager.SecureStringToString(_keyManager.AccessKey), KeyManager.SecureStringToString(_keyManager.SecretKey),
                new AmazonS3Config()
                {
                    ServiceURL = _serviceUrl,
                    // RegionEndpoint = RegionEndpoint.GetBySystemName(_region)
                    // SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA1,
                    // SignatureVersion = 
                });

            return result;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _keyManager?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DigitalOceanUploadManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
