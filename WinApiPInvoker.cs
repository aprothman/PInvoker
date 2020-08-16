using System.Runtime.InteropServices;

namespace DynamicPInvoke
{
    public class WinApiPInvoker : PInvoker
    {
        public WinApiPInvoker()
        {
            DllNameSuffix[Architecture.X64] = "";
            DllNameSuffix[Architecture.Arm64] = "";
            FinalizeInit();
        }
    }
}
