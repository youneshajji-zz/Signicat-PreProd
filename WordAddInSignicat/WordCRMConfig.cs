using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace WordAddInSignicat
{
    public class WordCRMConfig
    {
        public string Entitylogicalnames { get; set; }
        public string SPUser { get; set; }
        public string SPpassword { get; set; }
        public string Webapiurl { get; set; }
        public bool Wordsaveinsp { get; set; }
        public string Wordusername { get; set; }
        public int Wordsigningmethod { get; set; }
        public EntityReference Worduser { get; set; }

        //number fields 
        public string AccountNrField { get; set; }
        public string OrderNrField { get; set; }
        public string QuoteNrField { get; set; }
        public string IncidentNrField { get; set; }
        public string ContractNrField { get; set; }
        public string EmailField { get; set; }

    }
}
