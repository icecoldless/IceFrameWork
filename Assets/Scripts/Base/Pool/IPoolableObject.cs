using System;
using UnityEditor;
using UnityEngine;

namespace Pool
{
    public class IPoolableObject : MonoBehaviour
    {
        //隐藏时间超过就自动释放
        [ReadOnly]
        public float HideTime;
        //对应的预制体名称
        [ReadOnly]
        public string PrefabName;
        public void Reset()
        {
            PrefabName = this.gameObject.name;
        }
        /// <summary>
        /// 物体第一次被对象池产生的时候需要初始化做的事情
        /// </summary>
        public virtual void OneTimeInitAction()
        {

        }
        /// <summary>
        /// 当这个对象池被重置显示的时候初始化需要做的事情
        /// </summary>
        public virtual void ResetAction()
        {
            HideTime = 0;
        }
        /// <summary>
        /// 当这个对象被对象池回收的时候需要做的事情
        /// </summary>
        public virtual void RecycleAction()
        {
            //设置父物体为NULL
            transform.SetParent(null, false);
        }
    }
}
