using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
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
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    
    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [GitRepository] readonly GitRepository Repository;
    
    Target Clean => _ => _
        .Requires(() => Repository.IsOnMainOrMasterBranch())
        .Before(Restore)
        .DependentFor(Compile)
        .Executes(() =>
        {
            
            Serilog.Log.Information("Commit = {Value}", Repository.Commit);
            Serilog.Log.Information("Branch = {Value}", Repository.Branch);
            Serilog.Log.Information("Tags = {Value}", Repository.Tags);
            Serilog.Log.Information("main branch = {Value}", Repository.IsOnMainBranch());
            Serilog.Log.Information("main/master branch = {Value}", Repository.IsOnMainOrMasterBranch());
            Serilog.Log.Information("release/* branch = {Value}", Repository.IsOnReleaseBranch());
            Serilog.Log.Information("hotfix/* branch = {Value}", Repository.IsOnHotfixBranch());
            Serilog.Log.Information("Https URL = {Value}", Repository.HttpsUrl);
            Serilog.Log.Information("SSH URL = {Value}", Repository.SshUrl);

            Repository.GetLatestRelease();
        });


    Target GitRepo => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information(Configuration);
        });

    Target Restore => _ => _
        .DependsOn(GitRepo)
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
                s.SetProjectFile(Solution.Nuke_FunctionalTests)
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

    [GitVersion(Framework = "net6.0")] readonly GitVersion GitVersion;
    
    AbsolutePath PackageAbsolutePath => RootDirectory / "packages";
    
    Target Pack => _ => _
        .DependsOn(StopApi,RunTests)
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(p =>
                p.SetProject(RootDirectory / "Nuke.WebApplication")
                    .SetOutputDirectory(PackageAbsolutePath / "Nuke.WebApplication")
                    .SetVersion(GitVersion.NuGetVersionV2)
            );
        });

    IEnumerable<AbsolutePath> Packages => PackageAbsolutePath.GlobFiles("*/*.nupkg");

    [Parameter] string ApiKey;

    Target Publish => _ => _
        .Requires(()=>ApiKey)
        .DependsOn(Pack)
        .Executes(() =>
        {
            DotNetTasks.DotNetNuGetPush(p =>
                p.SetSource("https://www.myget.org/F/secvbulrep/api/v2/package")
                    .SetApiKey(ApiKey)
                    .CombineWith(Packages,(_,v)=> _
                        .SetTargetPath(v))
            );
        });
    
   
    
    Target RemoveExistingContainer => _ => _
        .Executes(() =>
        {
            IReadOnlyCollection<string> containers = new[] { "my-postgres-container" };
            
            DockerTasks.DockerRm(s => s
                .SetContainers(containers)
                .SetForce(true)
            );
        });

    Target StartPostgres => _ => _
        .DependsOn(RemoveExistingContainer) // Сначала удаляем существующий контейнер
        .Executes(() =>
        {
            DockerTasks.DockerRun(s => s
                .SetImage("postgres")
                .SetEnv("POSTGRES_PASSWORD=mysecretpassword")
                //.SetPort("5432:5432")
                .SetName("my-postgres-container")
                .SetDetach(true)
            );
        });

}