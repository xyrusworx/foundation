var solutionName = "XyrusWorx.Foundation";

var nugetFeed    = "https://api.nuget.org/v3/index.json";
var nugetApiKey  = (string)null;

var chocoFeed    = "https://api.nuget.org/v3/index.json";
var chocoApiKey  = (string)null;

var chocoFeedBundles = new string[]{ "app", "tools" };
var privateBundles   = new string[]{ "test", "workbench" };

var toolsVersion = MSBuildToolVersion.VS2019;
var codeSignKeypairPath = "%XW_KEYFILE_LOCATION%\\XyrusWorx\\keypair.snk";

var exceptionList = GetFiles(rootDirectory + "/src/template/**/*.csproj");
