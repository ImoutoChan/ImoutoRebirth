using System;
using System.Linq;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

public static class Z7Tasks
{
    public static void PackAs7z(Action<Z7SfxSettings> configurator)
    {
        var settings = new Z7SfxSettings();
        configurator(settings);
        
        ProcessTasks.StartProcess(settings).WaitForExit();
    }
    
    public class Z7SfxSettings : ToolSettings
    {
        public override string ProcessToolPath => @"C:\Program Files\7-Zip\7z.exe";

        public override Action<ToolSettings, IProcess> ProcessExitHandler => (_, process) =>
        {
            if (process.Output.Any(x => x.Type == OutputType.Err))
            {
                Serilog.Log.Error(
                    string.Join("\n", process.Output.Where(x => x.Type == OutputType.Err).Select(x => x.Text)));
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

        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            var resultArguments = arguments
                .Add(Command);
            
            if (!string.IsNullOrWhiteSpace(Switch))
                resultArguments = resultArguments
                    .Add(Switch);

            return resultArguments
                .Add("\"{0}\"", ArchiveName)
                .Add("\"{0}\\", SourceName);
        }
    }
}
