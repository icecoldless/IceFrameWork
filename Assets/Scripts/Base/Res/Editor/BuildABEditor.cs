using System.IO;
using UnityEditor;

namespace XLuaFramework
{
    public static class BuildABEditor
    {
        static ABIni Ini = new ABIni();
        /// <summary>
        /// 自动设置AB包标签
        /// 同时生成对应的配置文件
        /// </summary>
        [MenuItem("XLuaFramework/自动标签")]
        static void AutoSetABLabel()
        {
            //移除无用标签
            AssetDatabase.RemoveUnusedAssetBundleNames();
            //自动设置标签
            AutoSetABLabel(PathUtil.AssetPath, 0);
            //生成配置文件
            File.WriteAllText(PathUtil.ABIniFilePath, EditorJsonUtility.ToJson(Ini));
            //清除记录
            Ini.Clear();
        }
        private static void AutoSetABLabel(string filePath, int index, string abPrefix = null, string abSuffix = null)
        {
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                string resName; //配置文件内资源的名称
                if (fi.Extension == ".meta")
                    return;
                filePath = filePath.Substring(filePath.LastIndexOf("Assets"));                //Assets/Download/InitScene/Materials/cubemat.mat;
                AssetImporter ai = AssetImporter.GetAtPath(filePath);
                if (fi.Extension == ".unity")
                {
                    ai.assetBundleName = abPrefix + "/" + abPrefix;
                    ai.assetBundleVariant = "u3d";
                    resName = fi.Name.Substring(0, fi.Name.LastIndexOf("."));
                }
                else
                {
                    ai.assetBundleName = abPrefix + "/" + abSuffix;
                    ai.assetBundleVariant = "ab";
                    resName = fi.Name;
                }
                //添加配置文件
                //添加一层过滤，只记录prefab ，png图片和场景的资源路径，目前我只会用到prefab和场景scene和png精灵
                if (resName.EndsWith(".prefab") || !resName.Contains(".") || resName.EndsWith(".png"))
                    Ini.AddData(resName, ai.assetBundleName + "." + ai.assetBundleVariant);
            }
            else if (Directory.Exists(filePath))
            {
                if (index == 1)
                {
                    abPrefix = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                }
                else if (index == 2)
                {
                    abSuffix = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                }

                DirectoryInfo di = new DirectoryInfo(filePath);
                FileSystemInfo[] fsInfos = di.GetFileSystemInfos();
                for (int i = 0; i < fsInfos.Length; i++)
                {
                    AutoSetABLabel(fsInfos[i].FullName, index + 1, abPrefix, abSuffix);
                }
            }
        }

        [MenuItem("XLuaFramework/PC平台打包")]
        static void BuildABOnPC()
        {
            BuildPipeline.BuildAssetBundles(PathUtil.BuildABPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("XLuaFramework/Android平台打包")]
        static void BuildABOnAndroid()
        {
            BuildPipeline.BuildAssetBundles(PathUtil.BuildABPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("XLuaFramework/IOS平台打包")]
        static void BuildABOnIOS()
        {
            BuildPipeline.BuildAssetBundles(PathUtil.BuildABPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("XLuaFramework/清空AB")]
        static void Clear()
        {
            if (Directory.Exists(PathUtil.BuildRootPath))
            {
                Directory.Delete(PathUtil.BuildRootPath, true);
                AssetDatabase.Refresh();
            }
        }
    }
}

