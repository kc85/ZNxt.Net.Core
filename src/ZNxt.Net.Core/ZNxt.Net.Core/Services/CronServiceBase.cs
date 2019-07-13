using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Services
{
    public abstract class CronServiceBase : BaseService
    {
        protected IRouting Routings { get; private set; }

        public CronServiceBase(ParamContainer paramContainer)
            : base(paramContainer)
        {
            Routings = (IRouting)paramContainer.GetKey(CommonConst.CommonValue.PARAM_ROUTING_OBJECT);
        }
    }
}