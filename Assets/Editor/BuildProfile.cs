using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 打包配置表
/// </summary>
[CreateAssetMenu(fileName = "New Build Profile", menuName = "Build Profile")]
public sealed class BuildProfile : ScriptableObject
{
    /// <summary>
    /// 打包任务列表
    /// </summary>
    public List<BuildTask> BuildTasks = new List<BuildTask>(0);
}