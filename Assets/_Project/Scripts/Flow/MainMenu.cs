using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单按钮入口，集中处理开始游戏与退出应用。
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>加载开场视频场景。</summary>
	public void PlayGame()
	{
		SceneManager.LoadScene("IntroVideoScene");
	}

    /// <summary>退出独立应用；编辑器内 Application.Quit 不会关闭编辑器。</summary>
	public void QuitGame()
	{
		Application.Quit();
	}
}
