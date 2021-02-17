using System;
using Xunit;

namespace Deploy
{
    public class PackageExceptionTests
    {
        [Fact]
        public void IsTypeOfException()
        {
            Assert.IsAssignableFrom<Exception>(new PackageException());
        }

        [Fact]
        public void InnerExceptionSet()
        {
            var innerException = new Exception();
            var exception = new PackageException("msg", innerException);

            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void HasMessage()
        {
            var exception = new PackageException("msg");

            Assert.Equal("msg", exception.Message);
        }
    }
}
