namespace ZNxt.Net.Core.Services
{
    [System.Obsolete]
    public class EventSubscriberCompletedBaseService 
    {
        //protected object APIRsponse { get; set; }

        //public EventSubscriberCompletedBaseService(ParamContainer paramContainer)
        //    : base(paramContainer)
        //{
        //    APIRsponse = paramContainer.GetKey(CommonConst.CommonValue.PARAM_API_RESPONSE);
        //}

        //protected bool IsSuccessResponse()
        //{
        //    if (HttpProxy.ResponseStatusCode == CommonConst._200_OK)
        //    {
        //        if (APIRsponse is JObject)
        //        {
        //            var response = (APIRsponse as JObject);
        //            if (response[CommonConst.CommonField.HTTP_RESPONE_CODE].ToString() == CommonConst._1_SUCCESS.ToString())
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}