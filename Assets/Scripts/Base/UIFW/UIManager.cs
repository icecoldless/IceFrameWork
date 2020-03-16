using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIFW
{
    /// <summary>
    /// UI框架管理容器
    /// 管理所有已加载UIFrm
    /// 
    /// 内存回收：
    /// 检测所有直接继承IUserInterface的窗口，常驻窗口不需要回收。其余的如果发现窗口是隐藏状态，并且启用了自动回收（默认的m_bAutoRecycle = true;），那就需要对这个窗口进行销毁处理。
    //  销毁之前会检测这个窗口隐藏的时间是否超过了5s，超过就回收掉。  这个时间的记录是在调用窗口Hide（）的时候记录的。
    //  每10秒检测一次
    /// </summary>
    public class UIManager : IGameSystem
    {
        //工厂
        private UIFactory _Factory;
        //Canvas
        public Transform TraCanvas;
        //Normal
        public Transform TraNormal;
        //Fixed
        public Transform TraFixed;
        //PopUp
        public Transform TraPopUp;
        //UIMask
        public UIMaskMgr UIMaskMgr;
        //所有UIFrm
        public Dictionary<string, IUserInterface> DicAllUIFrm;
        //当前显示在游戏中的UIFrm
        public Dictionary<string, IUserInterface> DicCurShowUIFrm;
        //当前隐藏未释放在游戏中的UIFrm
        public List<IUserInterface> LstHideUIFrm;
        //避免在For循环中修改隐藏UIFrm集合
        private List<IUserInterface> _TmpLstHideUIFrm;
        //堆栈中的UIFrm
        public Stack<IUserInterface> StackUIFrm;

        //回收机制
        private int _RecycleTime;
        //记录时间
        private float _RecordTime;
        //最小隐藏时间
        [ReadOnly]
        public int _MinHideTime = 5;

        public override void Initialize()
        {
            base.Initialize();
            //加载Canvas
            TraCanvas = LoadCanvas();
            //获得Normal
            TraNormal = UITool.FindChildNode(TraCanvas, "Normal");
            //获得Fixed
            TraFixed = UITool.FindChildNode(TraCanvas, "Fixed");
            //获得PopUp
            TraPopUp = UITool.FindChildNode(TraCanvas, "PopUp");
            //获得UIMask
            GameObject maskObj = UITool.FindChildNode(TraCanvas, "UIMask").gameObject;
            UIMaskMgr = new UIMaskMgr(maskObj);
            //获得工厂 
            _Factory = new UIFactory(this);

            //集合分配内存空间
            DicAllUIFrm = new Dictionary<string, IUserInterface>();
            DicCurShowUIFrm = new Dictionary<string, IUserInterface>();
            StackUIFrm = new Stack<IUserInterface>();
            LstHideUIFrm = new List<IUserInterface>();
            _TmpLstHideUIFrm = new List<IUserInterface>();
            //回收
            _RecycleTime = 10;
            _MinHideTime = 5;
        }
        /// <summary>
        /// 加载Canvas
        /// </summary>
        /// <returns></returns>
        private Transform LoadCanvas()
        {
            //这是Resources目录的
            GameObject prefab = GameManager.Instance.LoadObject<GameObject>("Canvas");
            GameObject go = GameObject.Instantiate(prefab);
            GameManager.Instance.UnloadAsset("Canvas");
            return go.transform;
        }
        /// <summary>
        /// 打开一个UI窗体
        /// </summary>
        /// <param name="uiFrmName"></param>
        /// <returns></returns>
        public IUserInterface OpenUIFrm(string uiName, bool m_isPermanent = false, bool m_bAutoRecycle = true)
        {
            //获得UI资源名称
            string resName = uiName + ".prefab";
            IUserInterface userInterface = null;
            //从缓存中查询
            if (DicAllUIFrm.ContainsKey(resName))
            {
                userInterface = DicAllUIFrm[resName];
                //UI资源已经加载在游戏中，只需要显示即可
                if (userInterface.IsOpen())
                {
                    Debug.LogWarning($"UI已经处于显示中...{resName}");
                }
                else
                    userInterface.Open();
                return userInterface;
            }
            //获得Panel
            userInterface = _Factory.GetUIPanel(resName);
            //是否常驻，是否自动清理，回收。
            userInterface.IsPermanent = m_isPermanent;
            userInterface.AutoRecycle = m_bAutoRecycle;
            //开启UIPanel
            userInterface.Open();
            //加入缓存，更新不由自己来控制了
            DicAllUIFrm[resName] = userInterface;
            return userInterface;
        }
        /// <summary>
        /// 关闭UIFrm
        /// </summary>
        /// <param name="userInterface"></param>
        public void CloseUIFrm(string uiName)
        {
            //获得UI的资源名称
            string resName = uiName + ".prefab";
            IUserInterface userInterface = null;

            if (DicAllUIFrm.ContainsKey(resName))
                userInterface = DicAllUIFrm[resName];

            if (userInterface == null)
            {
                Debug.LogWarning($"这个资源并没有开启，未加载进缓存中，不需要关闭{resName}");
                return;
            }
            if (!userInterface.IsOpen())
            {
                Debug.LogWarning("这个资源并没有开启，不需要关闭");
                return;
            }
            userInterface.Close();
        }
        /// <summary>
        /// 卸载某个指定的UIFrm
        /// </summary>
        /// <param name="userInterface"></param>
        public void ReleaseUIFrm(string uiName)
        {
            string resName = uiName + ".prefab";
            IUserInterface userInterface = null;
            if (DicAllUIFrm.ContainsKey(resName))
            {
                userInterface = DicAllUIFrm[resName];
            }
            if (userInterface == null)
            {
                Debug.LogWarning($"这个资源未加载进缓存中，不需要释放{resName}");
                return;
            }
            userInterface.Release();
            //Debug.Log("AllUIFrm Count = " + DicAllUIFrm.Count);
            //Debug.Log("DicShowUIFrm Count = " + DicAllUIFrm.Count);
            //Debug.Log("Stack Count = " + StackUIFrm.Count);
            //Debug.Log("HideList Count = " + LstHideUIFrm.Count);
        }
        /// <summary>
        /// 卸载某个指定的UIFrm
        /// </summary>
        /// <param name="uiFrm">对应的UI窗体对象</param>
        private void ReleaseUIFrm(IUserInterface uiFrm)
        {
            uiFrm.Release();
        }
        /// <summary>
        /// UI管理器更新逻辑
        /// 处理UIFrm的自动回收
        /// </summary>
        public override void Update()
        {
            base.Update();
            //更新HideUIFrm的隐藏时间
            for (int i = 0; i < LstHideUIFrm.Count; i++)
            {
                LstHideUIFrm[i].HideTime += Time.deltaTime;
            }
            while (_RecordTime < _RecycleTime)
            {
                _RecordTime += Time.deltaTime;
                return;
            }
            _RecordTime = 0;
            //记录一次回收
            for (int i = 0; i < LstHideUIFrm.Count; i++)
            {
                //过滤第二层，过滤掉隐藏时间未达到要求的UIFrm，第一层在Close的时候已经过滤过。
                if (LstHideUIFrm[i].HideTime < _MinHideTime)
                    continue;
                _TmpLstHideUIFrm.Add(LstHideUIFrm[i]);
            }
            //开始真正回收
            _TmpLstHideUIFrm.ForEach(u => ReleaseUIFrm(u)); //内部对HideList进行了维护，删除。
            //回收完临时集合清空。
            _TmpLstHideUIFrm.Clear();
        }
        /// <summary>
        /// 卸载全部资源，保留UIRoot
        /// </summary>
        public override void ReleaseAll()
        {
            //清除场景引用
            DicAllUIFrm.Values.ToList().ForEach(m => m.Release());
            //资源回收
            Resources.UnloadUnusedAssets();
        }
    }
}
