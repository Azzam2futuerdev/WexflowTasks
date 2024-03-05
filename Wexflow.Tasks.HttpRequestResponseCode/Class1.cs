using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;
using System.Net;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace Wexflow.Tasks.HttpRequestResponseCode
{
    public class HttpRequestResponseCode : Task
    {
        public HttpRequestResponseCode(XElement xe, Workflow wf) : base(xe, wf)
    {
        }

        public override TaskStatus Run()
    {
            Info("Getting the response code...");
            var url = this.GetSetting("url");
            var responseCode = GetResponseCode(url);

            var outputFilePath = Path.Combine(Workflow.WorkflowTempFolder,
                $"HTTPCode_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.txt");
            File.WriteAllText(outputFilePath, responseCode.ToString());
            Files.Add(new FileInf(outputFilePath, Id));
            InfoFormat("The response code of the url {0} is {1}.", url, responseCode);
            return new TaskStatus(Status.Success);
        }

        private int GetResponseCode(string url)
    {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            try
        {
                using (var response = (HttpWebResponse)request.GetResponse())
            {
                    return (int)response.StatusCode;
                }
            }
            catch (WebException e)
        {
                if (e.Response != null)
            {
                    using (var response = (HttpWebResponse)e.Response)
                {
                        return (int)response.StatusCode;
                    }
                }
                return 0;
            }
        }
    }
}
