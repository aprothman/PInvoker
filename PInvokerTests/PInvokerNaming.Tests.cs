using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace DynamicPInvoke.Tests
{
    [TestFixture]
    public class PInvokerNamingTests
    {
        private class TestWrapper : PInvoker { }

        [Test]
        public void DllNaming_Default_GeneratesName()
        {
            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "Test.dll",
                [Architecture.Arm] = "Test.dll",
                [Architecture.X64] = "Test64.dll",
                [Architecture.Arm64] = "Test64.dll",
            };
            string expected = tuple;

            var test = new TestWrapper();
            var name = test.ComposeDllName();

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void DllNaming_ChangePath_GeneratesName()
        {
            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "lib32\\Test.dll",
                [Architecture.Arm] = "libarm\\Test.dll",
                [Architecture.X64] = "lib64\\Test.dll",
                [Architecture.Arm64] = "libarm64\\Test.dll",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.DllPath[Architecture.X86] = "lib32\\";
            test.DllPath[Architecture.X64] = "lib64\\";
            test.DllPath[Architecture.Arm] = "libarm\\";
            test.DllPath[Architecture.Arm64] = "libarm64\\";

            var name = test.ComposeDllName();

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void DllNaming_ChangeName_GeneratesName()
        {
            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "FirstWrapper.dll",
                [Architecture.Arm] = "SecondWrapper.dll",
                [Architecture.X64] = "ThirdWrapper.dll",
                [Architecture.Arm64] = "FourthWrapper.dll",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.DllName[Architecture.X86] = "FirstWrapper";
            test.DllName[Architecture.Arm] = "SecondWrapper";
            test.DllName[Architecture.X64] = "ThirdWrapper";
            test.DllName[Architecture.Arm64] = "FourthWrapper";

            var name = test.ComposeDllName();

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void DllNaming_ChangePrefix_GeneratesName()
        {
            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "X86Test.dll",
                [Architecture.Arm] = "ArmTest.dll",
                [Architecture.X64] = "X64Test.dll",
                [Architecture.Arm64] = "Arm64Test.dll",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.DllNamePrefix[Architecture.X86] = "X86";
            test.DllNamePrefix[Architecture.Arm] = "Arm";
            test.DllNamePrefix[Architecture.X64] = "X64";
            test.DllNamePrefix[Architecture.Arm64] = "Arm64";

            var name = test.ComposeDllName();

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void DllNaming_ChangeSuffix_GeneratesName()
        {
            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "TestX86.dll",
                [Architecture.Arm] = "TestArm.dll",
                [Architecture.X64] = "TestX64.dll",
                [Architecture.Arm64] = "TestArm64.dll",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.DllNameSuffix[Architecture.X86] = "X86";
            test.DllNameSuffix[Architecture.Arm] = "Arm";
            test.DllNameSuffix[Architecture.X64] = "X64";
            test.DllNameSuffix[Architecture.Arm64] = "Arm64";

            var name = test.ComposeDllName();

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void MethodNaming_Default_GeneratesName()
        {
            var initial = "Test";

            var test = new TestWrapper();

            var name = test.ComposeMethodName(initial);

            Assert.AreEqual(initial, name);
        }

        [Test]
        public void MethodNaming_ChangeTransform_GeneratesName()
        {
            var initial = "Test";

            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "X86",
                [Architecture.Arm] = "Arm",
                [Architecture.X64] = "X64",
                [Architecture.Arm64] = "Arm64",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.MethodNameTransform[Architecture.X86] = (s) => "X86";
            test.MethodNameTransform[Architecture.Arm] = (s) => "Arm";
            test.MethodNameTransform[Architecture.X64] = (s) => "X64";
            test.MethodNameTransform[Architecture.Arm64] = (s) => "Arm64";

            var name = test.ComposeMethodName(initial);

            Assert.AreEqual(expected, name);
        }

        public void MethodNaming_ChangePrefix_GeneratesName()
        {
            var initial = "Test";

            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "X86Test",
                [Architecture.Arm] = "ArmTest",
                [Architecture.X64] = "X64Test",
                [Architecture.Arm64] = "Arm64Test",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.MethodNamePrefix[Architecture.X86] = "X86";
            test.MethodNamePrefix[Architecture.Arm] = "Arm";
            test.MethodNamePrefix[Architecture.X64] = "X64";
            test.MethodNamePrefix[Architecture.Arm64] = "Arm64";

            var name = test.ComposeMethodName(initial);

            Assert.AreEqual(expected, name);
        }

        [Test]
        public void MethodNaming_ChangeSuffix_GeneratesName()
        {
            var initial = "Test";

            var tuple = new ArchTuple<string>() {
                [Architecture.X86] = "TestX86",
                [Architecture.Arm] = "TestArm",
                [Architecture.X64] = "TestX64",
                [Architecture.Arm64] = "TestArm64",
            };
            string expected = tuple;

            var test = new TestWrapper();
            SetBaseline(test);
            test.MethodNameSuffix[Architecture.X86] = "X86";
            test.MethodNameSuffix[Architecture.Arm] = "Arm";
            test.MethodNameSuffix[Architecture.X64] = "X64";
            test.MethodNameSuffix[Architecture.Arm64] = "Arm64";

            var name = test.ComposeMethodName(initial);

            Assert.AreEqual(expected, name);
        }

        private void SetBaseline(PInvoker wrapper)
        {
            wrapper.DllName = "";
            wrapper.DllNamePrefix = "";
            wrapper.DllNameSuffix = "";
            wrapper.DllPath = "";
            wrapper.MethodNameTransform = NameTransforms.NoOp;
            wrapper.MethodNamePrefix = "";
            wrapper.MethodNameSuffix = "";
        }
    }
}
