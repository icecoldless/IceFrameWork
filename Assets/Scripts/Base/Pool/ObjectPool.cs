using UnityEngine;
using System.Collections.Generic;
namespace Pool
{
    /// <summary>
    /// 需要经由对象池加载的Prefab
    /// </summary>
    [System.Serializable]
    public class ObjectPool
    {
        //初始化最大数量
        public int InitialBufferSize;
        //预制体的名称，可推断出资源名称，由AB包进行一个加载
        public string PrefabName;
        //是否允许扩展
        public bool ShouldExpand;
        //每次扩展个数
        public int ExpandSize;
        //下一个可用GO的索引
        [System.NonSerialized]
        public int NextAvailableIndex = 0;
        //池内资源是否自动释放
        public bool IsAutoRecycle = true;
        //这种资源的最大隐藏时间
        [ReadOnly]
        public int MaxHideTime = 10;     //TODO 可以继续修改，让MaxHideTime字段根据IsAutoRecycle来变化，未启用就不可编辑，为0，启用就默认状态，可编辑为5
        [System.NonSerialized]
        //对象集合
        public List<IPoolableObject> PoolObjects = new List<IPoolableObject>();
    }

}
