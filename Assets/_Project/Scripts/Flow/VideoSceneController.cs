using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// 通用视频场景控制器，在播放结束、按下 Space 或 Escape 时返回主菜单。
/// </summary>
/// <remarks>
/// 使用事件驱动结束判断，并显式解绑事件，防止场景重新加载后重复触发。
/// </remarks>
public class VideoSceneController : MonoBehaviour
{
	[Header("组件关联")]
	public VideoPlayer videoPlayer;

	[Header("下一场景")]
	public string mainMenuSceneName = "MainMenu";

	private void Start()
	{
		if (videoPlayer == null)
		{
			videoPlayer = GetComponent<VideoPlayer>();
		}
		if (videoPlayer != null)
		{
			videoPlayer.loopPointReached += OnVideoFinished;
			videoPlayer.Play();
		}
		else
		{
			Debug.LogError("[VideoSceneController] 未找到 VideoPlayer 组件。", this);
			ReturnToMainMenu();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
		{
			ReturnToMainMenu();
		}
	}

	private void OnVideoFinished(VideoPlayer source)
	{
		ReturnToMainMenu();
	}

	private void ReturnToMainMenu()
	{
		Time.timeScale = 1f;
		if (videoPlayer != null)
		{
			videoPlayer.loopPointReached -= OnVideoFinished;
		}
		SceneManager.LoadScene(mainMenuSceneName);
	}
}
