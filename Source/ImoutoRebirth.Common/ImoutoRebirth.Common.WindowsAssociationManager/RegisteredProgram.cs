using System.Diagnostics;
using Microsoft.Win32;

namespace ImoutoRebirth.Common.WindowsAssociationManager;

[DebuggerDisplay("{Name}")]
public class RegisteredProgram
{
    public RegisteredProgram(string productname)
    {
        Name = productname;
    }


    public string Name { get; }


    public Dictionary<string, string> Extensions { get; } = new Dictionary<string, string>();


    public Dictionary<string, string> Mimes { get; } = new Dictionary<string, string>();


    public static IEnumerable<RegisteredProgram> GetRegisteredPrograms()
    {
        var list = new List<RegisteredProgram>();
        using var registryKey = Registry.LocalMachine.OpenSubKey("Software\\RegisteredApplications");
        var flag = registryKey != null;
        if (flag)
        {
            foreach (var text in registryKey.GetValueNames())
            {
                var text2 = registryKey.GetValue(text) as string;
                var flag2 = string.IsNullOrEmpty(text2);
                if (!flag2)
                {
                    text2 = text2.TrimEnd(new[]
                    {
                        '\\'
                    }) + "\\";
                    var registeredProgram = new RegisteredProgram(text);
                    list.Add(registeredProgram);
                    string[] array =
                    {
                        "FileAssociations",
                        "MimeAssociations",
                        "URLAssociations",
                        "StartMenu"
                    };
                    var array2 = array;
                    var j = 0;
                    while (j < array2.Length)
                    {
                        var text3 = array2[j];
                        using (var registryKey2 = Registry.LocalMachine.OpenSubKey(text2 + text3))
                        {
                            var flag3 = registryKey2 == null;
                            if (!flag3)
                            {
                                foreach (var text4 in registryKey2.GetValueNames())
                                {
                                    var value = registryKey2.GetValue(text4) as string;
                                    var flag4 = string.IsNullOrEmpty(value);
                                    if (!flag4)
                                    {
                                        var a = text3;
                                        if (a != "FileAssociations")
                                        {
                                            if (a != "MimeAssociations")
                                            {
                                                if (a != "URLAssociations")
                                                {
                                                    if (a != "StartMenu")
                                                    {
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                registeredProgram.Mimes.Add(text4, value);
                                            }
                                        }
                                        else
                                        {
                                            registeredProgram.Extensions.Add("." + text4.TrimStart(new[]
                                            {
                                                '.'
                                            }), value);
                                        }
                                    }
                                }
                            }
                        }

                        j++;
                    }
                }
            }
        }

        return list;
    }
}
