using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Refit;

namespace Refit.Extention.Attributes
{
    public class ActionNameAttribute : HeadersAttribute
    {
        public ActionNameAttribute(string header) : base(string.Format("CulstActionName:{0}", header))
        {
        }
    }
    
}
