using System.Xml.Linq;
using Wexflow.Core;
using Task = Wexflow.Core.Task;
using TaskStatus = Wexflow.Core.TaskStatus;

namespace Wexflow.Tasks.PGDumpRemoteDB
{
    internal class PGDumpRemoteDB : Task
    {
        public string Host { get; private set; }
        public string Port { get; private set; }
        public string DbName { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string PgDumpPath { get; private set; }

        public PGDumpRemoteDB(XElement xe, Workflow wf) : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = GetSetting("port");
            DbName = GetSetting("dbName");
            Username = GetSetting("username");
            Password = GetSetting("password");
            PgDumpPath = GetSetting("pgDumpPath", string.Empty);
        }

        public override TaskStatus Run()
        {
            Info("Dumping remote database...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            foreach (var file in files)
            {
                var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                                       string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.sql", file.FileNameWithoutExtension,
                                                              System.DateTime.Now));
                var processInfo = new ProcessStartInfo
                {
                    FileName = PgDumpPath,
                    Arguments = string.Format("-h {0} -p {1} -U {2} -d {3} -f {4}", Host, Port, Username, DbName,
                                                              destPath),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = new System.Diagnostics.Process { StartInfo = processInfo };

                process.Start();
                process.WaitForExit();

                var error = process.StandardError.ReadToEnd();
                var output = process.StandardOutput.ReadToEnd();

                if (process.ExitCode == 0)
                {
                    InfoFormat("The database {0} was successfully dumped to {1}", DbName, destPath);
                    Files.Add(new FileInf(destPath, Id));
                    atLeastOneSucceed = true;
                }
                else
                {
                    success = false;
                    ErrorFormat("An error occured while dumping the database {0}", DbName);
                    Error(error);
                }
            }

            var status = WorkflowStatus.Success;

            if (!success && atLeastOneSucceed)
            {
                status = WorkflowStatus.WarningTask;
                {
                }
            }
        } 
    }
}
