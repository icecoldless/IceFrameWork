/// <summary>
/// 定义资源加载模式
/// </summary>

namespace XLuaFramework
{
    public enum ResMode
    {
        LocalAB = 0,          //读取打包路径下的AB包
        Online = 1           //从网络下载AB包到本地读取
    }

}
