using System.Collections.Generic;

namespace Wexflow.Tasks.DockerListContainers
{
    public class DockerContainerInfo
    {
        public string Id { get; set; }
        public List<string> Names { get; set; }
        public string Status { get; set; }
    }
}
