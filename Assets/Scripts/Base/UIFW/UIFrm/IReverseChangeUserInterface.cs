using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIFW
{
    public class IReverseChangeUserInterface : IUserInterface
    {
        /// <summary>
        /// 设置3D成像
        /// </summary>
        /// <param name="_gameObject"></param>
        public override void SetObject()
        {
            this.gameObject.transform.SetParent(_UIManager.TraPopUp, false);
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (!IsOpen())
            {
                Debug.LogWarning($"这个窗体还没有打开，不需要关闭！{this.GetType().Name}");
                return;
            }
            //关闭遮罩
            _UIManager.UIMaskMgr.CancelMaskWindow();
            //清除栈顶元素
            IUserInterface userInterface = base._UIManager.StackUIFrm.Pop();
            if (userInterface != this)
            {
                Debug.LogError("当前PopUp不是栈顶元素无法关闭!Close");
                return;
            }
            userInterface.Hide();
            //重绘栈顶元素
            if (base._UIManager.StackUIFrm.Count > 0)
            {
                userInterface = base._UIManager.StackUIFrm.Peek();
                userInterface.Display();
            }
        }
        /// <summary>
        /// 打开
        /// </summary>
        public override void Open()
        {
            base.Open();
            IUserInterface ui;
            //已经具有显示栈顶元素，说明Mask已经打开
            if (_UIManager.StackUIFrm.Count() > 0)
            {
                ui = _UIManager.StackUIFrm.Peek();
                ui.Hide();
            }
            else
            {
                //打开遮罩
                _UIManager.UIMaskMgr.SetMaskWindow(this.gameObject);
            }
            //把栈顶元素隐藏
            if (_UIManager.StackUIFrm.Contains(this))
            {
                Stack<IUserInterface> tmpS = new Stack<IUserInterface>();
                while (this != _UIManager.StackUIFrm.Peek())
                {
                    tmpS.Push(_UIManager.StackUIFrm.Pop());
                }
                //找到自身
                _UIManager.StackUIFrm.Pop();
                //把tmps中的元素重新加会stack中
                while (tmpS.Count() > 0)
                {
                    ui = tmpS.Pop();
                    _UIManager.StackUIFrm.Push(ui);
                }
                //把自身换到最顶层
                _UIManager.StackUIFrm.Push(this);
            }
            else
            {
                //加入栈顶元素
                _UIManager.StackUIFrm.Push(this);
            }
            //显示自身元素
            Display();
        }

        public override void Release()
        {
            //删除缓存中元素
            if (base._UIManager.StackUIFrm.Contains(this))
            {
                Stack<IUserInterface> tmps = new Stack<IUserInterface>();
                while (base._UIManager.StackUIFrm.Peek() != this)
                {
                    tmps.Push(base._UIManager.StackUIFrm.Pop());
                }
                base._UIManager.StackUIFrm.Pop();
                while (tmps.Count() > 0)
                {
                    _UIManager.StackUIFrm.Push(tmps.Pop());
                }
            }
            base.Release();
        }
    }
}
