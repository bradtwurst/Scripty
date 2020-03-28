using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Task = System.Threading.Tasks.Task;

namespace Scripty
{
    [Guid("51627fa3-8684-47fc-9674-004649986516")] // Must match the guidPackage value in the .vsct file
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(
        productName: "Scripty.CustomTool",
        productDetails: "Tools to let you use Roslyn-powered C# scripts for code generation. You can think of it as a scripted alternative to T4 templates.",
        productId: "0.7.4.0"
    )]       
    [ProvideMenuResource(
        resourceID: "Menus.ctmenu",
        version: 1
    )]
    [ProvideCodeGenerator(
        type: typeof(ScriptyCodeGenerator),
        name: ScriptyCodeGenerator.Name,
        description: ScriptyCodeGenerator.Description,
        generatesDesignTimeSource: true
    )]
    [ProvideUIContextRule(
        contextGuid: "51660bd3-80f0-4901-818d-c4656aaa0516", // Must match the guidUIContext value in the .vsct file
        name: "UI Context",
        expression: "csx", // This will make the button only show on .csx files
        termNames: new[] { "csx" },
        termValues: new[] { "HierSingleSelectionName:.csx$" })]
    public sealed class VSPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ApplyCustomTool.InitializeAsync(this);
        }
    }}
