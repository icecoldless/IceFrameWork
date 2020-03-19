using Coroutine;
using Pool;
using Scene;
using System;
using System.Collections;
using UIFW;
using UnityEngine;
using XLuaFramework;

/// <summary>
/// 门面模式，中介者模式，单例
/// 职责：负责子系统之间的沟通
/// </summary>
public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 游戏不销毁根物体
    /// </summary>
    public Transform GameRoot;
    /// <summary>
    /// 游戏是否结束
    /// </summary>
    public bool IsGameOver;

    /// <summary>
    /// UISystem
    /// </summary>
    public UIManager UIManager;
    /// <summary>
    /// 场景管理器
    /// </summary>
    public SceneMgr SceneMgr;

    public GameManager()
    {
        IsGameOver = false;

        UIManager = new UIManager();
        SceneMgr = new SceneMgr();
    }
    /// <summary>
    /// 游戏初始化
    /// </summary>
    public void Initialize(Transform root)
    {
        //游戏根母体
        GameRoot = root;
        //资源初始化
        ResManager.Instance.Init();
        //UISystem
        UIManager.Initialize();
        //场景管理
        SceneMgr.Initialize();
    }

    /// <summary>
    /// 游戏逻辑更新
    /// </summary>
    public void Update()
    {
        UIManager.Update();
    }

    /// <summary>
    /// 游戏资源全释放：场景加载
    /// </summary>
    public void ReleaseAll()
    {
        //对象池资源的释放
        if (PoolManager.SharedInstance)
            PoolManager.SharedInstance.ReleaseAll();
        //UI资源全释放
        UIManager.ReleaseAll();
        //内存资源全释放
        ResManager.Instance.ReleaseAll();
        //强制垃圾回收
        System.GC.Collect();
    }

    #region ResManager
    public T LoadObject<T>(string resName) where T : UnityEngine.Object
    {
        return ResManager.Instance.LoadObject<T>(resName);
    }


    public void LoadObjectAsync<T>(string resName, Action<T> action) where T : UnityEngine.Object
    {
        ResManager.Instance.LoadObjectAsync<T>(resName, action);
    }


    public void LoadSceneAB(string sceneName)
    {
        ResManager.Instance.LoadSceneAB(sceneName);
    }

    public void UnloadAsset(string resName)
    {
        ResManager.Instance.UnloadAsset(resName);
    }
    #endregion

    #region Scene
    public void SetSceneState(ISceneState scene, bool isLoading = false)
    {
        SceneMgr.SetSceneState(scene, isLoading);
    }

    public void SetSceneState<T>(string sceneName, bool isABLoad, bool isWithLoading, bool isLoading = false) where T : ISceneState, new()
    {
        SceneMgr.SetSceneState<T>(sceneName, isABLoad, isWithLoading, isLoading);
    }
    #endregion

    #region UIFW
    public IUserInterface OpenUIFrm(string uiName)
    {
        return UIManager.OpenUIFrm(uiName);
    }

    public void CloseUIFrm(string uiName)
    {
        UIManager.CloseUIFrm(uiName);
    }

    public void ReleaseUIFrm(string uiName)
    {
        UIManager.ReleaseUIFrm(uiName);
    }
    #endregion

    #region Pool
    public IPoolableObject New(string prefabName)
    {
        return PoolManager.SharedInstance.New(prefabName);
    }

    public void RecycleObject(GameObject o)
    {
        PoolManager.SharedInstance.RecycleObject(o.GetComponent<IPoolableObject>());
    }

    public void ReleaseObject(GameObject o)
    {
        PoolManager.SharedInstance.ReleaseObject(o.GetComponent<IPoolableObject>());
    }
    #endregion

    #region Coroutine
    public CoroutineHolder Get(IEnumerator target)
    {
        return CoroutineManager.Instance.Get(target);
    }

    public void Run(IEnumerator target)
    {
        CoroutineManager.Instance.Run(target);
    }
    #endregion
}