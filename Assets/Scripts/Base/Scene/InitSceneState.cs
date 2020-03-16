using Scene;

/// <summary>
/// 第一个场景
/// 一定要有初始化场景，用来初始化GameLoop
/// 否则如果有场景跳转到Start场景后会存在多个GameLoop
/// </summary>
namespace Scene
{
    public class InitSceneState : ISceneState
    {
        public override void SceneBegin()
        {
            base.SceneBegin();
            //加载到某一个场景（开始，登录）
        }
    }
}
