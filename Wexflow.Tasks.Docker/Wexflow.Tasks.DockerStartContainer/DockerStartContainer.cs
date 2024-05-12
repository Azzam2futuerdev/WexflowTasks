using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Wexflow.Tasks.DockerStartContainer
{
    public class DockerStartContainer : Task
    {
        public string DockerHost { get; private set; }
        public int DockerPort { get; private set; }
        public string DockerCertPath { get; private set; }
        public string DockerKeyPath { get; private set; }
        public string DockerCAPath { get; private set; }
        public string ContainerId { get; private set; }

        public DockerStartContainer(XElement xe, Workflow wf) : base(xe, wf)
        {
            DockerHost = GetSetting("dockerHost");
            DockerPort = GetSettingInt("dockerPort", 2376); // Default to 2376 for TLS
            DockerCertPath = GetSetting("dockerCertPath");
            DockerKeyPath = GetSetting("dockerKeyPath");
            DockerCAPath = GetSetting("dockerCAPAth");
            ContainerId = GetSetting("containerId");
        }

        public override TaskStatus Run()
        {
            Info("Starting container...");
            try
            {
                HttpClientHandler handler = CreateHandler(DockerCAPath, DockerCertPath, DockerKeyPath);
                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri($"{(DockerPort == 2375 ? "http" : "https")}://{DockerHost}:{DockerPort}/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Synchronous call to PostAsync using Result
                    HttpResponseMessage response = client.PostAsync("/containers/" + ContainerId + "/start", null).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        Info("The container was started.");
                        return new TaskStatus(Status.Success, true);
                    }
                    else
                    {
                        ErrorFormat("An error occured while starting the container. Status code: {0}", response.StatusCode);
                        return new TaskStatus(Status.Error, false);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while starting the container. Error: {0}", e.Message);
                return new TaskStatus(Status.Error, false);
            }

            return new TaskStatus(Status.Success);
        }

        private static HttpClientHandler CreateHandler(string caPath, string certPath, string keyPath)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;

            // Load and add the client certificate
            string certText = System.IO.File.ReadAllText(certPath);
            string keyText = System.IO.File.ReadAllText(keyPath);
            X509Certificate2 clientCertificate = X509Certificate2.CreateFromPem(certText, keyText);
            handler.ClientCertificates.Add(clientCertificate);

            // Load the CA certificate to validate the server
            X509Certificate2 caCertificate = new X509Certificate2(caPath);
            handler.ServerCertificateCustomValidationCallback = (HttpRequestMessage, cert, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None) return true;

                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain.ChainPolicy.ExtraStore.Add(caCertificate);

                bool isChainValid = chain.Build(cert as X509Certificate2);
                return isChainValid;
            };

            return handler;
        }
    }
}
