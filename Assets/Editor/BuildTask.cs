using System;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 打包任务
/// </summary>
[Serializable]
public sealed class BuildTask
{
    /// <summary>
    /// 名称
    /// </summary>
    public string ProductName;
    /// <summary>
    /// 目标平台
    /// </summary>
    public BuildTarget BuildTarget;
    /// <summary>
    /// 打包路径
    /// </summary>
    public string BuildPath;
    /// <summary>
    /// 打包场景
    /// </summary>
    public List<SceneAsset> SceneAssets = new List<SceneAsset>(0);
}