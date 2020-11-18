using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Admin.Services.Api
{
    public class GalleryController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly String collection = "gallery";

        private readonly IResponseBuilder _responseBuilder;
        private readonly ILogger _logger;
        private readonly IDBService _dBService;
        IHttpFileUploader _httpFileUploader;
        public GalleryController(IHttpContextProxy httpContextProxy, IDBService dBService, ILogger logger, IResponseBuilder responseBuilder, IHttpFileUploader httpFileUploader) : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _httpContextProxy = httpContextProxy;
            _responseBuilder = responseBuilder;
            _logger = logger;
            _dBService = dBService;
            _httpFileUploader = httpFileUploader;
            var gallerydb = CommonUtility.GetAppConfigValue("gallery_db");
            if (string.IsNullOrEmpty(gallerydb))
            {
                gallerydb = "ZNxt_EM-QA-ECS-UI";
            }
            _dBService.Init(gallerydb);

        }

        [Route("/gallery/image/upload", CommonConst.ActionMethods.POST, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject UploadImage()
        {
            try
            {
                //_httpContextProxy.GetQueryString("galleryname");
                var files = _httpFileUploader.GetFiles();
                if (files.Any())
                {
                    _logger.Debug($"getting file {files.First() }");
                    var filePrefix = CommonUtility.RandomString(5).ToUpper();
                    var request = new JObject();
                    request[CommonConst.CommonField.DISPLAY_ID] = request[CommonConst.CommonField.KEY] = CommonUtility.GetNewID();
                    request["images"] = new JArray();
                    request["file_name"] = files.First();
                    request["is_override"] = false;

                    var fileData = _httpFileUploader.GetFileData(files.First());
                    request["file_size"] = fileData.Length;

                    _logger.Debug($"getting thumbnails");
                    var rawImage = new MemoryStream(fileData);
                    var smallImage = new MemoryStream(Convert.FromBase64String(ImageUtility.GetCropedImage(fileData, 50, 50)));
                    var mediumImage = new MemoryStream(Convert.FromBase64String(ImageUtility.GetCropedImage(fileData, 120, 120)));
                    var largeImage = new MemoryStream(Convert.FromBase64String(ImageUtility.GetCropedImage(fileData, 200, 200)));

                    _logger.Debug($"UploadingImageToS3 Raw");
                    var pathRaw = UploadImageToS3(rawImage, filePrefix);

                    (request["images"] as JArray).Add(new JObject()
                    {
                        ["image"] = pathRaw,
                        ["type"] = "raw",
                        ["meta_data"] = new JObject()
                        {
                            ["size"] = pathRaw.Length
                        }
                    });

                    _logger.Debug($"UploadingImageToS3 Small");
                    var pathsmall = UploadImageToS3(smallImage, filePrefix);
                    (request["images"] as JArray).Add(new JObject()
                    {
                        ["image"] = pathsmall,
                        ["type"] = "thumbnail_s",
                        ["meta_data"] = new JObject()
                        {
                            ["size"] = pathsmall.Length
                        }
                    });
                    _logger.Debug($"UploadingImageToS3 M");
                    var pathM = UploadImageToS3(mediumImage, filePrefix);
                    (request["images"] as JArray).Add(new JObject()
                    {
                        ["image"] = pathM,
                        ["type"] = "thumbnail_m",
                        ["meta_data"] = new JObject()
                        {
                            ["size"] = pathM.Length
                        }
                    });
                    _logger.Debug($"UploadingImageToS3 L");
                    var pathLarge = UploadImageToS3(largeImage, filePrefix);
                    (request["images"] as JArray).Add(new JObject()
                    {
                        ["image"] = pathLarge,
                        ["type"] = "thumbnail_l",
                        ["meta_data"] = new JObject()
                        {
                            ["size"] = pathLarge.Length
                        }
                    });
                    _logger.Debug($"add to db", request);
                    _dBService.Write("gallery", request);

                    return _responseBuilder.Success(request);
                }
                else
                {

                    return _responseBuilder.BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }

        private string UploadImageToS3(Stream memoryStream,string filePrefix ="img")
        {
            string bucketName = "em-qa-ecomm-images";
            string keyName = $"{filePrefix}_{CommonUtility.GetNewID()}.png";
            RegionEndpoint bucketRegion = RegionEndpoint.APSouth1;
            string _accessKey = "AKIAXKQJDTEXVLEHBKH3";
            string _secretKey = "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";
            IAmazonS3 s3Client;

            try
            {
                var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
                s3Client = new AmazonS3Client(credentials, bucketRegion);


                S3CannedACL permissions = S3CannedACL.NoACL;
                var putObject = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = memoryStream,
                    ContentType = "image/png",
                    CannedACL = permissions
                };

                //setting content length for streamed input
                putObject.Metadata.Add("Content-Length", memoryStream.Length.ToString());
                var putResponse = s3Client.PutObjectAsync(putObject).GetAwaiter().GetResult();

                if (putResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    return $"/{keyName}";
                }
                else
                {
                    throw new Exception($"Error while upload image to s3 HttpStatusCode: {putResponse.HttpStatusCode }");
                }


            }
            catch (AmazonS3Exception e)
            {
                _logger.Error($"AmazonS3Exception : {e.Message}", e);
                throw;
            }
        }


        [Route("/gallery/images", CommonConst.ActionMethods.GET, "ecomm_admin," + CommonConst.CommonValue.SYS_ADMIN)]
        public JObject GetAllImages()
        {
            var cfurl = "https://d1garn5dlo0m6m.cloudfront.net";
            var data =  GetPaggedData(collection, null, "{'is_override': false}");
            if (data[CommonConst.CommonField.DATA] != null)
            {
                foreach (var item in data[CommonConst.CommonField.DATA])
                {
                    foreach (var img in item["images"])
                    {
                        img["image"] = $"{cfurl}{img["image"]}";
                    }
                    
                }
            }
            return data;
        
        }
        [Route("/gallery/image", CommonConst.ActionMethods.GET, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject GetImage()
        {
            var imageid = _httpContextProxy.GetQueryString("id");
            string filter = "{" + CommonConst.CommonField.DISPLAY_ID + ":'" + imageid + "'}";
            JArray data = _dBService.Get(collection, new RawQuery(filter));
            if (data.Count == 0)
            {
                return _responseBuilder.NotFound();
            }
            return _responseBuilder.Success(data.First() as JObject);
        }
    }

}
