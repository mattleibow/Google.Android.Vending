using System.Xml;
using System.Xml.Linq;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var RepositoryUrlRoot = "https://dl.google.com/android/repository/";
var RepositoryUrl = RepositoryUrlRoot + "addon.xml";
var RepositoryNS = (XNamespace)"http://schemas.android.com/sdk/android/addon/7";
var LicensingKey = "market_licensing";
var LicensingVersion = "r02";
var ExpansionKey = "market_apk_expansion";
var ExpansionVersion = "r03";

Task("Externals")
    .Does(() =>
{
    EnsureDirectoryExists("./externals/");

    // // download the repository directory
    // if (!FileExists("./externals/addon.xml")) {
    //     DownloadFile(RepositoryUrl, "./externals/addon.xml");
    // }

    // // download the Java code
    // var xdoc = XDocument.Load("./externals/addon.xml");
    // foreach (var extra in xdoc.Root.Elements(RepositoryNS + "extra")) {
    //     var path = extra.Element(RepositoryNS + "path");
    //     if (new [] { LicensingKey, ExpansionKey }.Contains(path.Value)) {
    //         var dest = "./externals/" + path.Value + ".zip";
    //         if (!FileExists(dest)) {
    //             var archive = extra
    //                 .Element(RepositoryNS + "archives")
    //                 .Element(RepositoryNS + "archive");
    //             var url = archive.Element(RepositoryNS + "url").Value;
    //             var size = archive.Element(RepositoryNS + "size").Value;

    //             DownloadFile(RepositoryUrlRoot + url, dest);
    //             Unzip(dest, "./externals/");
    //         }
    //     }
    // }

    // download the Java code
    var download = new Action<string, string>((key, version) => {
        var dest = "./externals/" + key + ".zip";
        if (!FileExists(dest)) {
            DownloadFile(RepositoryUrlRoot + string.Format("{0}-{1}.zip", key, version), dest);
            Unzip(dest, "./externals/");
        }
    });
    download(ExpansionKey, ExpansionVersion);
    download(LicensingKey, LicensingVersion);

    // Build the Java projects
    var result = StartProcess(IsRunningOnWindows() ? "cmd" : "sh", new ProcessSettings {
        Arguments = (IsRunningOnWindows() ? "/c gradlew" : "gradlew") + " bundleRelease",
        WorkingDirectory = "native"
    });
    if (result != 0) {
        throw new Exception("gradlew returned " + result);
    }
});

Task("Build")
    .IsDependentOn("Externals")
    .Does(() =>
{
    DotNetBuild("./source/Google.Android.Vending.sln");
});

Task("Clean")
    .Does(() =>
{
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);

