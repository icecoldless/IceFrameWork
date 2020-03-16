using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 资源管理类
/// </summary>

namespace XLuaFramework
{
    public class ResManager : SingletonMono<ResManager>
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public static ResMode NowResMode = ResMode.LocalAB;       //当前的资源加载模式

        #region 引用计数相关
        //已经加载了的AB包的列表
        private Dictionary<string, AssetBundleInfo> m_LoadAssetBundle = new Dictionary<string, AssetBundleInfo>();
        #endregion

        #region 加载管理
        /// <summary>
        /// 加载中的AB包相关信息类
        /// </summary>
        public class LoadingInfo
        {
            public bool m_UseCallBack;                      //加载完成后是否还需要调用回调函数
            public Action<AssetBundle> m_CallBack;          //回调函数

            public LoadingInfo(Action<AssetBundle> action)
            {
                m_CallBack = action;
                m_UseCallBack = true;
            }
        }

        //正在加载的资源队列
        private Dictionary<string, LoadingInfo> m_nowLodingList = new Dictionary<string, LoadingInfo>();
        #endregion

        //总的Manifest，用于查询依赖
        private AssetBundleManifest m_AssetBundleManifest;  //这个清单文件类就是一个缓存，不需要自己额外申请空间把文件类中的信息单独提取出来缓存

        //从文件当中读取，资源名称-ab包路径/资源名称-资源路径
        private Dictionary<string, string> _ResPath = new Dictionary<string, string>();

        #region 初始化
        public void Init(Action initOK = null)
        {
            //加载AssetBundleManifest文件
            var manifestName = PathUtil.ABRootPath.Trim('/');//移除结尾的/获取名字,AB.ab
            AssetBundle manifestBundle = LoadAssetsFromAB(manifestName);
            if (manifestBundle != null)
            {
                m_AssetBundleManifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                //释放包
                manifestBundle.Unload(false);
                manifestBundle = null;
                m_LoadAssetBundle.Remove(manifestName.ToLower());      //从加载队列中移除
            }
            else
            {
                Debug.LogError("[ResManger] manifestBundle is null!");
            }

            //加载Res路径
            //初始化AB资源集合
            ABIni ini = JsonUtility.FromJson<ABIni>(File.ReadAllText(PathUtil.ABIniFilePath));
            for (int i = 0; i < ini.Datas.Count; i++)
            {
                _ResPath.Add(ini.Datas[i].ResName, ini.Datas[i].ABName);
            }
            if (initOK != null) initOK.Invoke();
        }
        #endregion

        #region 资源加载  对外提供
        #region LoadObject
        /// <summary>
        /// 同步加载的资源,传入泛型为资源类型
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetName">资源名称,Cube01.prefab</param>
        /// <returns></returns>
        public T LoadObject<T>(string assetName) where T : UnityEngine.Object
        {
            T reObj = null;
            //处于AB包下的资源
            if (_ResPath.ContainsKey(assetName))
            {
                var reAB = LoadAssetsFromAB(_ResPath[assetName]);             //传入的是ab包名
                reObj = reAB.LoadAsset<T>(assetName);
            }
            else
            {
                //通过本地Resources加载
                reObj = Resources.Load<T>(assetName);
            }
            return reObj;
        }

