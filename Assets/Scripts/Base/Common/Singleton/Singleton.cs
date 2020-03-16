/// <summary>
/// 单例，不挂载游戏对象
/// </summary>
/// <typeparam name="T">脚本类型</typeparam>
public abstract class Singleton<T> where T : class, new()
{
    static T _Instance;

    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new T();
            }
            return _Instance;
        }
    }

}