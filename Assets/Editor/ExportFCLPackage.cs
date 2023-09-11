using UnityEditor;

namespace Editor
{
    public static class ExportFCLPackage
    {
        const string kPackageName = "SymbolCatalog.unitypackage";
        static readonly string[] kAssetPathes = {
                                                    // "Assets/Mobcast/Coffee/Editor/SymbolCatalog",
                                                };

        [MenuItem ("Export Package/" + kPackageName)]
        [InitializeOnLoadMethod]
        static void Export ()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
			
            
            // AssetDatabase.ExportPackage (kAssetPathes, kPackageName, ExportPackageOptions.Recurse | ExportPackageOptions.Default);
            // var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            // UnityEngine.Debug.Log ("Export successfully : " + kPackageName);
        }
    }}