        /// <summary>
        /// 异步加载的资源,传入泛型为资源类型
        /// 异步加载才需要回调
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetName">资源名称,Cube01.prefab</param>
        /// <param name="action">ab加载完成后的回调函数，参数为加载完的ab包</param>
        /// <returns></returns>
        public void LoadObjectAsync<T>(string assetName, Action<T> action) where T : UnityEngine.Object
        {
            StartCoroutine(OnLoadObjectAsync(assetName, action));
        }
        IEnumerator OnLoadObjectAsync<T>(string assetName, Action<T> action) where T : UnityEngine.Object
        {
            if (!_ResPath.ContainsKey(assetName))
            {
                //资源在Resources中
                var request = Resources.LoadAsync<T>(assetName);
                yield return request;
                if (
                    request != null
                    &&
                    request.isDone
                    )
                {
                    var reObj = request.asset as T;
                    if (action != null) action.Invoke(reObj);
                }
            }
            else
            {
                //传入的是ab包名
                yield return StartCoroutine(OnLoadAssetsFromABAsync(_ResPath[assetName], delegate (AssetBundle reAB)
                {
                    StartCoroutine(OnLoadObj<T>(reAB, assetName, action));
                }));
            }
        }
        IEnumerator OnLoadObj<T>(AssetBundle reAB, string assetName, Action<T> action) where T : UnityEngine.Object
        {
            var request = reAB.LoadAssetAsync<T>(assetName);
            yield return request;
            var reObj = request.asset as T;

            if (action != null) action.Invoke(reObj);
        }
        #endregion

        #region LoadScene
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="abName">ab包全名,如Assets/_Project/Scene/scene.unity3d</param>
        /// <param name="sceneName">资源全名,如Assets/_Project/Scene/Home.unity</param>
        public void LoadSceneAB(string sceneName)
        {
            LoadAssetsFromAB(_ResPath[sceneName]);             //传入的是ab包名
        }
        #endregion

        #region Assets加载  对内提供
        /// <summary>
        /// AB同步方式加载资源
        /// </summary>
        /// <param name="abName">ab包全名,如Assets/_Project/Prefab/prefab.u3d</param>
        /// 场景ab包后缀是.u3d，其余的都是.ab
        /// <returns></returns>
        private AssetBundle LoadAssetsFromAB(string abName)
        {
            abName = abName.ToLower();

            //查找AB包本体
            AssetBundleInfo assetBundleInfo = null;
            if (m_LoadAssetBundle.TryGetValue(abName, out assetBundleInfo))     //检查是否已加载
            {
                assetBundleInfo.m_ReferencedCount++;        //引用计数+1
            }
            else
            {
                var loadPath = PathUtil.DataPath + PathUtil.ABRootPath + abName;
                var reAB = AssetBundle.LoadFromFile(loadPath);
                if (reAB == null)
                {
                    Debug.LogError("[ResManager] AB包：" + abName + "，不存在:" + loadPath);
                }
                else
                {
                    assetBundleInfo = new AssetBundleInfo(reAB);
                    m_LoadAssetBundle.Add(abName, assetBundleInfo);         //加入已加载列表
                }
            }

            //加载依赖包
            if (m_AssetBundleManifest != null)
            {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                foreach (var dep in dependencies)
                {
                    LoadAssetsFromAB(dep);
                }
            }

            return assetBundleInfo.m_AssetBundle;
        }

