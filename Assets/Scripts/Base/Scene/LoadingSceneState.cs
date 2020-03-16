namespace Scene
{
    public class LoadingSceneState : ISceneState
    {
        /// <summary>
        /// Loading之后的一个场景
        /// </summary>
        private ISceneState _NextScene;
        /// <summary>
        /// 改变Loading场景接下来要跳转的场景
        /// </summary>
        /// <param name="scene"></param>
        public void SetNextScene(ISceneState scene)
        {
            this._NextScene = scene;
        }

        public override void SceneBegin()
        {
            base.SceneBegin();
            //打开LoadingUIFrm，下一个关卡需要异步进入
            GameManager.Instance.OpenUIFrm("LoadingUIFrm");
            //加载到下一个关卡，只能使用set   
            GameManager.Instance.SetSceneState(_NextScene, true);
        }
        public override void SceneExit()
        {
            _NextScene = null;
            IsBegin = false;
            GameManager.Instance.ReleaseUIFrm("LoadingUIFrm");
        }
    }
}
