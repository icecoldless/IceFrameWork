using System.IO;
using UnityEngine;

/// <summary>
/// 路径管理类
/// </summary>
namespace XLuaFramework
{
    public class PathUtil
    {
        public const string AssetPath = "Assets/Artist/";                                      //资源的存放目录
        public static string BuildRootPath = Application.streamingAssetsPath + "/";            //打包的根路径，在打包ab包和使用模式2的时候会用到
        public const string ABRootPath = "AB/";                                                //AB包打包的根路径

        /// <summary>
        /// 打包的AB路径
        /// </summary>
        public static string BuildABPath
        {
            get
            {
                string path = DataPath + ABRootPath;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return DataPath + ABRootPath;
            }
        }

        //配置文件路径
        public static string ABIniFilePath
        {
            get
            {
                string filePath = BuildABPath + "ABIni.json";
                if (!File.Exists(filePath))
                {
                    FileStream fs = File.Create(filePath);
                    fs.Close();
                }
                return filePath;
            }
        }

        /// <summary>
        /// 数据盘的根目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                switch (ResManager.NowResMode)
                {
                    case ResMode.LocalAB:
                        if (Application.isMobilePlatform)
                        {
                            return Application.persistentDataPath + Application.productName + "/";
                        }
                        else
                        {
                            return BuildRootPath;
                        }
                    case ResMode.Online:
                        if (Application.isMobilePlatform)
                        {
                            return Application.persistentDataPath + Application.productName + "/";
                        }
                        else
                        {
                            return "c://" + Application.productName + "/";
                        }

                    default:
                        return null;
                }
            }
        }
    }
}
