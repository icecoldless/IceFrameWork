namespace UIFW
{
    public class IHideOtherUserInterface : IUserInterface
    {
        /// <summary>
        /// 设置3D成像
        /// </summary>
        /// <param name="_gameObject"></param>
        public override void SetObject()
        {
            this.gameObject.transform.SetParent(_UIManager.TraNormal, false);
        }
        public override void Close()
        {
            base.Close();
            IUserInterface ui;
            Hide();
            if (_UIManager.DicCurShowUIFrm.ContainsKey(ResName))
                _UIManager.DicCurShowUIFrm.Remove(ResName);
            //显示Normal
            foreach (var item in _UIManager.DicCurShowUIFrm)
            {
                item.Value.Display();
            }
            //显示PopUp
            ui = _UIManager.StackUIFrm.Peek();
            ui.Display();
        }

        public override void Open()
        {
            base.Open();
            IUserInterface ui;
            //隐藏Normal
            foreach (var item in _UIManager.DicCurShowUIFrm)
            {
                item.Value.Hide();
            }
            //隐藏PopUp
            if (_UIManager.StackUIFrm.Count > 0)
            {
                ui = _UIManager.StackUIFrm.Peek();
                ui.Hide();
            }
            //显示自身
            Display();
            if (!_UIManager.DicCurShowUIFrm.ContainsKey(ResName))
                _UIManager.DicCurShowUIFrm.Add(ResName, this);
        }

        public override void Release()
        {
            if (_UIManager.DicCurShowUIFrm.ContainsKey(ResName))
                _UIManager.DicCurShowUIFrm.Remove(ResName);
            base.Release();
        }
    }
}
