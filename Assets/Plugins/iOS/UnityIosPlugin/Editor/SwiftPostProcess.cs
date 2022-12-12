using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

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

                // string targetName = "Unity-iPhone";
                var targetName = proj.GetUnityFrameworkTargetGuid(); 
                // var targetGuid = proj.TargetGuidByName();
                var targetGuid = proj.GetUnityMainTargetGuid();


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
                    queriesUrlTypeArray.AddString("blocto");
                }
                
                if(queriesUrlTypeArray?.values.Any(p => p.AsString() == "blocto-staging") == false)
                {
                    queriesUrlTypeArray.AddString("blocto-staging");
                }
                
                if(queriesUrlTypeArray?.values.Any(p => p.AsString() == "blocto-dev") == false)
                {
                    queriesUrlTypeArray.AddString("blocto-dev");
                }
                
                var fileName = OnProstProcessBuildIOS(projPath);
                proj.SetBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", fileName);
                
                plist.WriteToFile(plistPath);
                proj.WriteToFile(projPath);
            }
        }
        
        private static string OnProstProcessBuildIOS(string pathToBuiltProject)
        {
            //This is the default path to the default pbxproj file. Yours might be different
            string projectPath = "/Unity-iPhone.xcodeproj/project.pbxproj";
            //Default target name. Yours might be different
            string targetName = "Unity-iPhone";
            //Set the entitlements file name to what you want but make sure it has this extension
            string entitlementsFileName = "Unity-iPhoneReleaseForRunning.entitlements";
            
            
            // // iOS の ProjectCapabilityManagerに特定のパラメータをセット
            var separator = Path.DirectorySeparatorChar;
            var entitlementPath = default(string);
            if(Debug.isDebugBuild)
            {
                entitlementPath = pathToBuiltProject + separator + targetName + separator + targetName + "Debug.entitlements";
            }
            
            var entitlementFileName = Path.GetFileName(entitlementPath);
            var relativeDestination = targetName + "/" + entitlementFileName;
            var capabilityManager = new ProjectCapabilityManager(pathToBuiltProject, relativeDestination, targetName);
            
            // Universal-link 対応
            // capabilityManager.AddAssociatedDomains(new string[] { "applinks:aabb-61-216-44-25.jp.ngrok.io?mode=developer" });
            
            capabilityManager.WriteToFile();
            return relativeDestination;
        }
    }
}