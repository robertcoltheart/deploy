using Deploy.Api.Data;
using WixSharp;
using WixSharp.Controls;

namespace Deploy.Api
{
    public class DeployDatabase
    {
        public DeployDatabase(string filename)
        {
            var project = new Project();

            Compiler.BuildMsi(project);
        }

        public SummaryInformation Summary { get; }

        public TableCollection Tables { get; }
    }
}
