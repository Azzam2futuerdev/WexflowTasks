using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Wexflow.Tasks.DockerVersion
{
    public class DockerVersion : Task
    {
        public string DockerHost { get; private set; }
        public int DockerPort { get; private set; }
        public string DockerCertPath { get; private set; }
        public string DockerTlsVerify { get; private set; }

        public DockerVersion(XElement xe, Workflow wf) : base(xe, wf)
        {
            DockerHost = GetSetting("dockerHost");
            DockerPort = GetSettingInt("dockerPort", 2375); // Default to 2376 for TLS
            DockerCertPath = GetSetting("dockerCertPath");
            DockerTlsVerify = GetSetting("dockerTlsVerify");
        }

        public override TaskStatus Run()
        {
            Info("Getting Docker info...");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if ( DockerPort == 2375)
                    {
                        client.BaseAddress = new Uri($"http://{DockerHost}:{DockerPort}/");
                    } 
                    else
                    {
                        client.BaseAddress = new Uri($"https://{DockerHost}:{DockerPort}/");
                    }

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
                        string name = jsonObject["Name"].Value<int>();
                        Info($"Name: {name}");

                        // Number of Containers
                        int containers = jsonObject["Containers"].Value<int>();
                        Info($"Number of Containers: {containers}");
						
						// Number of ContainersRunning
                        int rcontainers = jsonObject["ContainersRunning"].Value<int>();
                        Info($"Number of Running Containers: {rcontainers}");

						// Number of ContainersPaused
                        int pcontainers = jsonObject["ContainersPaused"].Value<int>();
                        Info($"Number of Running Containers: {pcontainers}");
						
						// Number of ContainersStopped
                        int scontainers = jsonObject["ContainersStopped"].Value<int>();
                        Info($"Number of Running Containers: {scontainers}");
						
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
    }
}