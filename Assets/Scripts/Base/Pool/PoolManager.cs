using System.Collections.Generic;
using UnityEngine;

namespace Pool
{
    /// <summary>
    /// 属于场景
    /// 不同的场景共享不同的对象集合
    /// 一个物体只有一个池子!
    /// 对象池一定是管理Prefab的，不可能对一张贴图进行对象池管理
    /// 我的对象池使用标签来进行管理，通过标签推断出对象的资源名称，例如对象资源名称为Road1.prefab，标签就得为Road1
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        //共享单例
        public static PoolManager SharedInstance;
        //检测自动释放时间
        public int AutoReleaseTime = 10;
        public List<ObjectPool> m_PoolList = new List<ObjectPool>();
        //隐藏中的物体
        private List<IPoolableObject> m_HideObjs = new List<IPoolableObject>();
        //避免在循环迭代的时候新增集合
        private List<IPoolableObject> m_ExpandHideObjs = new List<IPoolableObject>();
        //避免在循环迭代的时候减少集合
        private List<IPoolableObject> m_ReduceHideObjs = new List<IPoolableObject>();
        //是否正在更新隐藏物体
        private bool m_IsUpdateHideTime = false;
        //避免在迭代对象池集合的时候自动回收对象池内的资源对象
        private List<IPoolableObject> m_ReleaseObjs = new List<IPoolableObject>();
        private void Awake()
        {
            SharedInstance = this;

            //对象池初始化
            for (int i = 0; i < m_PoolList.Count; i++)
            {
                string resName = m_PoolList[i].PrefabName + ".prefab";
                for (int j = 0; j < m_PoolList[i].InitialBufferSize; j++)
                {
                    GameObject obj = (GameObject)Instantiate(GameManager.Instance.LoadObject<GameObject>(resName));
                    OneTimeInitObject(obj);
                }
            }
            //开启自动回收
            InvokeRepeating("AutoRelease", AutoReleaseTime, AutoReleaseTime);
        }
        /// <summary>
        /// 处理对象资源的隐藏时间
        /// </summary>
        private void Update()
        {
            m_IsUpdateHideTime = true;
            for (int i = 0; i < m_HideObjs.Count; i++)
            {
                m_HideObjs[i].HideTime += Time.deltaTime;
            }
            m_IsUpdateHideTime = false;
            if (m_ExpandHideObjs.Count > 0)
            {
                m_HideObjs.AddRange(m_ExpandHideObjs);
                m_ExpandHideObjs.Clear();
            }
            if (m_ReduceHideObjs.Count > 0)
            {
                for (int i = 0; i < m_ReduceHideObjs.Count; i++)
                {
                    m_HideObjs.Remove(m_ReduceHideObjs[i]);
                }
                m_ReduceHideObjs.Clear();
            }
        }
        /// <summary>
        /// New新物体
        /// </summary>
        /// <param name="prefabName">物体的prefabName=物体名称</param>
        /// <returns></returns>
        public IPoolableObject New(string prefabName)
        {
            IPoolableObject o;
            //如果池子里面存在需要的对象
            for (int i = 0; i < m_PoolList.Count; i++)
            {
                if (m_PoolList[i].PrefabName == prefabName)
                {
                    if (m_PoolList[i].NextAvailableIndex < m_PoolList[i].PoolObjects.Count)
                    {
                        o = m_PoolList[i].PoolObjects[m_PoolList[i].NextAvailableIndex++];
                        //显示之前先重置物体
                        o.ResetAction();
                        //显示加载出来的GameObject物体
                        o.gameObject.SetActive(true);
                        //从HideList中剔除
                        if (!m_IsUpdateHideTime)
                            m_HideObjs.Remove(o);
                        else
                            m_ReduceHideObjs.Add(o);
                        return o;
                    }
                    else
                    {
                        //新增物体
                        if (m_PoolList[i].ShouldExpand)
                        {
                            string resName = prefabName + ".prefab";
                            for (int j = 0; j < m_PoolList[i].ExpandSize; j++)
                            {
                                GameObject obj = (GameObject)Instantiate(GameManager.Instance.LoadObject<GameObject>(resName));
                                OneTimeInitObject(obj);
                            }
                            //补全占满
                            m_PoolList[i].NextAvailableIndex++;
                            o = m_PoolList[i].PoolObjects[m_PoolList[i].NextAvailableIndex - 1];
                            //显示加载出来的GameObject物体,出来的都是第一次出生的物体，所以不需要重置
                            o.gameObject.SetActive(true);
                            //从HideList中剔除
                            if (!m_IsUpdateHideTime)
                                m_HideObjs.Remove(o);
                            else
                                m_ReduceHideObjs.Add(o);
                            return o;
                        }
                        else
                        {
                            Debug.LogError(prefabName + "对象池已经占满，而且不允许拓展!--" + "当前活动指针为：" + m_PoolList[i].NextAvailableIndex);
                            return null;
                        }
                    }
                }
            }
            Debug.LogError(prefabName + "的物体在该场景中不由对象池进行管理!");
            return null;
        }
        /// <summary>
        /// 回收一个物体
        /// </summary>
        /// <param name="o">物体</param>
        public void RecycleObject(IPoolableObject o)
        {
            //物体回收
            for (int i = 0; i < m_PoolList.Count; i++)
            {
                if (m_PoolList[i].PrefabName == o.PrefabName)
                {
                    if (m_PoolList[i].PoolObjects.Contains(o))
                    {
                        m_PoolList[i].PoolObjects.Remove(o);
                    }
                    m_PoolList[i].PoolObjects.Add(o);
                    //更新活动指针
                    if (m_PoolList[i].NextAvailableIndex >= m_PoolList[i].PoolObjects.Count)
                    {
                        m_PoolList[i].NextAvailableIndex = m_PoolList[i].PoolObjects.Count - 1;
                    }
                    break;
                }
            }
            //回收物体委托回调
            o.RecycleAction();
            //物体隐藏
            o.gameObject.SetActive(false);
            if (!m_IsUpdateHideTime)
                m_HideObjs.Add(o);
            else
                m_ExpandHideObjs.Add(o);
        }
        /// <summary>
        /// 物体第一次引进对象池
        /// </summary>
        /// <param name="o">对象</param>
        private void OneTimeInitObject(GameObject obj)
        {
            IPoolableObject o = obj.GetComponent<IPoolableObject>();
            if (o == null)
            {
                Debug.LogError("未在内存池中找到此对象：" + obj.name);
                return;
            }
            //加入对象池
            for (int i = 0; i < m_PoolList.Count; i++)
            {
                if (m_PoolList[i].PrefabName == o.PrefabName)
                {
                    m_PoolList[i].PoolObjects.Add(o);
                    break;
                }
            }
            //第一次出生初始化函数回调
            o.OneTimeInitAction();
            //物体隐藏
            o.gameObject.SetActive(false);
            if (!m_IsUpdateHideTime)
                m_HideObjs.Add(o);
            else
                m_ExpandHideObjs.Add(o);
        }
        /// <summary>
        /// 卸载释放一个对象（从对象池中）
        /// </summary>
        /// <param name="o">对象</param>
        public void ReleaseObject(IPoolableObject o)
        {
            //总池删除
            for (int i = 0; i < m_PoolList.Count; i++)
            {
                if (m_PoolList[i].PrefabName == o.PrefabName)
                {
                    //维护活动指针!
                    //正好是当前活动指针的元素，则不需要维护。
                    //大于当前活动指针的元素，不维护
                    //当前活动指针大于--，维护
                    int oindex = m_PoolList[i].PoolObjects.IndexOf(o);
                    if (oindex < m_PoolList[i].NextAvailableIndex)
                    {
                        --m_PoolList[i].NextAvailableIndex;
                    }
                    m_PoolList[i].PoolObjects.Remove(o);
                    break;
                }
            }
            //隐藏池删除
            if (m_HideObjs.Contains(o))
            {
                if (m_IsUpdateHideTime)
                    m_HideObjs.Remove(o);
                else
                    m_ReduceHideObjs.Add(o);
            }
            Destroy(o.gameObject, .5f);
        }
        /// <summary>
        /// 全部卸载
        /// </summary>
        public void ReleaseAll()
        {
            //取消回调
            CancelInvoke();
            //内存释放
            m_HideObjs.Clear();
            m_PoolList.Clear();
            m_ReduceHideObjs.Clear();
            m_ExpandHideObjs.Clear();
            //单例释放
            SharedInstance = null;
        }
        /// <summary>
        /// 自动回收
        /// </summary>
        private void AutoRelease()
        {
            for (int i = 0; i < m_PoolList.Count; i++)
            {
                for (int j = 0; j < m_PoolList[i].PoolObjects.Count; j++)
                {
                    if (m_PoolList[i].IsAutoRecycle)
                    {
                        if (m_PoolList[i].PoolObjects[j].HideTime > m_PoolList[i].MaxHideTime)
                        {
                            //记录回收
                            m_ReleaseObjs.Add(m_PoolList[i].PoolObjects[j]);
                        }
                    }
                }
            }
            //真正回收
            if (m_ReleaseObjs.Count > 0)
            {
                for (int i = 0; i < m_ReleaseObjs.Count; i++)
                {
                    ReleaseObject(m_ReleaseObjs[i]);
                }
            }
            //集合清空
            m_ReleaseObjs.Clear();
        }
    }
}
