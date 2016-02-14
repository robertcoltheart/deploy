using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Deploy
{
    [TestClass]
    public class DeployBuilderTests
    {
        [TestMethod]
        public void TesT()
        {
            var builder = new PackageBuilder();

            builder.UpgradeCode(Guid.NewGuid())
                .Build("output");
        }
    }
}