        /// <summary>
        /// AB异步方式加载资源
        /// </summary>
        /// <param name="abName">ab包全名,如Assets/_Project/Prefab/prefab.u3d</param>
        /// <param name="action">ab加载完成后的回调函数，参数为加载完的ab包</param>
        /// <returns></returns>
        private void LoadAssetsFromABAsync(string abName, Action<AssetBundle> action)
        {
            StartCoroutine(OnLoadAssetsFromABAsync(abName, action));
        }
        IEnumerator OnLoadAssetsFromABAsync(string abName, Action<AssetBundle> action)
        {
            abName = abName.ToLower();

            //加载AB包本体
            AssetBundleInfo assetBundleInfo = null;
            if (m_LoadAssetBundle.TryGetValue(abName, out assetBundleInfo))     //检查是否已加载
            {
                assetBundleInfo.m_ReferencedCount++;        //引用计数+1

                if (action != null) action.Invoke(assetBundleInfo.m_AssetBundle);

                //依赖计数+1
                if (m_AssetBundleManifest != null)
                {
                    string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                    foreach (var dep in dependencies)
                    {
                        yield return StartCoroutine(OnLoadAssetsFromABAsync(dep, null));
                    }
                }

                yield break;
            }
            else
            {
                if (m_nowLodingList.ContainsKey(abName))     //该资源正在被加载
                {
                    //加入多播委托列表
                    if (action != null) m_nowLodingList[abName].m_CallBack += action;

                    //以委托的形式加入回调方法，在资源完成加载后增加引用计数
                    m_nowLodingList[abName].m_CallBack += delegate
                    {
                        //增加引用计数
                        m_LoadAssetBundle[abName].m_ReferencedCount++;
                        //依赖计数+1
                        string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                        foreach (var dep in dependencies)
                        {
                            m_LoadAssetBundle[dep].m_ReferencedCount++;
                        }
                    };

                    Debug.Log("[ResManager] AB包：" + abName + "，正在被加载！");
                    yield break;
                }
                else
                {
                    //加入加载列表
                    if (action != null) m_nowLodingList.Add(abName, new LoadingInfo(action));

                    //异步加载资源
                    var loadPath = PathUtil.DataPath + PathUtil.ABRootPath + abName;
                    var abRequest = AssetBundle.LoadFromFileAsync(loadPath);
                    yield return abRequest;
                    var reAB = abRequest.assetBundle;
                    if (reAB == null)
                    {
                        Debug.LogError("[ResManager] AB包：" + abName + "，不存在:" + loadPath);
                    }
                    else
                    {
                        assetBundleInfo = new AssetBundleInfo(reAB);
                        m_LoadAssetBundle.Add(abName, assetBundleInfo);         //加入已加载列表
                    }

                    //加载依赖包
                    if (m_AssetBundleManifest != null)
                    {
                        string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                        foreach (var dep in dependencies)
                        {
                            yield return StartCoroutine(OnLoadAssetsFromABAsync(dep, null));
                        }
                    }

                    if (m_nowLodingList.ContainsKey(abName) && m_nowLodingList[abName].m_UseCallBack)
                    {
                        var actionList = m_nowLodingList[abName].m_CallBack.GetInvocationList();        //获取委托列表
                        m_nowLodingList.Remove(abName);         //记得一定要从队列中移除
                        //最后完成委托调用!
                        foreach (Action<AssetBundle> a in actionList)
                        {
                            if (a != null) a.Invoke(assetBundleInfo.m_AssetBundle);
                        }
                    }
                }
            }
        }
        #endregion

        #region 中断加载
        /// <summary>
        /// 中断当前所有正在加载的程序
        /// </summary>
        private void CutAllLoadingAssets()
        {
            foreach (var loadingInfo in m_nowLodingList)
            {
                loadingInfo.Value.m_UseCallBack = false;
            }
        }
        #endregion

        #region 资源卸载  对外提供
        public void UnloadAsset(string assetName, bool isThorough = true)
        {
            if (!_ResPath.ContainsKey(assetName))
                return;
            UnLoadAssetBundle(_ResPath[assetName], isThorough);
        }
        /// <summary>
        /// 根据abName卸载AB包
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnLoadAssetBundle(string abName, bool isThorough = true)
        {
            abName = abName.ToLower();

            if (m_LoadAssetBundle.ContainsKey(abName))
            {
                //卸载本体
                if (--m_LoadAssetBundle[abName].m_ReferencedCount <= 0)
                {
                    m_LoadAssetBundle[abName].m_AssetBundle.Unload(isThorough);
                    m_LoadAssetBundle.Remove(abName);
                }

                //递归卸载依赖
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                foreach (var dep in dependencies)
                {
                    UnLoadAssetBundle(dep, isThorough);
                }
            }
            else
            {
                Debug.Log("[ResManager] 要卸载的资源：" + abName + " 不存在！");
            }
        }
        /// <summary>
        /// 卸载全部资源
        /// </summary>
        public void ReleaseAll()
        {
            CutAllLoadingAssets();
            foreach (var kv in m_LoadAssetBundle)
            {
                kv.Value.m_AssetBundle.Unload(true);
            }
            m_LoadAssetBundle.Clear();
            m_nowLodingList.Clear();
            //强制垃圾回收
            Resources.UnloadUnusedAssets();
        }
        #endregion
        #endregion
    }
}
