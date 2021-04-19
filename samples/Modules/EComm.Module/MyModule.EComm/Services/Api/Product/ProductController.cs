using MyModule.EComm.Consts;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace MyModule.EComm.Services.Api.Product
{
    public class ProductController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly ILogger _logger;

        public ProductController(IHttpContextProxy httpContextProxy,
            IDBService dBService,
            IRDBService rDBService,
            ILogger logger, IResponseBuilder responseBuilder)
      : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _logger = logger;
            _responseBuilder = responseBuilder;
        }

        [Route(ECommConsts.SERVICE_API_PREFIX + "/product", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject Ping()
        {
            var product = new JObject()
            {
                ["id"] = 1,
                ["name"] = "Product 1",
                ["description"] = "Product 1 description",
                ["sku"] = "skup01" ,
            };
            return _responseBuilder.Success(product);
        }
    }
}
