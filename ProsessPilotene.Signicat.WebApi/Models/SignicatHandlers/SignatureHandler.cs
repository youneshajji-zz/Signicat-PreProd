using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    internal class SignatureHandler
    {
        internal authenticationbasedsignature[] GetAuthSignatures(SigningInfo signingInfo)
        {
            if (signingInfo.signingMetodText != "nbid" && signingInfo.signingMetodText != "handwritten")
            {
                var authsignature = new[]
                {
                    new authenticationbasedsignature()
                    {
                                        method = new method[]
                                        {
                                            new method
                                            {
                                                handwritten = signingInfo.isInk,
                                                handwrittenSpecified = signingInfo.isInk,
                                               Value = signingInfo.signingMetodText
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            if (signingInfo.signingMetodText == "nbid")
            {
                var authsignature = new[]
                {
                    new authenticationbasedsignature
                    {
                                        method = new method[]
                                        {
                                            new method
                                            {
                                                handwritten = signingInfo.isInk,
                                                handwrittenSpecified = signingInfo.isInk,
                                               Value = "nbid"
                                            },
                                            new method
                                            {
                                                handwritten = signingInfo.isInk,
                                                handwrittenSpecified = signingInfo.isInk,
                                               Value = "nbid-mobil"
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            if (signingInfo.signingMetodText == "handwritten")
            {
                var authsignature = new[]
                {
                    new authenticationbasedsignature
                    {
                        method = new method[]
                                        {
                                            new method
                                            {
                                               handwritten = true,
                                               handwrittenSpecified = true,
                                               Value = "nbid"
                                            }, new method
                                            {
                                               handwritten = true,
                                               handwrittenSpecified = true,
                                               Value = "nbid-mobil"
                                            }, new method
                                            {
                                               handwritten = true,
                                               handwrittenSpecified = true
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            return null;
        }

        internal signature[] GetSignatures(SigningInfo signingInfo)
        {
            if (signingInfo.signingMetodText == "nbid")
            {
                var signature = new[]
                {
                    new signature
                    {
                        responsive = true,
                                        method = new []
                                        {
                                            new method
                                            {
                                               Value = "nbid-sign"
                                            },
                                            new method
                                            {
                                               Value = "nbid-mobil-sign"
                                            }
                                        }
                    }
                };
                return signature;
            }

            return null;
        }

        public string GetMethod(string signingMetod)
        {
            if (signingMetod == "1") //BankID
                return "nbid";

            if (signingMetod == "2" || signingMetod == "22") //SMS email OTP
                return "scid-otp";

            if (signingMetod == "3" || signingMetod == "33") //Social
                return "social";

            if (signingMetod == "4") //Handwritten
                return "handwritten";
            return "nbid";
        }

        public void AddAuthMethod(SigningInfo signingInfo, createrequestrequest request)
        {
            if (signingInfo.authMetod == "1") //BankID
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "nbid", "nbid-mobil" }
                    };
                }
            }

            if (signingInfo.authMetod == "2" || signingInfo.authMetod == "22") //SMS Email OTP
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "scid-otp" }
                    };
                }
            }

            if (signingInfo.authMetod == "3" || signingInfo.authMetod == "33") //Social
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "social" }
                    };
                }
            }
        }

        public bool CheckIfInk(string signingMetod)
        {
            if (signingMetod == "22" || signingMetod == "33")
                return true;
            return false;
        }
    }
}