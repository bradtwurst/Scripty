using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Scripty
{
    internal sealed class ApplyCustomTool
    {
        private const int _commandId = 0x0100;  //must match ApplyCustomToolId value in .vsct file
        private static readonly Guid _commandSet = new Guid("516f93c0-70ae-4a4b-9fb6-1ad3997a9516"); //must match guidPackageCmdSet value in .vsct file
        private static DTE _dte;
        
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(_dte);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as IMenuCommandService;
            Assumes.Present(commandService);

            var cmdId = new CommandID(_commandSet, _commandId);

            var cmd = new OleMenuCommand(OnExecute, cmdId)
            {
                // This will defer visibility control to the VisibilityConstraints section in the .vsct file
                Supported = false
            };

            commandService.AddCommand(cmd);
        }

        private static void OnExecute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProjectItem item = _dte.SelectedItems.Item(1).ProjectItem;

            if (item != null)
            {
                item.Properties.Item("CustomTool").Value = ScriptyCodeGenerator.Name;
            }
        }
    }
}
