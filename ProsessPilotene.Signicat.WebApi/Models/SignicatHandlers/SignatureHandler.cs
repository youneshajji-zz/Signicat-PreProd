using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    internal class SignatureHandler
    {
        internal authenticationbasedsignature[] GetAuthSignatures(string signingInfoSigningMetodText)
        {
            if (signingInfoSigningMetodText != "nbid" && signingInfoSigningMetodText != "handwritten")
            {
                var authsignature = new[]
                {
                    new authenticationbasedsignature()
                    {
                                        method = new method[]
                                        {
                                            new method
                                            {
                                                handwritten = true,
                                               Value = signingInfoSigningMetodText
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            if (signingInfoSigningMetodText == "nbid")
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
                                               Value = "nbid"
                                            },
                                            new method
                                            {
                                                handwritten = true,
                                               Value = "nbid-mobil"
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            if (signingInfoSigningMetodText == "handwritten")
            {
                var authsignature = new[]
                {
                    new authenticationbasedsignature
                    {
                        method = new method[]
                                        {
                                            new method
                                            {
                                               handwritten = false,
                                               Value = "nbid"
                                            }, new method
                                            {
                                               handwritten = true,
                                               Value = "nbid-mobil"
                                            }, new method
                                            {
                                               handwritten = true
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            return null;
        }

        internal signature[] GetSignatures(string signingInfoSigningMetodText)
        {
            if (signingInfoSigningMetodText == "nbid")
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

            if (signingMetod == "2") //SMS email OTP
                return "scid-otp";

            if (signingMetod == "3") //Social
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

            if (signingInfo.authMetod == "2") //SMS Email OTP
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "scid-otp" }
                    };
                }
            }

            if (signingInfo.authMetod == "3") //Social
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
    }
}