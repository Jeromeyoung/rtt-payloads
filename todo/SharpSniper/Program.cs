using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpSniper
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
            if (args.Length != 3 && args.Length != 1)
            {
                System.Console.WriteLine("\r\n\r\nSniper: Find hostname and IP address of specific user (CEO etc) in Domain (requires Domain Admin Rights or DC Event" +
                    "logs must be readable by your user.");
                System.Console.WriteLine("Usage:");
                System.Console.WriteLine("Credentialed Auth:   Sniper.exe TARGET_USERNAME DAUSER DAPASSWORD");
                System.Console.WriteLine("Process Token Auth:  Sniper.exe TARGET_USERNAME");
                // System.Environment.Exit(1);
            }

            string targetusername = String.Empty;
            string dauser = String.Empty;
            string dapass = String.Empty;

            bool credentialed = false;

            targetusername = args[0];

            if (args.Length == 3)
            {
                credentialed = true;
                dauser = args[1];
                dapass = args[2];
            }

            string domain_name = DomainInformation.GetDomainOrWorkgroup();

            Domain domain = Domain.GetCurrentDomain();

            //Finds all of the discoverable domain controllers in this domain.
            List<string> dclist = new List<string>();
            foreach (DomainController dc in domain.FindAllDiscoverableDomainControllers())
            {
                dclist.Add(dc.Name);
            }
            // Loop through domain controllers and find hostname of user
            foreach (string dcfound in dclist)
            {
                string target_hostname = string.Empty;
                target_hostname = credentialed ?
                                    QueryDC.QueryRemoteComputer(targetusername, domain_name, dcfound, dauser, dapass) :
                                    QueryDC.QueryRemoteComputer(targetusername, domain_name, dcfound);
                Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                MatchCollection result = ip.Matches(target_hostname);
                {
                    Console.WriteLine("User: " + targetusername + " - " + "IP Address: " + result[0]);
                }
                
            }


        }

    }
}

