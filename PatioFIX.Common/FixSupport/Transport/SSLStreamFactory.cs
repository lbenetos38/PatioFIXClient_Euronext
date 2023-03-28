using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace PatioFIX.Common.FixSupport.Transport
{
    /// <summary>
    /// 
    /// </summary>
    public class SSLStreamFactory
    {
        static Logger theLogger = new Logger("SSLFactory");
        readonly FixConfiguration theSettings = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public SSLStreamFactory(FixConfiguration settings)
        {
            this.theSettings = settings;
        }


        /// <summary>
        /// Δημιουργεί ενα SSL tunnel και αυθεντικοποιεί τον remote Server
        /// Exception handling will be done by the caller (by design...)
        /// </summary>
        /// <param name="innerStream"></param>
        /// <returns></returns>
        public SslStream CreateClientStreamAndAuthenticate(Stream innerStream)
        {
            var sslStream = new SslStream(innerStream, false, ValidateServerCertificate, null);

            theLogger.Info("Try to create an SSL tunnel...");

            // Setup secure SSL Communication
            var options = new SslClientAuthenticationOptions
            {
                TargetHost = theSettings.SSLServerName,
                ClientCertificates = null,
                EnabledSslProtocols = SslProtocols.None,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            };

            sslStream.AuthenticateAsClient(options);

            return sslStream;
        }



        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns></returns>
        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            /*
             * we log some info about the certificate....
             */
            if (certificate != null)
            {
                theLogger.Info($"VerifyCertificate:: Subject( {certificate.Subject})");
                theLogger.Info($"VerifyCertificate:: Issuer ({certificate.Issuer})");
                theLogger.Info($"VerifyCertificate:: Valid (From={certificate.GetEffectiveDateString()}, To={certificate.GetExpirationDateString()})");
            }
            /*
             * if we don't have any sslPolicyErrors we have benn validated succesfully
             */
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                theLogger.Info("VerifyCertificate:: VALID CERTIFICATE");
                return true;
            }

            /*
             * we log the sslPolicyErrors and the ChainStatus
             */
            theLogger.Info($"VerifyCertificate:: NOT A VALID certificate ( sslPolicyErrors = {sslPolicyErrors})!");
            if (chain != null)
            {
                foreach (var item in chain.ChainStatus)
                {
                    theLogger.Info($"VerifyCertificate:: ChainStatus = {item.Status} - StatusInformation = {item.StatusInformation}");
                }
            }

            // Accept without looking at if the certificate is valid if validation is disabled
            if (theSettings.VerifyCertificate == false)
            {
                theLogger.Warning("VerifyCertificate:: certificate IS ACCEPTED WITHOUT VERIFICATION!");
                return true;
            }

            throw new NotSupportedException("Server Cedrtificate Verification is not Supported yet!");
        }


    }
}
