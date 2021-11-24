from re import findall
from os import walk
from os.path import join, splitext

regex = r"static void Main\(.*\)"
blob = """[DllImport("shell32.dll", SetLastError = true)]
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
}"""

ioImport = "using System.IO;"
interopImport = "using System.Runtime.InteropServices;"

def main(filename):
    with open(filename, 'rt') as readFile:
        contents = readFile.read()
    
    if blob not in contents:
        contentsList = contents.split('\n')
        new = str()
        addingImports = True
        for line in contentsList:
            if addingImports:
                if not len(line):
                    if ioImport not in new:
                        new += ioImport + '\n'
                    if interopImport not in new:
                        new += interopImport + '\n'
                    new += '\n'

                    addingImports = False
                else:
                    new += line + '\n'
            else:
                match = findall(regex, line)
                if match:
                    new += blob + '\n\n'
                new += line + '\n'
        new = new.strip('\ufeff')
        with open(filename, 'wt') as writeFile:
            writeFile.write(new)

if __name__ == '__main__':
    for root, _, files in walk('.'):
        for file in files:
            name, extension = splitext(file)
            if '.cs' == extension:
                fullPath = join(root, file)
                with open(fullPath, 'rt') as readFile:
                    fileContents = readFile.read()
                    match = findall(regex, fileContents)
                    if match and len(match) == 1:
                        main(fullPath)
