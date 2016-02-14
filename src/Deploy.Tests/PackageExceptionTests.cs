using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Deploy
{
    [TestClass]
    public class PackageExceptionTests
    {
        [TestMethod]
        public void IsTypeOfException()
        {
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(typeof(PackageException)), "Type is not an exception.");
        }

        [TestMethod]
        public void InnerExceptionSet()
        {
            var innerException = new Exception();
            var exception = new PackageException("msg", innerException);

            Assert.AreSame(innerException, exception.InnerException);
        }

        [TestMethod]
        public void HasMessage()
        {
            var exception = new PackageException("msg");

            Assert.AreEqual("msg", exception.Message);
        }

        [TestMethod]
        public void IsSerializableDecorated()
        {
            Assert.IsTrue(typeof(PackageException).GetCustomAttributes(typeof(SerializableAttribute), true).Length == 1, "Type must be serializable.");
        }

        [TestMethod]
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