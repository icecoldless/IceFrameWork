using UnityEngine;

namespace XLuaFramework
{
    /// <summary>
    /// AB包相关信息类
    /// </summary>
    public class AssetBundleInfo
    {
        public AssetBundle m_AssetBundle;                   //AB包引用
        public int m_ReferencedCount;                       //引用计数

        public AssetBundleInfo(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }
}
