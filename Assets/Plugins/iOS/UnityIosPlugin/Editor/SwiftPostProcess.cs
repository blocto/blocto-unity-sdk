using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Plugins.iOS.UnityIosPlugin.Editor
{
    public static class SwiftPostProcess 
    {

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                var projPath = buildPath + "/Unity-Iphone.xcodeproj/project.pbxproj";
                var proj = new PBXProject();
                proj.ReadFromFile(projPath);

                var targetGuid = proj.TargetGuidByName(PBXProject.GetUnityTestTargetName());


                proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");



                proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/UnityIosPlugin/Source/UnityPlugin-Bridging-Header.h");
                proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "UnityFramework-Swift.h");

                proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
                proj.AddBuildProperty(targetGuid, "FRAMERWORK_SEARCH_PATHS", "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
                proj.AddBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
                proj.AddBuildProperty(targetGuid, "DYLIB_INSTALL_NAME_BASE", "@rpath");
                proj.AddBuildProperty(targetGuid, "LD_DYLIB_INSTALL_NAME", "@executable_path/../Frameworks/$(EXECUTABLE_PATH)");
                proj.AddBuildProperty(targetGuid, "DEFINES_MODULE", "YES");
                proj.AddBuildProperty(targetGuid, "SWIFT_VERSION", "4.0");
                proj.AddBuildProperty(targetGuid, "COREML_CODEGEN_LANGUAGE", "Swift");

                var plistPath = buildPath + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                var isContainQueriesSchemes = plist.root.values.ContainsKey("LSApplicationQueriesSchemes");
                var queriesUrlTypeArray = default(PlistElementArray);
                if(isContainQueriesSchemes)
                {
                    queriesUrlTypeArray = plist.root["LSApplicationQueriesSchemes"] as PlistElementArray;
                }
                else
                {
                    queriesUrlTypeArray = plist.root.CreateArray("LSApplicationQueriesSchemes");
                }
                
                
                if(queriesUrlTypeArray?.values.Any(p => p.AsString() == "blocto") == false)
                {
                    queriesUrlTypeArray?.AddString("blocto");
                }
                
                if(queriesUrlTypeArray?.values.Any(p => p.AsString() == "blocto-staging") == false)
                {
                    queriesUrlTypeArray?.AddString("blocto-staging");
                }
                
                OnProstProcessBuildIOS(buildPath);
                
                plist.WriteToFile(plistPath);
                proj.WriteToFile(projPath);
            }
        }
        
        private static void OnProstProcessBuildIOS(string pathToBuiltProject)
        {
            //This is the default path to the default pbxproj file. Yours might be different
            string projectPath = "/Unity-iPhone.xcodeproj/project.pbxproj";
            //Default target name. Yours might be different
            string targetName = "Unity-iPhone";
            //Set the entitlements file name to what you want but make sure it has this extension
            string entitlementsFileName = "my_app.entitlements";
            
            var entitlements = new ProjectCapabilityManager(pathToBuiltProject + projectPath, entitlementsFileName, targetName);
            entitlements.AddAssociatedDomains(new string[] { "applinks:fed9-114-36-191-176.jp.ngrok.io?mode=developer" });
            //Apply
            entitlements.WriteToFile();
            
            
            // string projectPath = PBXProject.GetPBXProjectPath(path);
            //
            // // iOS の ProjectCapabilityManagerに特定のパラメータをセット
            // var separator = Path.DirectorySeparatorChar;
            // string targetName = PBXProject.GetUnityTargetName();
            // var entitlementPath = projectPath + separator + targetName + separator + targetName + ".entitlements";
            // var entitlementFileName = Path.GetFileName(entitlementPath);
            // var unityTarget = PBXProject.GetUnityTargetName();
            // var relativeDestination = unityTarget + "/" + entitlementFileName;
            // var capabilityManager = new ProjectCapabilityManager(projectPath, relativeDestination, unityTarget);
            //
            // // Universal-link 対応
            // capabilityManager.AddAssociatedDomains(new string[] { "applinks:ここに短縮URL記入" });
            //
            // capabilityManager.WriteToFile();
        }

        // public int callbackOrder { get; }
        //
        // /// <summary>
        // /// エントリーポイント
        // /// </summary>
        // /// <param name="report">Report.</param>
        // public void OnPostprocessBuild(BuildReport report) {
        //     if (report.summary.platform == BuildTarget.iOS) {
        //         iOSPostProcess(report.summary.outputPath);
        //     }
        // }
        //
        // /// <summary>
        // /// iOS個別処理
        // /// </summary>
        // /// <param name="path">Path.</param>
        // private void iOSPostProcess(string path) {
        //     string projectPath = PBXProject.GetPBXProjectPath(path);
        //
        //     // iOS の ProjectCapabilityManagerに特定のパラメータをセット
        //     var separator = Path.DirectorySeparatorChar;
        //     string targetName = PBXProject.GetUnityTargetName();
        //     var entitlementPath = projectPath + separator + targetName + separator + targetName + ".entitlements";
        //     var entitlementFileName = Path.GetFileName(entitlementPath);
        //     var unityTarget = PBXProject.GetUnityTargetName();
        //     var relativeDestination = unityTarget + "/" + entitlementFileName;
        //     var capabilityManager = new ProjectCapabilityManager(projectPath, relativeDestination, unityTarget);
        //
        //     // Universal-link 対応
        //     capabilityManager.AddAssociatedDomains(new string[] { "applinks:ここに短縮URL記入" });
        //
        //     capabilityManager.WriteToFile();
        // }
    }
}