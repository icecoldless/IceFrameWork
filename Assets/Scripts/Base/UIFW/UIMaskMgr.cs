using UnityEngine;

/// <summary>
/// UI遮罩
/// 启用遮罩和关闭遮罩
/// </summary>
namespace UIFW
{
    public class UIMaskMgr
    {
        /// <summary>
        /// 节点
        /// </summary>
        GameObject UIMask; //遮罩的物体

        public UIMaskMgr(GameObject uiMask)
        {
            //获得UIMask
            UIMask = uiMask;
        }
        public void SetMaskWindow(GameObject UIForm)
        {
            UIMask.SetActive(true);
            //PopUp UIForm下移
            UIForm.transform.SetAsLastSibling();
        }
        public void CancelMaskWindow()
        {
            //隐藏UiMask
            if (UIMask.activeInHierarchy)
                UIMask.SetActive(false);
        }
    }
}
