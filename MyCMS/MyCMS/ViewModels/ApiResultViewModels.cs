using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCMS.ViewModels
{
    public static class ApiHelpers
    {
        public static T CreateApiResult<T>(ApiCode apiCode) where T : ApiResultBase
        {
            var result = Activator.CreateInstance<T>();
            result.ApiCode = (int)apiCode;
            result.ApiResult = ApiResult.GetApiResult(apiCode);
            result.ApiMessage = string.Empty;
            return result;
        }
    }

    public enum ApiCode
    {
        Success = 0,
        NotFound = 1,
        BadRequest = 4,
        Unavailable = 5,
        Unknown = 9
    };

    public static class ApiResult
    {
        public const string SUCCESS = "OK";
        public const string NOT_FOUND = "NOT FOUND";
        public const string BAD_REQUEST = "BAD REQUEST";
        public const string UNAVAILABLE = "UNAVAILABLE";
        public const string UNKNOWN = "UNKNOWN";

        public static string GetApiResult(ApiCode apiCode)
        {
            switch (apiCode)
            {
                case ApiCode.Success: return SUCCESS;
                case ApiCode.NotFound: return NOT_FOUND;
                case ApiCode.BadRequest: return BAD_REQUEST;
                case ApiCode.Unavailable: return UNAVAILABLE;
                case ApiCode.Unknown:
                default:
                    return UNKNOWN;
            }
        }
    }

    public class ApiResultBase
    {
        public int ApiCode { get; set; }
        public string ApiResult { get; set; }
        public string ApiMessage { get; set; }
    }

    public class PostImportApiResult : ApiResultBase { }
}
