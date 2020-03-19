using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    /// <summary>
    /// 场景管理器
    /// 状态设计模式，因为很多子系统都有可能会设计到场景跳转，例如UI的按钮点击
    /// </summary>
    public class SceneMgr
    {
        /// <summary>
        /// 特殊Loading场景
        /// </summary>
        private LoadingSceneState _LoadingScene;
        /// <summary>
        /// 当前场景状态
        /// </summary>
        private ISceneState _CurScene;
        /// <summary>
        /// 异步操作数
        /// 查看当前场景是否已经加载完成
        /// </summary>
        public AsyncOperation LoadingProcess;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            _LoadingScene = CreateSceneState<LoadingSceneState>("LoadingScene", false, false) as LoadingSceneState;
            _CurScene = CreateSceneState<InitSceneState>("InitScene", false, false);
            _CurScene.IsBegin = true;
            _CurScene.SceneBegin();
        }
        /// <summary>
        /// 输入一个场景的名称，
        /// 先跳转到Loading场景在跳转进目标场景中
        /// </summary>
        /// <param name="sceneName"></param>
        private void LoadSceneWithLoading(ISceneState scene)
        {
            _LoadingScene.SetNextScene(scene);
            //同步加载到Loading场景
            LoadScene(_LoadingScene);
        }

        /// <summary>
        /// 不通过LoadingScene简单同步加载
        /// </summary>
        /// <param name="sceneName"></param>
        private void LoadScene(ISceneState scene)
        {
            //旧状态离开
            if (_CurScene != null)
                _CurScene.SceneExit();
            //先加载对应的AB包
            if (scene.IsABLoad)
            {
                GameManager.Instance.LoadSceneAB(scene.SceneName);
            }
            SceneManager.LoadScene(scene.SceneName);
            //赋值新状态
            _CurScene = scene;
        }
        /// <summary>
        /// 异步加载，返回异步操作数，协程操作
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        private void LoadSceneAsync(ISceneState scene)
        {
            //先加载对应的AB包
            if (scene.IsABLoad)
            {
                GameManager.Instance.LoadSceneAB(scene.SceneName);
            }
            //使用异步操作数直接赋值监控就行了，好像不用迭代
            LoadingProcess = SceneManager.LoadSceneAsync(scene.SceneName); //内部开启了一个协程，我们不用管它的逻辑
            LoadingProcess.allowSceneActivation = false; //异步加载
            _CurScene = scene;
        }
        /// <summary>
        /// 创建一个场景状态
        /// </summary>
        /// <returns></returns>
        public ISceneState CreateSceneState<T>(string sceneName, bool isABLoad, bool isWithLoading) where T : ISceneState, new()
        {
            T t = new T();
            t.Init(isWithLoading, isABLoad, sceneName, this);
            return t;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="scene">场景状态类</param>
        /// <param name="isLoading">是否已经通过Loading界面加载</param>
        public void SetSceneState(ISceneState scene, bool isLoading)
        {
            if (scene.IsWithLoading)
            {
                if (!isLoading)
                {
                    LoadSceneWithLoading(scene);
                }
                else
                {
                    LoadSceneAsync(scene);
                }
            }
            else
            {
                //加载新场景
                LoadScene(scene);
            }
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sceneName">场景名称</param>
        /// <param name="isABLoad">场景是否在AB包中</param>
        /// <param name="isWithLoading">是否属于异步Loading界面加载</param>
        /// <param name="isLoading">是否已经通过Loading界面加载</param>
        public void SetSceneState<T>(string sceneName, bool isABLoad, bool isWithLoading, bool isLoading) where T : ISceneState, new()
        {
            ISceneState scene = CreateSceneState<T>(sceneName, isABLoad, isWithLoading);
            SetSceneState(scene, isLoading);
        }

        public void Update()
        {
            //等待场景加载完成
            if (LoadingProcess != null)
            {
                if (!LoadingProcess.allowSceneActivation)
                {
                    //Loading界面更新
                    _LoadingScene.SceneUpdate();
                    return;
                }
                else
                {
                    //LoadingScene的离开，此时新场景已经Awake了
                    _LoadingScene.SceneExit();
                    LoadingProcess = null;
                }
            }
            //新状态初始化
            if (!_CurScene.IsBegin)
            {
                _CurScene.IsBegin = true;
                _CurScene.SceneBegin();
            }
            //场景状态更新
            _CurScene.SceneUpdate();
        }
    }
}
