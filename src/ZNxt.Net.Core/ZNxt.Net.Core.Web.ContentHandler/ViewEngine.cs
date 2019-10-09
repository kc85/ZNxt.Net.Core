﻿
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using RazorLight;

namespace ZNxt.Net.Core.Web.ContentHandler
{
    public class RazorTemplateEngine : IViewEngine
    {
        private static readonly object _lock = new object();
        private static RazorTemplateEngine _viewEngine;

        public RazorTemplateEngine()
        {

        }

        public static RazorTemplateEngine GetEngine()
        {
            if (_viewEngine == null)
            {
                lock (_lock)
                {
                    _viewEngine = new RazorTemplateEngine();

                    return _viewEngine;
                }
            }
            else
            {
                return _viewEngine;
            }
        }

        public string Compile(string inputTemplete, string key, Dictionary<string, dynamic> dataModel)
        {
            if (dataModel == null)
            {
                dataModel = new Dictionary<string, dynamic>();
            }
            if (dataModel is Dictionary<string, dynamic>)
            {
                StringBuilder headerAppender = new StringBuilder();
                headerAppender.AppendLine("@{");
                foreach (var item in (dataModel as Dictionary<string, dynamic>))
                {
                    if (item.Key == CommonConst.CommonValue.METHODS)
                    {
                        foreach (var itemMethod in (item.Value as Dictionary<string, dynamic>))
                        {
                            headerAppender.AppendLine(string.Format("dynamic {0} = @Model[\"{1}\"][\"{0}\"];", itemMethod.Key, CommonConst.CommonValue.METHODS));
                        }
                    }
                }
                inputTemplete = headerAppender.AppendLine("}").AppendLine(inputTemplete).ToString();
            }

            var engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(this.GetType())
            .UseMemoryCachingProvider()
            .Build();

            return engine.CompileRenderStringAsync<Dictionary<string, dynamic>>(key, inputTemplete, dataModel).GetAwaiter().GetResult();
        }
    }
}
