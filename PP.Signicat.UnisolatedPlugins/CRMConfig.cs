using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace PP.Signicat.UnisolatedPlugins
{
    public class CRMConfig
    {
        public EntityReference SigningUser { get; set; }
        public string SigningUsername { get; set; }
    }
}
