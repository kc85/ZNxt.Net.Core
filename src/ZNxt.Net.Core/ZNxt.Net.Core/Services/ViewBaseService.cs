using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Services
{
    public abstract class ViewBaseService : ApiBaseService
    {
        protected IStaticContentHandler ContentHandler;

        public ViewBaseService(ParamContainer paramContainer)
            : base(paramContainer)
        {
            ContentHandler = paramContainer.GetKey(CommonConst.CommonValue.PARAM_CONTENT_HANDLER);
        }
    }
}