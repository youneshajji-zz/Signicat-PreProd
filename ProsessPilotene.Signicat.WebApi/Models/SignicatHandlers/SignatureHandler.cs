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
                                               Value = "nbid-mobil"
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

            if (signingInfoSigningMetodText == "handwritten")
            {
                var signature = new[]
                {
                    new signature
                    {
                        responsive = true,
                                        method = new method[]
                                        {
                                            new method
                                            {
                                               handwritten = true
                                            }
                                        }
                    }
                };
                return signature;
            }

            return null;
        }
    }
}