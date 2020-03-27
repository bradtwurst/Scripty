using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Scripty.MsBuild.Tests
{
    [TestFixture]
    public class ScriptyTaskFixture
    {
        static readonly string SolutionFilePath = Path.GetFullPath($"{AppDomain.CurrentDomain.BaseDirectory}/SampleSolution/Sample.sln");
        static readonly string ProjectFilePath = Path.GetFullPath($"{AppDomain.CurrentDomain.BaseDirectory}/SampleSolution/Proj/Proj.csproj");
        static readonly string ScriptyAssembly = Path.GetFullPath($"{AppDomain.CurrentDomain.BaseDirectory}/Scripty.MsBuild.dll");
        static readonly string _vswhere = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "vswhere.exe");

        string _msbuild;
        string _output;

        [OneTimeSetUp]
        public void InitFixture()
        {
            _msbuild = FindMsBuild();
            _output = Path.Combine(Path.GetDirectoryName(ProjectFilePath), "test.cs");
        }

        private static string FindMsBuild()
        {
            var args = @"-latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe";
            var info = new ProcessStartInfo(_vswhere, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var output = new List<string>();
            Process p = new Process();
            p.StartInfo = info;
            p.OutputDataReceived += (s, e) => output.Add(e.Data);
            p.ErrorDataReceived += (s, e) => output.Add(e.Data);
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();

            var exe = output.FirstOrDefault(f => File.Exists(f));
            
            return exe ?? throw new ApplicationException("Could not find the location of MSBuild.");
        }

        [SetUp]
        public void InitTest()
        {
            File.Delete(_output);
        }

        [Test]
        public void UsesSolutionAndProperties()
        {
            var args = $"\"{SolutionFilePath}\" /p:ScriptyAssembly=\"{ScriptyAssembly}\";Include1=true;Include3=true";

            var info = new ProcessStartInfo(_msbuild, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process p = new Process();
            p.StartInfo = info;
            p.OutputDataReceived += (s, e) => TestContext.Out.WriteLine(e.Data);
            p.ErrorDataReceived += (s, e) => TestContext.Out.WriteLine(e.Data);
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();

            Assert.AreEqual(0, p.ExitCode);
            Assert.That(File.Exists(_output));
            Assert.AreEqual($@"//Class1.cs;Class3.cs;ClassSolution.cs", File.ReadAllText(_output));
        }

    }
}
