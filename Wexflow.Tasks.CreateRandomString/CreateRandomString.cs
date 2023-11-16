using RndF;
using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;

namespace Wexflow.Tasks.CreateRandomString
{
    public class CreateRandomString : Task
    {
        public int Length;
        public bool LowerChar;
        public bool SpcChar;
        private string Result;
        public CreateRandomString(XElement xe, Workflow wf) : base(xe, wf)
        {
            Length = this.GetSettingInt("Length", 10);
            LowerChar = this.GetSettingBool("Lower", true);
            SpcChar = this.GetSettingBool("Special", false);
            Result = this.GetSetting("Result");
        }

        public override TaskStatus Run()
        {
            Info("Creating random string...");
            var memory = this.SharedMemory;
            var randomString = Rnd.StringF.Get(length: Length, chars: c => c with { Lower = LowerChar, Special = SpcChar });
            memory.Add(Result, randomString);
            InfoFormat("Random string {0} created.", randomString);
            return new TaskStatus(Status.Success);
        }

    }
}