using System;
using System.Collections.Generic;
using Editor;
using UnityEditor;

/// <summary>
/// 打包任务
/// </summary>
[Serializable]
public sealed class BuildTask
{
    /// <summary>
    /// 名稱
    /// </summary>
    public string ProductName;
    
    /// <summary>
    /// 目標平台
    /// </summary>
    public BuildTarget BuildTarget;
    
    /// <summary>
    /// 打包路徑
    /// </summary>
    public string BuildPath;

    /// <summary>
    /// 是否為輸出 .unitypackage
    /// </summary>
    public bool IsExportPackage { get; set; }

    /// <summary>
    /// Package Type, ex: FCL, Blocto-unity-sdk
    /// </summary>
    public PackageTypeEnum PackageType { get; set; }

    /// <summary>
    /// 版號
    /// </summary>
    public string BuildVersion { get; set; }
    
    /// <summary>
    /// 打包場景
    /// </summary>
    public List<SceneAsset> SceneAssets = new List<SceneAsset>(0);
}