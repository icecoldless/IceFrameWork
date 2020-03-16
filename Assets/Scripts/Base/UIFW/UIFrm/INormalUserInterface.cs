using UnityEngine;

namespace UIFW
{
    public class INormalUserInterface : IUserInterface
    {
        /// <summary>
        /// Normal的父节点：可能为Fixed或者Normal
        /// </summary>
        protected Transform TraParent;
        /// <summary>
        /// 设置3D成像
        /// </summary>
        /// <param name="_gameObject"></param>
        public override void SetObject()
        {
            this.gameObject.transform.SetParent(TraParent, false);
        }
        public override void Close()
        {
            base.Close();
            Hide();
            if (base._UIManager.DicCurShowUIFrm.ContainsKey(ResName))
                _UIManager.DicCurShowUIFrm.Remove(ResName);
        }

        public override void Open()
        {
            base.Open();
            Display();
            if (!base._UIManager.DicCurShowUIFrm.ContainsKey(ResName))
            {
                _UIManager.DicCurShowUIFrm[ResName] = this;
            }
        }
        public override void Release()
        {
            if (base._UIManager.DicCurShowUIFrm.ContainsKey(ResName))
                _UIManager.DicCurShowUIFrm.Remove(ResName);
            base.Release();
        }
    }
}
