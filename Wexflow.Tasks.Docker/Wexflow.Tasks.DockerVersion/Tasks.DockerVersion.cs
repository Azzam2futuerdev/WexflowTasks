using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;
using System.Net.Http.Headers;

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
            DockerPort = GetSettingInt("dockerPort", 2376); // Default to 2376 for TLS
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
                    client.BaseAddress = new Uri($"https://{DockerHost}:{DockerPort}/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Synchronous call to GetAsync using Result
                    HttpResponseMessage response = client.GetAsync("info").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        Info("Docker info retrieved.");
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