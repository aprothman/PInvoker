using DynamicPInvoke;
using NUnit.Framework;

namespace PInvokerTests
{
    [TestFixture]
    public class NameTransformsTests
    {
        [TestCase("test", "test")]
        [TestCase("Test", "Test")]
        [TestCase("TestIt", "TestIt")]
        [TestCase("testIt", "testIt")]
        [TestCase("test_it", "test_it")]
        [TestCase("TEST_IT", "TEST_IT")]
        [TestCase("__test", "__test")]
        [TestCase("__testIt", "__testIt")]
        [TestCase("ManyTokenTestWithAVariety", "ManyTokenTestWithAVariety")]
        public void NoOp_Invoke_NoChange(string original, string expected)
        {
            var result = NameTransforms.NoOp(original);

            Assert.AreEqual(expected, result);
        }

        [TestCase("test", "test")]
        [TestCase("Test", "test")]
        [TestCase("TestIt", "testIt")]
        [TestCase("testIt", "testIt")]
        [TestCase("test_it", "testIt")]
        [TestCase("TEST_IT", "testIt")]
        [TestCase("__test", "test")]
        [TestCase("__testIt", "testIt")]
        [TestCase("ManyTokenTestWithAVariety", "manyTokenTestWithAVariety")]
        public void Camel_Invoke_Changes(string original, string expected)
        {
            var result = NameTransforms.Camel(original);

            Assert.AreEqual(expected, result);
        }

        [TestCase("test", "test")]
        [TestCase("Test", "test")]
        [TestCase("TestIt", "test_it")]
        [TestCase("testIt", "test_it")]
        [TestCase("test_it", "test_it")]
        [TestCase("TEST_IT", "test_it")]
        [TestCase("__test", "test")]
        [TestCase("__testIt", "test_it")]
        [TestCase("ManyTokenTestWithAVariety", "many_token_test_with_a_variety")]
        public void Snake_Invoke_Changes(string original, string expected)
        {
            var result = NameTransforms.Snake(original);

            Assert.AreEqual(expected, result);
        }

        [TestCase("test", "Test")]
        [TestCase("Test", "Test")]
        [TestCase("TestIt", "TestIt")]
        [TestCase("testIt", "TestIt")]
        [TestCase("test_it", "TestIt")]
        [TestCase("TEST_IT", "TestIt")]
        [TestCase("__test", "Test")]
        [TestCase("__testIt", "TestIt")]
        [TestCase("ManyTokenTestWithAVariety", "ManyTokenTestWithAVariety")]
        public void Pascal_Invoke_Changes(string original, string expected)
        {
            var result = NameTransforms.Pascal(original);

            Assert.AreEqual(expected, result);
        }

        [TestCase("test", "TEST")]
        [TestCase("Test", "TEST")]
        [TestCase("TestIt", "TEST_IT")]
        [TestCase("testIt", "TEST_IT")]
        [TestCase("test_it", "TEST_IT")]
        [TestCase("TEST_IT", "TEST_IT")]
        [TestCase("__test", "TEST")]
        [TestCase("__testIt", "TEST_IT")]
        [TestCase("ManyTokenTestWithAVariety", "MANY_TOKEN_TEST_WITH_A_VARIETY")]
        public void CapSnake_Invoke_Changes(string original, string expected)
        {
            var result = NameTransforms.CapSnake(original);

            Assert.AreEqual(expected, result);
        }
    }
}
