using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
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

    [Parameter("Hello param decription")] readonly string Hello;


    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
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
        .DependsOn(Restore, Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s =>
                s.SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
        });
}