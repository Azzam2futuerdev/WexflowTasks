using Wexflow.Core;
using Task= Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;
using System.Xml.Linq;

namespace Wexflow.Tasks.FlipCoin
{
    public class FlipCoin :Task
    {
        public FlipCoin(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Flipping coin...");
            var result = new Random().Next(0, 2) == 0 ? "Heads" : "Tails";
            InfoFormat("Result: {0}", result);
            return new TaskStatus(Status.Success);
        }

    }
}