using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Handlers
{
    public class HttpProxyHandler
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextProxy _httpContext;
        private readonly IServiceResolver _serviceResolver;


        public HttpProxyHandler(RequestDelegate next, IHttpContextProxy httpContext, IServiceResolver serviceResolver)
        {
            _httpContext = httpContext;
            _next = next;
            _serviceResolver = serviceResolver ;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var txnId = context.Request.Headers[CommonConst.CommonField.TRANSACTION_ID];
                if (string.IsNullOrEmpty(txnId))
                {
                    txnId = CommonUtility.GenerateTxnId();
                }
                context.Response.Headers.Add("powered-by", "znxt.app");
                context.Response.Headers.Add(CommonConst.CommonField.TRANSACTION_ID, txnId);
                context.Response.Headers.Add(CommonConst.CommonField.CREATED_DATA_DATE_TIME, CommonUtility.GetTimestampMilliseconds(DateTime.Now).ToString());
                await _next(context);
            }
            catch (Exception ex)
            {
                _serviceResolver.Resolve<ILogger>().Error(ex.Message, ex);
                await context.Response.WriteAsync(_serviceResolver.Resolve<IResponseBuilder>().ServerError(ex.Message).ToString());
            }
            
        }
    }
    public static class HttpProxyHandlerExtensions
    {
        public static IApplicationBuilder UseHttpProxyHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpProxyHandler>();
        }
    }
}
