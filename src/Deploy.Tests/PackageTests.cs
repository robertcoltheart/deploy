using Xunit;

namespace Deploy
{
    public class PackageTests
    {
        [Fact]
        public void Fact()
        {
            var package = new Package(@"C:\Windows\Installer\530580c4.msi");
        }
    }
}
