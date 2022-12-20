using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Editor;
using Flow.Net.SDK.Extensions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[CustomEditor(typeof(BuildProfile))]
public sealed class BuildProfileInspector : UnityEditor.Editor
{
    private readonly Dictionary<BuildTask, bool> foldoutMap = new Dictionary<BuildTask, bool>();
    private Vector2 scroll = Vector2.zero;
    private BuildProfile profile;
    private bool boolValue;
    private int selGridInt = 0;
    private string[] selStrings = { "FCL", "Blocto-unity-SDK", "Portto.Blocto.Core", "Portto.Blocto.Solana" };

    private void OnEnable() { profile = target as BuildProfile; }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        {
            NewTaskLayout();
            TaskListLayout();

            GUI.color = Color.yellow;
            ClearTaskListLayoutWithHandler();

            GUI.color = Color.cyan;
            if (GUILayout.Button("打包", "ButtonRight"))
            {
                try
                {
                    TaskHandler();
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            GUI.color = Color.white;
        }

        GUILayout.EndHorizontal();
        scroll = GUILayout.BeginScrollView(scroll);
        {
            foreach (var task in profile.BuildTasks)
            {
                if (!foldoutMap.ContainsKey(task))
                    foldoutMap.Add(task, true);

                if (DeleteTaskLayout(task))
                {
                    break;
                }

                if (foldoutMap[task])
                {
                    GUILayout.BeginVertical("Box");
                    IsExportPackageLayout(task);
                    GUILayout.EndHorizontal();

                    if (task.IsExportPackage == false)
                    {
                        CreateBuildLayout(task);
                    }

                    BuildVersionLayout(task);
                    GUILayout.EndVertical();
                }
            }
        }

        GUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            EditorUtility.SetDirty(profile);
    }

    private void TaskListLayout()
    {
        if (GUILayout.Button("展開", "ButtonMid"))
        {
            foreach (var t in profile.BuildTasks)
            {
                foldoutMap[t] = true;
            }
        }

        if (GUILayout.Button("收缩", "ButtonMid"))
        {
            foreach (var t in profile.BuildTasks)
            {
                foldoutMap[t] = false;
            }
        }
    }

    private void NewTaskLayout()
    {
        if (GUILayout.Button("新建", "ButtonLeft"))
        {
            Undo.RecordObject(profile, "Create");
            var task = new BuildTask()
                       {
                           ProductName = "Product Name",
                           BuildTarget = BuildTarget.StandaloneWindows64,
                           BuildPath = Directory.GetParent(Application.dataPath).FullName
                       };

            profile.BuildTasks.Add(task);
        }
    }

    private void ClearTaskListLayoutWithHandler()
    {
        if (GUILayout.Button("清空", "ButtonMid"))
        {
            Undo.RecordObject(profile, "Clear");
            if (EditorUtility.DisplayDialog("提醒", "是否確定清空列表?", "確定", "取消"))
            {
                profile.BuildTasks.Clear();
            }
        }
    }

    private void TaskHandler()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("打包報告:\r\n");
        for (int i = 0; i < profile.BuildTasks.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Build", "Building...", i + 1 / profile.BuildTasks.Count);
            var task = profile.BuildTasks[i];
            var report = TaskExecuteor(task);
        }

        EditorUtility.ClearProgressBar();
    }

