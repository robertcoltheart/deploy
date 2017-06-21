using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        [Fact]
        public void IsSerializableDecorated()
        {
            Assert.True(typeof(PackageException).GetCustomAttributes(typeof(SerializableAttribute), true).Length == 1, "Type must be serializable.");
        }

        [Fact]
        public void IsSerializable()
        {
            var exception = new PackageException();

            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, exception);

                stream.Position = 0;

                exception = (PackageException) formatter.Deserialize(stream);
            }

            Assert.NotNull(exception);
        }
    }
}
