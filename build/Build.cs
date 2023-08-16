using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;


    Target Clean => _ => _
        .Before(Restore)
        .DependentFor(Compile)
        .Executes(() =>
        {
        });


    Target Test => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information(Configuration);
        });

    Target Restore => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s =>
                s.SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
        });

    Target UnitTests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(s =>
                s.SetProjectFile(RootDirectory / "Nuke.UnitTests")
                    .EnableNoRestore()
                    .EnableNoBuild());
        });

    Target FunctionalTests => _ => _
        .DependsOn(Compile, StartApi)
        .Triggers(StopApi)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(s =>
                s.SetProjectFile(RootDirectory / "Nuke.FunctionalTests")
                    .EnableNoRestore()
                    .EnableNoBuild());
        });

    IProcess ApiProcess;
    Target StartApi => _ => _
        .Executes(() =>
        {
            ApiProcess = ProcessTasks.StartProcess("dotnet", "run", RootDirectory / "Nuke.WebApplication");
        });

    Target StopApi => _ => _
        .Executes(() =>
        {
            ApiProcess.Kill();
        });
    
    Target RunTests => _ => _
        .DependsOn(UnitTests, FunctionalTests)
        .Executes(() =>
        {
        });
}