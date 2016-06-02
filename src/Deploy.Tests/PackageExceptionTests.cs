using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Deploy
{
    [TestFixture]
    public class PackageExceptionTests
    {
        [Test]
        public void IsTypeOfException()
        {
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(typeof(PackageException)), "Type is not an exception.");
        }

        [Test]
        public void InnerExceptionSet()
        {
            var innerException = new Exception();
            var exception = new PackageException("msg", innerException);

            Assert.AreSame(innerException, exception.InnerException);
        }

        [Test]
        public void HasMessage()
        {
            var exception = new PackageException("msg");

            Assert.AreEqual("msg", exception.Message);
        }

        [Test]
        public void IsSerializableDecorated()
        {
            Assert.IsTrue(typeof(PackageException).GetCustomAttributes(typeof(SerializableAttribute), true).Length == 1, "Type must be serializable.");
        }

        [Test]
        public void IsSerializable()
        {
            var exception = new PackageException();

            try
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, exception);

                    stream.Position = 0;

                    exception = (PackageException)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                Assert.Fail("Type must be serializable.");
            }

            Assert.IsNotNull(exception, "Type could not be deserialized.");
        }
    }
}
