using UnityEngine;

/// <summary>
/// 单例，游戏对象
/// </summary>
/// <typeparam name="T">脚本类型</typeparam>
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }
            return _instance;
        }
    }
}