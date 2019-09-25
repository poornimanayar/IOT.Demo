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
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace IOT.Demo.Umbraco.RequestHandlers
{
    public class AlexaRequestValidationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            if (WebConfigurationManager.AppSettings["mode"] != "debug" && requestMessage.RequestUri.AbsolutePath.ToLower().Contains("umbraco/api/AlexaCodeGardenApi"))
            {
                if (!requestMessage.Headers.Contains("Signature") || !requestMessage.Headers.Contains("SignatureCertChainUrl"))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                if (string.IsNullOrEmpty(requestMessage.Headers.GetValues("Signature").First()) || string.IsNullOrEmpty(requestMessage.Headers.GetValues("SignatureCertChainUrl").First()))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                var signatureCertChainUrl = requestMessage.Headers.GetValues("SignatureCertChainUrl").First().Replace("/../", "/");
                if (string.IsNullOrWhiteSpace(signatureCertChainUrl))
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                Uri certUrl = new Uri(signatureCertChainUrl);

                if (!(certUrl.Port == 443
                    && certUrl.Host.Equals("s3.amazonaws.com", StringComparison.OrdinalIgnoreCase)
                    && certUrl.AbsolutePath.StartsWith("/echo.api/")
                    && certUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
                {
                     throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                using (var webClient = new WebClient())
                {
                    var cert = webClient.DownloadData(certUrl);
                    var certificate = new X509Certificate2(cert);

                    var effectiveDate = DateTime.MinValue;
                    var expiryDate = DateTime.MinValue;
                    if (!((DateTime.TryParse(certificate.GetEffectiveDateString(), out effectiveDate) && effectiveDate < DateTime.UtcNow)
                        && (DateTime.TryParse(certificate.GetExpirationDateString(), out expiryDate) && expiryDate > DateTime.UtcNow)))
                    {
                        //  Log.Logger = new LoggerConfiguration().WriteTo.File(@"C:\inetpub\wwwroot\AlexaUmbracoNewsAPI\log.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Failed at dates}").CreateLogger();
                        throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                    }

                    if (!certificate.Subject.Contains("echo-api.amazon.com"))
                    {
                        throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                    }

                    var signatureString = requestMessage.Headers.GetValues("Signature").First();
                    byte[] signatureByes = Convert.FromBase64String(signatureString);

                    using (var sha1 = new SHA1Managed())
                    {
                        var body = await requestMessage.Content.ReadAsStringAsync();

                        var data = sha1.ComputeHash(Encoding.UTF8.GetBytes(body));
                        var rsa = (RSACryptoServiceProvider)certificate.PublicKey.Key;
                        if (rsa == null || !rsa.VerifyHash(data, CryptoConfig.MapNameToOID("SHA1"), signatureByes))
                        {
                            //   Log.Logger = new LoggerConfiguration().WriteTo.File(@"C:\inetpub\wwwroot\AlexaUmbracoNewsAPI\log.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Failed at verify hash}").CreateLogger();
                            throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                        }
                    }

                }
            }
            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}