namespace DynamicPInvoke
{
    public class WinApiPInvoker : PInvoker
    {
        public WinApiPInvoker()
        {
            DllNameSuffix[Architecture.x64] = "";
            FinalizeInit();
        }
    }
}
