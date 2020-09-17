using NUnit.Framework;
using PerudoBot.Extensions;
namespace PerudoBotTests
{
    public class StringExtensionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("d*^`\"'~ave", "dave")]
        [TestCase("abc@#<>def", "abc@#<>def")]
        public void Test1(string text, string expected)
        {
            var result = text.StripSpecialCharacters();
            Assert.AreEqual(expected, result);
        }
    }
}