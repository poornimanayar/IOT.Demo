using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;

namespace IOT.Demo.Umbraco.RequestHandlers
{
    public class AlexaRequestValidationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            if (WebConfigurationManager.AppSettings["mode"] != "debug" && requestMessage.RequestUri.AbsolutePath.ToLower().Contains("umbraco/api/alexaapi/friendlyevents"))
            {
                //Requests that Alexa sends to your web service include two HTTP headers that you must use to 
                // check the request signature:SignatureCertChainUrl, Signature
                if (!requestMessage.Headers.Contains("Signature") || !requestMessage.Headers.Contains("SignatureCertChainUrl"))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                if (string.IsNullOrEmpty(requestMessage.Headers.GetValues("Signature").First()) || string.IsNullOrEmpty(requestMessage.Headers.GetValues("SignatureCertChainUrl").First()))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                // check and validate the request signature:

                //Normalize the URL in the SignatureCertChainUrl header by removing dot segments and duplicate slashes and fragment.
                var signatureCertChainUrl = requestMessage.Headers.GetValues("SignatureCertChainUrl").First().Replace("/../", "/");
                if (string.IsNullOrWhiteSpace(signatureCertChainUrl))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                Uri certUrl = new Uri(signatureCertChainUrl);

                //Make sure that the URL meets all of the following criteria:
                // The protocol is https(not case sensitive).
                //The hostname is s3.amazonaws.com(not case sensitive).
                //The path begins with / echo.api / (case sensitive).
                //If a port is specified in the URL, the port is 443.
                if (!(certUrl.Port == 443
                    && certUrl.Host.Equals("s3.amazonaws.com", StringComparison.OrdinalIgnoreCase)
                    && certUrl.AbsolutePath.StartsWith("/echo.api/")
                    && certUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                //Download the PEM-encoded X.509 certificate chain using the URL specified in the SignatureCertChainUrl header in the request. 
                //This chain is provided at runtime so that it can be updated periodically
                using (var webClient = new WebClient())
                {
                    var cert = webClient.DownloadData(certUrl);
                    var certificate = new X509Certificate2(cert);

                    var effectiveDate = DateTime.MinValue;
                    var expiryDate = DateTime.MinValue;
                    if (!((DateTime.TryParse(certificate.GetEffectiveDateString(), out effectiveDate) && effectiveDate < DateTime.UtcNow)
                        && (DateTime.TryParse(certificate.GetExpirationDateString(), out expiryDate) && expiryDate > DateTime.UtcNow)))
                    {
                        throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                    }

                    //Make sure the domain name echo-api.amazon.com is present in the subject section of the signing certificate.
                    if (!certificate.Subject.Contains("echo-api.amazon.com"))
                    {
                        throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                    }

                    //Base64 - decode the value of the Signature header in the request to obtain the encrypted signature.
                    //Use the public key that you extracted from the signing certificate to decrypt the encrypted signature to produce the asserted hash value.
                    //Generate a SHA-1 hash value from the full HTTP request body to produce the derived hash value.
                    //Compare the asserted hash value and derived hash values to ensure that they match.If they do not match, discard the request.
                    var signatureString = requestMessage.Headers.GetValues("Signature").First();
                    byte[] signatureByes = Convert.FromBase64String(signatureString);

                    using (var sha1 = new SHA1Managed())
                    {
                        var body = await requestMessage.Content.ReadAsStringAsync();

                        var data = sha1.ComputeHash(Encoding.UTF8.GetBytes(body));
                        var rsa = (RSACryptoServiceProvider)certificate.PublicKey.Key;
                        if (rsa == null || !rsa.VerifyHash(data, CryptoConfig.MapNameToOID("SHA1"), signatureByes))
                        {
                            throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                        }
                    }

                }
            }
            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}