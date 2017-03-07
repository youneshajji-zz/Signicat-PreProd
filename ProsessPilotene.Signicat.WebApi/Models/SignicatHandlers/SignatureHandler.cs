using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PP.Signicat.WebApi.SignicatPreProd;
using static PP.Signicat.WebApi.Models.Types;

namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    internal class SignatureHandler
    {
        internal authenticationbasedsignature[] GetAuthSignatures(SigningInfo signingInfo)
        {
            if (signingInfo.signingMetodText != "nbid" && signingInfo.signingMetodText != "ink" &&
                signingInfo.signingMetodText != "nemid")
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

            //if (signingInfo.signingMetodText == "nbid")
            //{
            //    var authsignature = new[]
            //    {
            //        new authenticationbasedsignature
            //        {
            //                            method = new method[]
            //                            {
            //                                new method
            //                                {
            //                                    handwritten = signingInfo.isInk,
            //                                    handwrittenSpecified = signingInfo.isInk,
            //                                   Value = "nbid"
            //                                },
            //                                new method
            //                                {
            //                                    handwritten = signingInfo.isInk,
            //                                    handwrittenSpecified = signingInfo.isInk,
            //                                   Value = "nbid-mobil"
            //                                }
            //                            }
            //        }
            //    };
            //    return authsignature;
            //}

            if (signingInfo.signingMetodText == "ink")
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
                        responsive = true,responsiveSpecified = true,
                                        method = new []
                                        {
                                            new method
                                            {
                                                 handwritten = signingInfo.isInk,
                                                handwrittenSpecified = signingInfo.isInk,
                                               Value = "nbid-sign"
                                            },
                                            new method
                                            {
                                                 handwritten = signingInfo.isInk,
                                                handwrittenSpecified = signingInfo.isInk,
                                               Value = "nbid-mobil-sign"
                                            }
                                        }
                    }
                };
                return signature;
            }

            if (signingInfo.signingMetodText == "nemid")
            {
                var signature = new[]
                {
                    new signature
                    {
                        responsive = true,responsiveSpecified = true,
                                        method = new []
                                        {
                                            new method
                                            {
                                                 handwritten = signingInfo.isInk,
                                                handwrittenSpecified = signingInfo.isInk,
                                               Value = "nemid-sign"
                                            }
                                        }
                    }
                };
                return signature;
            }

            return null;
        }

        public string GetMethod(int signingMetod)
        {
            if (signingMetod == (int)SignMethod.Ink)
                return "ink";

            if (signingMetod == (int)SignMethod.BankId)
                return "nbid";

            if (signingMetod == (int)SignMethod.Tupas)
                return "tupas";

            if (signingMetod == (int)SignMethod.NemId)
                return "nemid";

            if (signingMetod == (int)SignMethod.Otp)
                return "scid-otp";

            if (signingMetod == (int)SignMethod.Social)
                return "social";



            return "nbid";
        }

        public void AddAuthMethod(SigningInfo signingInfo, createrequestrequest request)
        {
            if (signingInfo.authMetod == (int)AuthMethod.BankId)
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "nbid", "nbid-mobil" }
                    };
                }
            }

            if (signingInfo.authMetod == (int)AuthMethod.Tupas)
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "tupas", "tupas-mobil" }
                    };
                }
            }

            if (signingInfo.authMetod == (int)AuthMethod.NemId)
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "nemid" }
                    };
                }
            }

            if (signingInfo.authMetod == (int)AuthMethod.Otp)
            {
                for (int i = 0; i < request.request[0].task.Length; i++)
                {
                    request.request[0].task[i].authentication = new authentication
                    {
                        method = new string[] { "scid-otp" }
                    };
                }
            }

            if (signingInfo.authMetod == (int)AuthMethod.Social)
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