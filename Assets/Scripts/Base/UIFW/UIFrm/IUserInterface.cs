using UnityEngine;
using System;

namespace UIFW
{
    /// <summary>
    /// UI界面
    /// 模板设计模式：接口控制流程
    /// 还是改为由普通Unity的声明周期来控制初始化，更新的操作
    /// </summary>
    public abstract class IUserInterface : MonoBehaviour
    {
        [NonSerialized]
        public string ResName;                                  //资源名称
        protected UIManager _UIManager;                         //UIContext
        [NonSerialized]
        public bool IsPermanent;                                //是否是常驻UI
        [NonSerialized]
        public bool AutoRecycle;                                //是否自动回收
        [ReadOnly]
        public float HideTime;                                  //在内存中隐藏了多长时间
        /// <summary>
        /// 设置3D成像的位置
        /// </summary>
        public virtual void SetObject()
        {

        }
        /// <summary>
        /// UI释放
        /// </summary>
        public virtual void Release()
        {
            //内存资源释放
            if (!IsOpen())
            {
                _UIManager.LstHideUIFrm.Remove(this);
            }
            _UIManager.DicAllUIFrm.Remove(ResName);
            //删除3D成像
            GameObject.Destroy(this.gameObject);
            //释放prefab的内存镜像资源。
            GameManager.Instance.UnloadAsset(ResName);
        }
        /// <summary>
        /// 设置管理上下文
        /// </summary>
        /// <param name="uiSystem"></param>
        public void AddContext(UIManager uiSystem)
        {
            _UIManager = uiSystem;
        }
        /// <summary>
        /// 窗体的显示
        /// </summary>
        public void Display()
        {
            //控制3D物体动画效果
            this.gameObject.SetActive(true);
        }
        /// <summary>
        /// 窗体的隐藏
        /// </summary>
        public void Hide()
        {
            //控制3D物体动画效果
            this.gameObject.SetActive(false);
        }
        /// <summary>
        /// 窗体的打开
        /// </summary>
        public virtual void Open()
        {
            //移除回收记录
            _UIManager.LstHideUIFrm.Remove(this);
            //Reset HideTime
            HideTime = 0;
        }
        /// <summary>
        /// 窗体的关闭
        /// </summary>
        public virtual void Close()
        {
            //记录回收，过滤第一层，过滤掉那些不需要自动回收管理的UI
            if (
                !this.IsPermanent
                ||
                this.AutoRecycle
                )
                _UIManager.LstHideUIFrm.Add(this);
        }
        /// <summary>
        /// 窗体是否打开
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOpen()
        {
            return this.gameObject.activeSelf;
        }
    }
}
