// sample at https://github.com/madskristensen/SingleFileGeneratorSample

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Scripty.Core;

namespace Scripty
{
    [Guid("516b7601-6a1b-4f28-a2d0-a435e6686516")]
    public sealed class ScriptyCodeGenerator : BaseCodeGeneratorWithSite
    {
        public const string Name = nameof(ScriptyCodeGenerator);
        public const string Description = "Runs Scripty as a custom tool.";

        public override string GetDefaultExtension()
        {
            return ".log";
            //Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            //var item = GetService(typeof(ProjectItem)) as ProjectItem;
            //return ".generated" + Path.GetExtension(item?.FileNames[1]);
        }
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var projectItem = GetService(typeof(ProjectItem)) as ProjectItem;
                var inputFilePath = projectItem.Properties.Item("FullPath").Value.ToString();
                var project = projectItem.ContainingProject;
                var solution = project.DTE.Solution;

                // Run the generator and get the results
                var source = new ScriptSource(inputFilePath, inputFileContent);
                var engine = new ScriptEngine(project.FullName, solution.FullName, null);
                var result = ThreadHelper.JoinableTaskFactory.RunAsync(
                    () =>
                    {
                        return engine.Evaluate(source);
                    }
                ).Join();

                // Report errors
                if (result.Messages.Count > 0)
                {
                    foreach (ScriptMessage error in result.Messages)
                    {
                        switch (error.MessageType)
                        {
                            case MessageType.Error:
                                GeneratorErrorCallback(warning: false, level: 4, message: error.Message, line: error.Line, column: error.Column);
                                break;
                            case MessageType.Warning:
                                GeneratorErrorCallback(warning: true, level: 4, message: error.Message, line: error.Line, column: error.Column);
                                break;
                        }
                    }
                    return null;
                }

                // Add generated files to the project
                foreach (var outputFile in result.OutputFiles.Where(x => x.BuildAction != Scripty.Core.Output.BuildAction.GenerateOnly))
                {
                    var outputItem = projectItem.ProjectItems.Cast<ProjectItem>()
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                        .FirstOrDefault(x => x.Properties.Item("FullPath")?.Value?.ToString() == outputFile.FilePath)
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
                        ?? projectItem.ProjectItems.AddFromFile(outputFile.FilePath);

                    outputItem.Properties.Item("ItemType").Value = outputFile.BuildAction.ToString();
                }

                // Remove/delete files from the last generation but not in this one
                var logPath = Path.ChangeExtension(inputFilePath, ".log");
                if (File.Exists(logPath))
                {
                    var logLines = File.ReadAllLines(logPath);
                    foreach (var fileToRemove in logLines.Where(x => result.OutputFiles.All(y => y.FilePath != x)))
                    {
                        solution.FindProjectItem(fileToRemove)?.Delete();
                    }
                }

                // Create the log file
                return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, result.OutputFiles.Select(x => x.FilePath)));

            }
            catch (Exception ex)
            {
                GeneratorErrorCallback(warning: false, level: 4, message: ex.ToString(), line: 0, column: 0);
                return null;
            }

        }


    }
}
