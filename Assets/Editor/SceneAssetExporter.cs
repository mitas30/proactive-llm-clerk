using UnityEditor;
using UnityEngine;

public class SceneAssetExporter
{
    [MenuItem("Tools/Export Scene Assets")]
    public static void ExportSceneAssets()
    {
        // エクスポート対象のシーンファイルのパス
        string scenePath = "Assets/_Scenes/TITLE.unity";

        // シーンに依存しているすべてのアセットを収集する
        string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);

        // パッケージとしてエクスポートする
        string exportPath = "TitleScene.unitypackage";
        AssetDatabase.ExportPackage(dependencies, exportPath, ExportPackageOptions.Interactive);

        Debug.Log("Exported package to: " + exportPath);
    }
}
