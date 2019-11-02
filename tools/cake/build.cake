const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

var configuration = Argument("configuration",  "Release");
var version       = Argument("packageVersion", "");
var preRelease    = Argument("preRelease",     "");
var target        = Argument("target",         "Build");
var scope         = Argument("scope",          "lib");

////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// HERE BE DRAGONS
////////////////////////////////////////////////////////////////////////////////////////////////////////////
var wcRoot = Directory("./../../");
var rootDirectory = wcRoot.Path.FullPath;

#load "./../../config.cake"
#reference "./XyrusWorx.Management.dll"

using XyrusWorx.Management;
using System.Xml.Linq;

var scopeRoot = wcRoot + Directory("src/" + scope);
var projects = GetDirectories(scopeRoot.Path.FullPath + "/*");
var propFile = GetFiles(scopeRoot.Path.FullPath + "/../package.props").SingleOrDefault();

var solutionPropsDocument = XDocument.Load(propFile.FullPath);
var solutionPropsOriginalDocument = XDocument.Parse(solutionPropsDocument.ToString());
var solutionProps = solutionPropsDocument.Root
	.Elements(XName.Get("PropertyGroup", msbuildNamespace))
	.SelectMany(x => x.Elements())
	.ToDictionary(x => x.Name.LocalName, x => x);
	
var currentVersion = SemanticVersion.Parse(solutionProps["PackageVersion"].Value);
var keyFileLocation = string.IsNullOrWhiteSpace(codeSignKeypairPath) ? null : Environment.ExpandEnvironmentVariables(codeSignKeypairPath);

void SetVersion() {
	
	var versionBefore = currentVersion;
		
	if (!string.IsNullOrWhiteSpace(version)) {
		currentVersion = SemanticVersion.Parse(version);
	}
	
	if (!string.IsNullOrWhiteSpace(preRelease)) {
		currentVersion = currentVersion.DeclarePreRelease(preRelease);
	}
	
	if (versionBefore != currentVersion) {
		
		var vNext = string.IsNullOrWhiteSpace(preRelease) 
			? currentVersion.DeclareFinal().RaiseMinor() 
			: currentVersion.DeclareFinal();
		
		solutionProps["PackageVersion"].Value = currentVersion.ToString();
		solutionProps["PackageBaseVersion"].Value = vNext.ToString();
		
		solutionPropsDocument.Save(propFile.FullPath, SaveOptions.None);
	}
}

void RestoreVersion() {
	
	solutionPropsOriginalDocument.Save(propFile.FullPath, SaveOptions.None);
}

void BuildProjects(string targets) {
	
	foreach(var projectDir in projects) {
		
		if (projectDir.GetDirectoryName().StartsWith(".")) continue;
		
		var projectList = GetFiles(projectDir.FullPath + "/*.*proj");
		
		if (exceptionList != null) {
		
			foreach(var file in exceptionList){
			
				projectList -= file;
			}
		}
		
		foreach(var projectFile in projectList) {
		
			Information("Building " + System.IO.Path.GetFileNameWithoutExtension(projectFile.FullPath));
		
			var msb = new MSBuildSettings() {
				Configuration = configuration,
				ToolVersion = toolsVersion,
				Verbosity = Verbosity.Quiet
			};
			
			foreach(var target in targets.Split(',')) {
				msb.Targets.Add(target);
			}
			
			msb.ArgumentCustomization = a => {
				a.Append("/nologo");
				
				if (!string.IsNullOrWhiteSpace(keyFileLocation)) {
					a.Append("/p:SignAssembly=true");
					a.Append("/p:AssemblyOriginatorKeyFile=\"" + keyFileLocation + "\"");
				}
				
				return a;
			};
		
			MSBuild(projectFile.FullPath, msb);
		}
	}
}
	
Task("Clean")
	.Does(() => {
		
		CleanDirectory(wcRoot + Directory("out/" + scope));
		
		foreach(var projectDir in projects) {
			
			CleanDirectory(projectDir + Directory("/bin/" + configuration));
		}
	});
	
Task("Build")
	.Does(() => {
		
		SetVersion();
		BuildProjects("Restore,Build");
		
	})
	.Finally(() => RestoreVersion());
	
Task("Rebuild")
	.IsDependentOn("Clean")
	.Does(() => {
	
		SetVersion();
		BuildProjects("Restore,Rebuild");
	
	});
	
Task("Pack")
	.IsDependentOn("Rebuild")
	.Does(() => {
		
		var isPrivatePackage = privateBundles.Contains(scope.ToLowerInvariant());
		if (isPrivatePackage)
		{
			return;
		}
		
		CleanDirectory(wcRoot + Directory("out/" + scope));
		
		SetVersion();
		BuildProjects("Pack");
		
		EnsureDirectoryExists(wcRoot + Directory("out/" + scope));
		CopyFiles(GetFiles(scopeRoot.Path.FullPath + "/**/bin/" + configuration + "/*.nupkg"), wcRoot + Directory("out/" + scope));
		
	});
	
Task("Publish")
	.IsDependentOn("Pack")
	.Does(() => {

		var isChocoPackage = chocoFeedBundles.Contains(scope.ToLowerInvariant());
		var isPrivatePackage = privateBundles.Contains(scope.ToLowerInvariant());
		
		if (isPrivatePackage)
		{
			return;
		}
	
		foreach(var package in GetFiles((wcRoot + Directory("out")).Path.FullPath + "/" + scope + "/*.nupkg")) {
		
			Information("Pushing \"" + package + "\" to \"" + (isChocoPackage ? chocoFeed : nugetFeed) + "\"");
			NuGetPush(package, new NuGetPushSettings 
            { 
                Source = isChocoPackage ? chocoFeed : nugetFeed,
                ApiKey = isChocoPackage ? chocoApiKey : nugetApiKey, 
                Verbosity = NuGetVerbosity.Quiet
            });		

		}
		
	});

Task("Init").Does(() => { Information("##solutionName[" + solutionName + "]"); });
Task("Default").IsDependentOn("Build").Does(() => {});

RunTarget(target);