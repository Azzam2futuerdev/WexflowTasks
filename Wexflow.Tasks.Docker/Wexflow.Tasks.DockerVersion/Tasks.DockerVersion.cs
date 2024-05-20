using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Wexflow.Tasks.DockerVersion
{
    public class DockerVersion : Task
    {
        public string DockerHost { get; private set; }
        public int DockerPort { get; private set; }
        public string DockerCertPath { get; private set; }
        public string DockerKeyPath { get; private set; }
        public string DockerCAPath { get; private set; }

        public DockerVersion(XElement xe, Workflow wf) : base(xe, wf)
        {
            DockerHost = GetSetting("dockerHost");
            DockerPort = GetSettingInt("dockerPort", 2376); // Default to 2376 for TLS
            DockerCertPath = GetSetting("dockerCertPath");
            DockerKeyPath = GetSetting("dockerKeyPath");
            DockerCAPath = GetSetting("dockerCAPAth");
        }

        public override TaskStatus Run()
        {
            Info("Getting Docker info...");

            try
            {

                HttpClientHandler handler = CreateHandler(DockerCAPath, DockerCertPath, DockerKeyPath);
                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri($"{(DockerPort == 2375 ? "http" : "https")}://{DockerHost}:{DockerPort}/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Synchronous call to GetAsync using Result
                    HttpResponseMessage response = client.GetAsync("info").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        Info("Docker info retrieved.");
                        // Parse the JSON response
                        var jsonObject = JObject.Parse(result);
						
						// Name
                        string name = jsonObject["Name"].Value<string>();
                        Info($"Name: {name}");

                        // Number of Containers
                        int containers = jsonObject["Containers"].Value<int>();
                        Info($"Number of Containers: {containers}");
						
						// Number of ContainersRunning
                        int rcontainers = jsonObject["ContainersRunning"].Value<int>();
                        Info($"Number of Running Containers: {rcontainers}");

						// Number of ContainersPaused
                        int pcontainers = jsonObject["ContainersPaused"].Value<int>();
                        Info($"Number of Paused Containers: {pcontainers}");
						
						// Number of ContainersStopped
                        int scontainers = jsonObject["ContainersStopped"].Value<int>();
                        Info($"Number of Stopped Containers: {scontainers}");
						
                        // Add more fields as needed
                        int images = jsonObject["Images"].Value<int>();
                        Info($"Number of Images: {images}");

                        string dockerVersion = jsonObject["ServerVersion"].Value<string>();
                        Info($"Docker Server Version: {dockerVersion}");

                        return new TaskStatus(Status.Success, false);
                    }
                    else
                    {
                        Error("An error occurred while getting Docker info.");
                        return new TaskStatus(Status.Error, false);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorFormat("An error occurred while getting Docker info: {0}", e.Message);
                return new TaskStatus(Status.Error, false);
            }
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