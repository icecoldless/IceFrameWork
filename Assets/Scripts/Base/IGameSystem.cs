/// <summary>
/// 所有管理容器的父类
/// 基于游戏循环设计
/// </summary>
public abstract class IGameSystem
{
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Initialize() { }
    /// <summary>
    /// 更新
    /// </summary>
    public virtual void Update() { }
    /// <summary>
    /// 释放全部资源
    /// </summary>
    public virtual void ReleaseAll() { }
}