using System;
using System.Net.Http;
using System.Net.Security;

namespace ChatAppWithSignalR.Client.Helpers
{
    public class DevHttpsConnectionHelper
    {
        public DevHttpsConnectionHelper(int sslPort)
        {
            SslPort = sslPort;
            DevServerRootUrl = FormattableString.Invariant($"https://{DevServerName}:{SslPort}");
            // Updated to ensure that the HttpClient is constructed with a valid handler.
            LazyHttpClient = new Lazy<HttpClient>(() => new HttpClient(GetPlatformMessageHandler()));
        }

        public int SslPort { get; }

        public string DevServerName =>
#if WINDOWS
            "localhost";
#elif ANDROID
            "0.0.0.0";
#else
            throw new PlatformNotSupportedException("Only Windows and Android currently supported.");
#endif

        public string DevServerRootUrl { get; }

        private Lazy<HttpClient> LazyHttpClient;
        public HttpClient HttpClient => LazyHttpClient.Value;

        public HttpMessageHandler? GetPlatformMessageHandler()
        {
#if WINDOWS
            // Return a default handler instead of null.
            return new HttpClientHandler();
#elif ANDROID
            var handler = new CustomAndroidMessageHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert != null && cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == SslPolicyErrors.None;
            };
            return handler;
#else
            throw new PlatformNotSupportedException("Only Windows and Android currently supported.");
#endif
        }

#if ANDROID
        internal sealed class CustomAndroidMessageHandler : Xamarin.Android.Net.AndroidMessageHandler
        {
            protected override Javax.Net.Ssl.IHostnameVerifier GetSSLHostnameVerifier(Javax.Net.Ssl.HttpsURLConnection connection)
                => new CustomHostnameVerifier();

            private sealed class CustomHostnameVerifier : Java.Lang.Object, Javax.Net.Ssl.IHostnameVerifier
            {
                public bool Verify(string? hostname, Javax.Net.Ssl.ISSLSession? session)
                {
                    // Check the default verifier first.
                    return Javax.Net.Ssl.HttpsURLConnection.DefaultHostnameVerifier.Verify(hostname, session)
                        || ((hostname == "0.0.0.0") && session.PeerPrincipal?.Name == "CN=localhost");
                }
            }
        }
#endif
    }
}