    private BuildReport TaskExecuteor(BuildTask task)
    {
        var report = default(BuildReport);
        if(task.IsExportPackage)
        {
            var directories = new List<string>();
            
            var timeVersion = $"{DateTime.UtcNow.DayOfYear}{(DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute}";
            var version = $"{task.BuildVersion}.{timeVersion}";
            
            switch (task.PackageType)
            {
                case PackageTypeEnum.FCL:
                    var fclAssetsPaths = new List<string>
                                         {
                                             "Assets/Plugins/Flow/FCL",
                                             "Assets/Plugins/Flow/Flow.Net.Sdk",
                                             "Assets/Plugins/Flow/Flow.Net.Sdk.Client.Unity",
                                             "Assets/Plugins/Flow/Flow.Net.Sdk.Core",
                                             "Assets/Plugins/Flow/Flow.Net.Sdk.Utility",
                                             "Assets/Plugins/Dll",
                                             "Assets/Plugins/System.ComponentModel.Annotations.dll"
                                         };
                    AssetDatabase.ExportPackage(fclAssetsPaths.ToArray(), $"release/fcl-unity/FCL.{task.BuildVersion}.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.Default);
                    Debug.Log("FCL export successed.");
                    break;
                case PackageTypeEnum.BloctoUnitySDK:
                    var bloctoSdkDirInfo = new DirectoryInfo($"{Application.dataPath}/Plugins/Blocto.Sdk");
                    var bloctoSdkDirPaths = bloctoSdkDirInfo.GetDirectories().Select(p => {
                                                                                         var tmp = p.FullName.Split("Assets/")[1];
                                                                                         return $"Assets/{tmp}";
                                                                                     }).ToList();
                    directories.AddRange(bloctoSdkDirPaths);
                    directories.Add("Assets/Plugins/Dll");
                    directories.Add($"Assets/Plugins/Android");
                    directories.Add($"Assets/Plugins/iOS/UnityIosPlugin");
                    directories.Add($"Assets/Plugins/System.ComponentModel.Annotations.dll");
                    AssetDatabase.ExportPackage(directories.ToArray(), $"release/blocto-unity-sdk/Blocto-unity-sdk.{version}.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.Default);
                    Debug.Log("Blocto-unity-SDK export successed.");
                    break;
                case PackageTypeEnum.Core:
                    var coreOutputPath = $"release/{task.BuildVersion}";
                    if(Directory.Exists(coreOutputPath) == false)
                    {
                        Directory.CreateDirectory(coreOutputPath);
                    }
                    
                    var coreDirInfo = new DirectoryInfo($"{Application.dataPath}/Plugins/Blocto.Sdk/Core");
                    var coreDirPaths = coreDirInfo.GetDirectories().Select(p => {
                                                                               var tmp = p.FullName.Split("Assets/")[1];
                                                                               return $"Assets/{tmp}";
                                                                           }).ToList(); 
                    directories.AddRange(coreDirPaths);
                    directories.Add("Assets/Plugins/Dll");
                    directories.Add($"Assets/Plugins/Android");
                    directories.Add($"Assets/Plugins/iOS/UnityIosPlugin");
                    AssetDatabase.ExportPackage(directories.ToArray(), $"release/{task.BuildVersion}/Portto.Blocto.Core.{version}.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.Default);
                    Debug.Log("Portto.Blocto.Core export successed.");
                    break;
                case PackageTypeEnum.Solana:
                    var solanaOutputPath = $"release/{task.BuildVersion}";
                    if(Directory.Exists(solanaOutputPath) == false)
                    {
                        Directory.CreateDirectory(solanaOutputPath);
                    }

                    var solanaDirInfo = new DirectoryInfo($"{Application.dataPath}/Plugins/Blocto.Sdk/Solana");
                    var solanaDirPaths = solanaDirInfo.GetDirectories().Select(p => {
                                                                               var tmp = p.FullName.Split("Assets/")[1];
                                                                               return $"Assets/{tmp}";
                                                                           }).ToList(); 
                    directories.AddRange(solanaDirPaths);
                    AssetDatabase.ExportPackage(directories.ToArray(), $"release/{task.BuildVersion}/Portto.Blocto.Solana.{version}.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.Default);
                    Debug.Log("Protto.Blocto.Solana export successed.");
                    break;
            }
        }
        else
        {
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (var t in task.SceneAssets)
            {
                var scenePath = AssetDatabase.GetAssetPath(t);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            var locationPathName = $"{task.BuildPath}/{task.ProductName}";
            report = BuildPipeline.BuildPlayer(buildScenes.ToArray(), locationPathName, task.BuildTarget, BuildOptions.None);
            Debug.Log($"[{task.ProductName}] 打包结果: {report.summary.result}\r\n");

        }
        
        return report;
    }

    private void BuildVersionLayout(BuildTask task)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("版號：", GUILayout.Width(70));
        task.BuildVersion = GUILayout.TextField(task.BuildVersion);

        GUILayout.EndHorizontal();
    }

    private void IsExportPackageLayout(BuildTask task)
    {
        GUILayout.BeginHorizontal();
        var newValue = GUILayout.Toggle(task.IsExportPackage, "Is Export Package");
        if (newValue != task.IsExportPackage)
        {
            task.IsExportPackage = newValue;
        }
        
        if(task.IsExportPackage)
        {
            var newIndex = GUILayout.SelectionGrid((int) task.PackageType, selStrings, 1);
            if((int)task.PackageType != newIndex)
            {
                task.PackageType = (PackageTypeEnum) newIndex;    
            }
        }
    }

    private bool DeleteTaskLayout(BuildTask task)
    {
        GUILayout.BeginHorizontal("Badge");
        GUILayout.Space(12);
        foldoutMap[task] = EditorGUILayout.Foldout(foldoutMap[task], $"{task.ProductName}", true);
        GUILayout.Label(string.Empty);
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), "IconButton", GUILayout.Width(20)))
        {
            Undo.RecordObject(profile, "Delete Task");
            foldoutMap.Remove(task);
            profile.BuildTasks.Remove(task);
            return true;
        }

        GUILayout.EndHorizontal();
        return false;
    }

    private void CreateBuildLayout(BuildTask task)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("打包場景：", GUILayout.Width(70));
        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), GUILayout.Width(28)))
        {
            task.SceneAssets.Add(null);
        }

        GUILayout.EndHorizontal();
        if (task.SceneAssets.Count > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(75);
            GUILayout.BeginVertical("Badge");
            for (int j = 0; j < task.SceneAssets.Count; j++)
            {
                var sceneAsset = task.SceneAssets[j];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{j + 1}.", GUILayout.Width(20));
                task.SceneAssets[j] = EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (GUILayout.Button("↑", "MiniButtonLeft", GUILayout.Width(20)))
                {
                    if (j > 0)
                    {
                        Undo.RecordObject(profile, "Move Up Scene Assets");
                        var temp = task.SceneAssets[j - 1];
                        task.SceneAssets[j - 1] = sceneAsset;
                        task.SceneAssets[j] = temp;
                    }
                }

                if (GUILayout.Button("↓", "MiniButtonMid", GUILayout.Width(20)))
                {
                    if (j < task.SceneAssets.Count - 1)
                    {
                        Undo.RecordObject(profile, "Move Down Scene Assets");
                        var temp = task.SceneAssets[j + 1];
                        task.SceneAssets[j + 1] = sceneAsset;
                        task.SceneAssets[j] = temp;
                    }
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), "MiniButtonMid", GUILayout.Width(20)))
                {
                    Undo.RecordObject(profile, "Add Scene Assets");
                    task.SceneAssets.Insert(j + 1, null);
                    break;
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), "MiniButtonMid", GUILayout.Width(20)))
                {
                    Undo.RecordObject(profile, "Delete Scene Assets");
                    task.SceneAssets.RemoveAt(j);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("產品名稱：", GUILayout.Width(70));
        var newPN = GUILayout.TextField(task.ProductName);
        if (task.ProductName != newPN)
        {
            Undo.RecordObject(profile, "Product Name");
            task.ProductName = newPN;
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("打包平台：", GUILayout.Width(70));
        var newBT = (BuildTarget)EditorGUILayout.EnumPopup(task.BuildTarget);
        if (task.BuildTarget != newBT)
        {
            Undo.RecordObject(profile, "Build Target");
            task.BuildTarget = newBT;
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("打包路徑：", GUILayout.Width(70));
        GUILayout.TextField(task.BuildPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            task.BuildPath = EditorUtility.SaveFolderPanel("Build Path", task.BuildPath, "");
        }

        GUILayout.EndHorizontal();
    }
}