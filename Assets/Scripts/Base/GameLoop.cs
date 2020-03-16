using UnityEngine;

/// <summary>
/// 游戏循环设计
/// 1.玩家操作
/// 2.游戏逻辑更新
/// 3.画面更新
/// </summary>
public class GameLoop : MonoBehaviour
{
    private void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
        GameManager.Instance.Initialize();
    }

    private void Update()
    {
        GameManager.Instance.SceneMgr.Update();
    }
}