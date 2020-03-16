namespace Scene
{
    public abstract class ISceneState
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName;
        /// <summary>
        /// 是否通过AB加载场景
        /// </summary>
        public bool IsABLoad;
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        public bool IsBegin;
        /// <summary>
        /// 是否要通过Loading方式加载到新场景中
        /// </summary>
        public bool IsWithLoading;
        /// <summary>
        /// 状态上下文
        /// </summary>
        protected SceneMgr _Context;
        /// <summary>
        /// 外部不要使用，仅供SceneMgr使用
        /// </summary>
        /// <param name="isWithLoading">是否通过Loading界面异步加载</param>
        /// <param name="isABLoad">是否通过AB包加载场景</param>
        /// <param name="sceneName">场景名称</param>
        /// <param name="context">上下文</param>
        public void Init(bool isWithLoading, bool isABLoad, string sceneName, SceneMgr context)
        {
            this.IsBegin = false;
            this.IsWithLoading = isWithLoading;
            this.IsABLoad = isABLoad;
            this.SceneName = sceneName;
            this._Context = context;
        }
        /// <summary>
        /// 场景开始
        /// </summary>
        public virtual void SceneBegin()
        {

        }
        /// <summary>
        /// 场景进入
        /// </summary>
        public virtual void SceneUpdate()
        {
            GameManager.Instance.Update();
        }
        /// <summary>
        /// 场景离开时
        /// </summary>
        public virtual void SceneExit()
        {
            //场景离开释放所有的内存镜像
            GameManager.Instance.ReleaseAll();
        }
    }
}
