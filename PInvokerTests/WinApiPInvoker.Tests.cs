using System;
using NUnit.Framework;

namespace DynamicPInvoke.Tests
{
    [TestFixture]
    public class WinApiPInvokerTests
    {
        private class User32Wrapper : WinApiPInvoker
        {
            [PInvokable]
            public IntPtr GetActiveWindow() { return (IntPtr)Result; }

            [PInvokable]
            public void ThisFunctionIsMissing() { }
        }

        private User32Wrapper _user32;

        [OneTimeSetUp]
        public void Init()
        {
            _user32 = new User32Wrapper();
        }

        [Test]
        public void PInvoke_Call_CallsReturns()
        {
            var initial = IntPtr.Add(IntPtr.Zero, 10);
            var hwnd = initial;
            hwnd = _user32.GetActiveWindow();

            Assert.AreNotEqual(initial, hwnd);
        }

        [Test]
        public void PInvoke_CallMissing_Throws()
        {
            Assert.Throws<System.Reflection.TargetInvocationException>(() => _user32.ThisFunctionIsMissing());
        }
    }
}
