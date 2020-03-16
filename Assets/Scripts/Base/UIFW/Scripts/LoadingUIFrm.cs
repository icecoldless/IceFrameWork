using UnityEngine;
using UnityEngine.UI;

namespace UIFW
{
    public class LoadingUIFrm : IHideOtherUserInterface
    {
        /// <summary>
        /// 滑动条
        /// </summary>
        Slider slider_Loading;
        /// <sumary>
        /// 加载进度值文本
        /// </summary>
        Text txt_LoadingValue;
        /// <summary>
        /// 进度值 不要随便修改由unity控制的值
        /// </summary>
        float _Progress;

        public void Awake()
        {
            //获得UI组件
            slider_Loading = UITool.GetComponentFromChildNode<Slider>(transform, "sliderLoading");
            txt_LoadingValue = UITool.GetComponentFromChildNode<Text>(transform, "txtLoadingValue");
        }

        public void Update()
        {
            if (GameManager.Instance.SceneMgr.LoadingProcess == null)
                return;
            _Progress = GameManager.Instance.SceneMgr.LoadingProcess.progress;
            if (Mathf.Approximately(_Progress, .9f))
            {
                _Progress = 1;
            }
            //更新UI
            UpdateUI(_Progress);
        }

        private void UpdateUI(float value)
        {
            if (slider_Loading.value < value && !Mathf.Approximately(slider_Loading.value, value))
            {
                slider_Loading.value += .05f;
            }
            if (txt_LoadingValue != null)
                txt_LoadingValue.text = Mathf.Round(slider_Loading.value * 100) + "%";
            if (Mathf.Approximately(slider_Loading.value, 1f))
            {
                GameManager.Instance.SceneMgr.LoadingProcess.allowSceneActivation = true;
            }
        }

        public override void Release()
        {
            //引用置空
            txt_LoadingValue = null;
            slider_Loading = null;

            base.Release();
        }
    }
}
