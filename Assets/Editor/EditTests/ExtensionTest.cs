using Flow.Net.Sdk.Core;
using NUnit.Framework;

namespace Editor.EditTests
{
    [TestFixture]
    public class ExtensionTest
    {
        [Test]
        public void StringToHex_Test()
        {
            var message = "test";
            var expected = "74657374";
            var actual = message.StringToHex();
            Assert.AreEqual(expected, actual);
        }
    }
}