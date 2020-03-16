using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Refit.Extention
{
    public static class RefitExtion
    {
        public static string GetActionName(this HttpRequestMessage httpRequest)
        {

            string strRet = null;

            var headers = httpRequest.Headers;

            string name = "CulstActionName";

            IEnumerable<string> values = null;
            if (headers.TryGetValues(name, out values))
            {
                strRet = values.FirstOrDefault(); ;
                if (!false)
                {
                    headers.Remove(name);
                }
            }

            return strRet;
        }
    }
}
