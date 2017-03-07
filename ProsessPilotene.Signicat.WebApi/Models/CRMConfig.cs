using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;

namespace PP.Signicat.WebApi.Models
{
    public class CRMConfig
    {
        public string SPUser { get; set; }
        public string SPpassword { get; set; }
        public EntityReference SigningUser { get; set; }
        public string SigningUsername { get; set; }

        //SP root Folders
        public string SpAccountRoot { get; set; }
        public string SpQuoteRoot { get; set; }
        public string SpOpportunityRoot { get; set; }
        public string SpIncidentRoot { get; set; }
        public string SpOrderRoot { get; set; }
        public string SpContractRoot { get; set; }
    }
}