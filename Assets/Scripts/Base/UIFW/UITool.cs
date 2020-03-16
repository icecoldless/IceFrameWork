using UnityEngine;

namespace UIFW
{
    /// <summary>
    /// 工具人
    /// </summary>
    public class UITool
    {
        /// <summary>
        /// 根据父节点和子节点名称来查找到子节点
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="childName">子节点名字</param>
        /// <returns></returns>
        public static Transform FindChildNode(Transform parent, string childName)
        {
            Transform target = null;
            target = parent.Find(childName);
            if (target == null)
            {
                foreach (Transform childTran in parent)
                {
                    if (childTran != parent)
                    {
                        target = FindChildNode(childTran, childName);
                        if (target != null) //这一句很关键，如果搜索出来的target等于null了，返回了就不会继续搜索了
                            return target;
                    }
                }
            }
            return target;
        }
        /// <summary>
        /// 根据父节点和子节点名字来返回T类型的脚本组件
        /// Button
        /// </summary>
        /// <typeparam name="T">需要返回的脚本类型</typeparam>
        /// <param name="parent">父节点</param>
        /// <param name="chlldName">子节点名字</param>
        /// <returns></returns>
        public static T GetComponentFromChildNode<T>(Transform parent, string childName) where T : Component
        {
            T res = null;
            Transform childTran = FindChildNode(parent, childName);
            if (childTran != null)
            {
                res = childTran.GetComponent<T>();
            }
            return res;
        }
    }
}
