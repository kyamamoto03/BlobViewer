using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlobViewer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageLoaderController : ControllerBase
    {
        private const string CONTAINER_NAME = "id-card";

        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageLoaderController> _logger;

        public ImageLoaderController(IConfiguration configuration,ILogger<ImageLoaderController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            _logger.LogInformation($"Get id:{id}");
            var filename = IDからfilename変換処理(id);

            var documentData = await GetBlobData(filename);
            if (documentData != null)
            {
                string fileName = id.ToString();

                return File(documentData, "image/jpg", fileName);
            }
            _logger.LogError($"Image Not Found id:{id}");

            return null;
        }

        /// <summary>
        /// URLで指定されたIDを内部のファイル名に変換
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string IDからfilename変換処理(int id)
        {
            return "19.jpg";
        }

        private async Task<byte[]> GetBlobData(string fileName)
        {
            byte[] retBytes = null;
            try
            {
                ///環境変数(appsetting.json)から接続先情報取得
                string con = _configuration.GetValue<string>("ConnectionStrings:Blob"); 
                var containerClient = new BlobContainerClient(con, CONTAINER_NAME);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient($"{fileName}");
                var downloadInfo = await blobClient.DownloadAsync();

                var stream = downloadInfo.Value.Content;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    retBytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retBytes;
        }
    }
}
