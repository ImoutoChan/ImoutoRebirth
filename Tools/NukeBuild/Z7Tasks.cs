using System;
using System.Collections.Generic;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

public static class Z7Tasks
{
    public static void PackAs7Z(Action<Z7SfxSettings> configurator)
    {
        var settings = new Z7SfxSettings();
        configurator(settings);

        ProcessTasks
            .StartProcess(
                settings.ProcessToolPath, 
                settings.GetArguments(), 
                logger: settings.ProcessExitHandler,
                logOutput: true, 
                logInvocation: true)
            .WaitForExit();
    }
    
    public class Z7SfxSettings
    {
        public string ProcessToolPath => @"C:\Program Files\7-Zip\7z.exe";

        public Action<OutputType, string> ProcessExitHandler => (type, text) =>
        {
            if (type == OutputType.Err)
            {
                Serilog.Log.Error(string.Join("\n", text));
            }
            else
            {
                Serilog.Log.Information("Archive created");
            }
        };

        public string Command { get; private set; }
    
        public string Switch { get; private set; }
    
        public string ArchiveName { get; private set; }
    
        public string SourceName { get; private set; }

        public Z7SfxSettings CreateArchive()
        {
            Command = "a";
            return this;
        }

        public Z7SfxSettings AsSfx()
        {
            Switch = "-sfx";
            return this;
        }

        public Z7SfxSettings SetOutputArchiveFile(AbsolutePath archiveName)
        {
            ArchiveName = archiveName;
            return this;
        }

        public Z7SfxSettings SetSourceDirectory(string sourceName)
        {
            SourceName = sourceName;
            return this;
        }

        public string GetArguments()
        {
            var arguments = new List<string>();

            arguments.Add(Command);
            
            if (!string.IsNullOrWhiteSpace(Switch))
                arguments.Add(Switch);

            arguments.Add($"\"{ArchiveName}\"");
            arguments.Add($"\"{SourceName}\\");

            return string.Join(" ", arguments);
        }
    }
}
