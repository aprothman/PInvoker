using System.Runtime.InteropServices;
using NUnit.Framework;

namespace DynamicPInvoke.Tests
{
    [TestFixture]
    public class ArchTupleTests
    {
        [Test]
        public void Value_SetInt_Sets()
        {
            var tuple = new ArchTuple<int> {
                [Architecture.X86] = 1,
                [Architecture.X64] = 2,
                [Architecture.Arm] = 3,
                [Architecture.Arm64] = 4,
            };

            Assert.AreEqual(1, tuple[Architecture.X86]);
            Assert.AreEqual(2, tuple[Architecture.X64]);
            Assert.AreEqual(3, tuple[Architecture.Arm]);
            Assert.AreEqual(4, tuple[Architecture.Arm64]);
        }

        [Test]
        public void Value_SetString_Sets()
        {
            var tuple = new ArchTuple<string> {
                [Architecture.X86] = "test1",
                [Architecture.X64] = "test2",
                [Architecture.Arm] = "test3",
                [Architecture.Arm64] = "test4",
            };

            Assert.AreEqual("test1", tuple[Architecture.X86]);
            Assert.AreEqual("test2", tuple[Architecture.X64]);
            Assert.AreEqual("test3", tuple[Architecture.Arm]);
            Assert.AreEqual("test4", tuple[Architecture.Arm64]);
        }

        [Test]
        public void Value_SetEnum_Sets()
        {
            var tuple = new ArchTuple<CharSet> {
                [Architecture.X86] = CharSet.Ansi,
                [Architecture.X64] = CharSet.Unicode,
                [Architecture.Arm] = CharSet.Ansi,
                [Architecture.Arm64] = CharSet.Unicode,
            };

            Assert.AreEqual(CharSet.Ansi, tuple[Architecture.X86]);
            Assert.AreEqual(CharSet.Unicode, tuple[Architecture.X64]);
            Assert.AreEqual(CharSet.Ansi, tuple[Architecture.Arm]);
            Assert.AreEqual(CharSet.Unicode, tuple[Architecture.Arm64]);
        }

        [Test]
        public void Value_SetInt_Changes()
        {
            var tuple = new ArchTuple<int>(5);
            tuple[Architecture.X86] = 1;
            tuple[Architecture.X64] = 2;
            tuple[Architecture.Arm] = 3;
            tuple[Architecture.Arm64] = 4;

            Assert.AreEqual(1, tuple[Architecture.X86]);
            Assert.AreEqual(2, tuple[Architecture.X64]);
            Assert.AreEqual(3, tuple[Architecture.Arm]);
            Assert.AreEqual(4, tuple[Architecture.Arm64]);
        }

        [Test]
        public void Value_SetString_Changes()
        {
            var tuple = new ArchTuple<string>("test");
            tuple[Architecture.X86] = "test1";
            tuple[Architecture.X64] = "test2";
            tuple[Architecture.Arm] = "test3";
            tuple[Architecture.Arm64] = "test4";

            Assert.AreEqual("test1", tuple[Architecture.X86]);
            Assert.AreEqual("test2", tuple[Architecture.X64]);
            Assert.AreEqual("test3", tuple[Architecture.Arm]);
            Assert.AreEqual("test4", tuple[Architecture.Arm64]);
        }

        [Test]
        public void Value_SetEnum_Changes()
        {
            var tuple = new ArchTuple<CharSet>(CharSet.Auto);
            tuple[Architecture.X86] = CharSet.Ansi;
            tuple[Architecture.X64] = CharSet.Unicode;
            tuple[Architecture.Arm] = CharSet.Ansi;
            tuple[Architecture.Arm64] = CharSet.Unicode;

            Assert.AreEqual(CharSet.Ansi, tuple[Architecture.X86]);
            Assert.AreEqual(CharSet.Unicode, tuple[Architecture.X64]);
            Assert.AreEqual(CharSet.Ansi, tuple[Architecture.Arm]);
            Assert.AreEqual(CharSet.Unicode, tuple[Architecture.Arm64]);
        }

        [Test]
        public void SetValues_Sets()
        {
            var tuple = new ArchTuple<string>();
            tuple.SetValues("test");

            Assert.AreEqual("test", tuple[Architecture.X86]);
            Assert.AreEqual("test", tuple[Architecture.X64]);
            Assert.AreEqual("test", tuple[Architecture.Arm]);
            Assert.AreEqual("test", tuple[Architecture.Arm64]);
        }

        [Test]
        public void Constructor_Sets()
        {
            var tuple = new ArchTuple<string>("test");

            Assert.AreEqual("test", tuple[Architecture.X86]);
            Assert.AreEqual("test", tuple[Architecture.X64]);
            Assert.AreEqual("test", tuple[Architecture.Arm]);
            Assert.AreEqual("test", tuple[Architecture.Arm64]);
        }

        [Test]
        public void Implicit_String_Converts()
        {
            ArchTuple<string> tuple = "test";

            Assert.AreEqual("test", tuple[Architecture.X86]);
            Assert.AreEqual("test", tuple[Architecture.X64]);
            Assert.AreEqual("test", tuple[Architecture.Arm]);
            Assert.AreEqual("test", tuple[Architecture.Arm64]);
        }


        [Test]
        public void Implicit_Int_Converts()
        {
            ArchTuple<int> tuple = 1;

            Assert.AreEqual(1, tuple[Architecture.X86]);
            Assert.AreEqual(1, tuple[Architecture.X64]);
            Assert.AreEqual(1, tuple[Architecture.Arm]);
            Assert.AreEqual(1, tuple[Architecture.Arm64]);
        }

        [Test]
        public void Implicit_Enum_Converts()
        {
            ArchTuple<CharSet> tuple = CharSet.Auto;

            Assert.AreEqual(CharSet.Auto, tuple[Architecture.X86]);
            Assert.AreEqual(CharSet.Auto, tuple[Architecture.X64]);
            Assert.AreEqual(CharSet.Auto, tuple[Architecture.Arm]);
            Assert.AreEqual(CharSet.Auto, tuple[Architecture.Arm64]);
        }
    }
}
