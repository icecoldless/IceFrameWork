using UnityEngine;

namespace UIFW
{
    /// <summary>
    /// 创建UI Panel对象
    /// </summary>
    public class UIFactory
    {
        private UIManager _UIManager;
        public UIFactory(UIManager uiManager)
        {
            this._UIManager = uiManager;
        }
        /// <summary>
        /// 创建一个UI窗体
        /// </summary>
        /// <typeparam name="T">UI窗体类型</typeparam>
        /// <param name="obj">Model</param>
        /// <returns></returns>
        public IUserInterface GetUIPanel(string resName)
        {
            //建造3D成像
            GameObject prefab = GameManager.Instance.LoadObject<GameObject>(resName);
            IUserInterface panel = GameObject.Instantiate(prefab).GetComponent<IUserInterface>();
            //设置上下文
            panel.AddContext(_UIManager);
            //设置3D成像（位置）
            panel.SetObject();
            //设置资源名称
            panel.ResName = resName;
            return panel;
        }
    }
}
