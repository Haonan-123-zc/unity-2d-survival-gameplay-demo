using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// 开场视频控制器，在播放完成或玩家跳过后进入正式关卡。
/// </summary>
/// <remarks>
/// 场景切换前会解除 VideoPlayer 事件订阅并恢复 timeScale，
/// 避免返回菜单后重复加载场景或保留暂停状态。
/// </remarks>
public class IntroVideoController : MonoBehaviour
{
	[Header("组件关联")]
	public VideoPlayer videoPlayer;

	[Header("正式关卡场景")]
	public string gameGameplaySceneName = "SampleScene";

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
			StartGameplay();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
		{
			StartGameplay();
		}
	}

	private void OnVideoFinished(VideoPlayer source)
	{
		StartGameplay();
	}

	private void StartGameplay()
	{
		Time.timeScale = 1f;
		if (videoPlayer != null)
		{
			videoPlayer.loopPointReached -= OnVideoFinished;
		}
		SceneManager.LoadScene(gameGameplaySceneName);
	}
}
