using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PP.Signicat.WebApi.Models
{
    public class Types
    {
        public enum SignMethod
        {
            Ink = 1,
            BankId = 2,
            Tupas = 3,
            NemId = 4,
            Otp = 5,
            Social = 6
        }

        public enum AuthMethod
        {
            BankId = 2,
            Tupas = 3,
            NemId = 4,
            Otp = 5,
            Social = 6
        }
    }
}