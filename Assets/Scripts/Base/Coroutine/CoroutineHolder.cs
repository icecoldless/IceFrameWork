using System;
using System.Collections;
using UnityEngine;

namespace Coroutine
{
    /// <summary>
    /// 一个简易的装饰器设计
    /// 包裹Coroutine，加上相应需要的功能
    /// 因为功能比较单一，所以没必要抽象出一个独立的接口，Coroutine也很单一，没有其他抽象
    /// </summary>
    public class CoroutineHolder
    {
        /// <summary>
        /// 包裹的协程
        /// </summary>
        private IEnumerator coroutine;
        /// <summary>
        /// 协程暂停
        /// </summary>
        private bool paused = false;
        /// <summary>
        /// 协程运行
        /// </summary>
        private bool running = false;
        /// <summary>
        /// 协程停止
        /// </summary>
        private bool stopped = false;

        #region 完成时回调
        private event Action Finished;
        #endregion

        public CoroutineHolder(IEnumerator c, bool autoStart = true)
        {
            coroutine = c;
            //默认完成事件，清理回收
            Finished += Finish;
            //默认自动启动
            if (autoStart)
                Start();
        }
        /// <summary>
        /// 设置协程暂停
        /// </summary>
        public void Paused()
        {
            paused = true;
        }
        /// <summary>
        /// 设置协程继续
        /// </summary>
        public void Resume()
        {
            paused = false;
        }
        /// <summary>
        /// 设置协程停止
        /// </summary>
        public void Stop()
        {
            stopped = true;
            running = false;
        }
        /// <summary>
        /// 添加完成事件
        ///     链式调用
        /// </summary>
        /// <param name="handle">委托方法</param>
        /// <returns></returns>
        public CoroutineHolder OnComplete(Action handle)
        {
            Finished += handle;
            return this;
        }
        /// <summary>
        /// 设置协程开始
        /// </summary>
        public void Start()
        {
            if (null != coroutine)
            {
                running = true;
                //让协程载体来启动协程
                CoroutineManager.Instance.StartCoroutine(CallWrapper());
            }
            else
            {
                Debug.Log("Coroutine 未指定，避免直接调用该方法。");
            }
        }

        /// <summary>
        /// 回收：
        ///     完成回调并断引用
        /// </summary>
        private void Finish()
        {
            Finished = null;
            coroutine = null;
        }

        /// <summary>
        /// 协程启动Core
        /// </summary>
        /// <returns></returns>
        IEnumerator CallWrapper()
        {
            yield return null;
            IEnumerator e = coroutine;
            while (running)
            {
                if (paused)
                    yield return null;
                else
                {
                    if (e != null && e.MoveNext())
                    {
                        yield return e.Current;
                    }
                    else
                    {
                        running = false;
                    }
                }
            }
            Finished();
        }
    }
}
