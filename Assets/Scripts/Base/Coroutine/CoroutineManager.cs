using System.Collections;

namespace Coroutine
{
    /// <summary>
    /// 协程管理容器
    /// 管理协程的开启,而协程本身
    /// 需要支持协程嵌套
    /// 现在的协程只适合私有逻辑，就是业务类提供协程，而且仅供这个业务需要使用，别的业务不会参与。
    /// </summary>
    public class CoroutineManager : SingletonMono<CoroutineManager>
    {
        /// <summary>
        /// 直接运行一个协程
        /// </summary>
        /// <param name="target">协程本身</param>
        /// <returns></returns>
        public CoroutineHolder Run(IEnumerator target)
        {
            return new CoroutineHolder(target);
        }
        /// <summary>
        /// 获得一个协程装饰器，不自动运行
        /// </summary>
        /// <param name="target">协程</param>
        /// <returns></returns>
        public CoroutineHolder Get(IEnumerator target)
        {
            return new CoroutineHolder(target, false);
        }
    }
}
