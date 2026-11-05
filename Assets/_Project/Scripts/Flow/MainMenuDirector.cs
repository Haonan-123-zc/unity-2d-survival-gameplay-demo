using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单入场演出控制器，按时间轴编排黑场、Logo 弹入和灰色斜框对撞。
/// </summary>
/// <remarks>
/// 动画全部由协程按配置时长推进，避免依赖大量 Animator 状态和动画事件。
/// isTransitioning 防止玩家在入场演出中重复点击开始按钮。
/// </remarks>
public class MainMenuDirector : MonoBehaviour
{
	[Header("场景切换")]
	[Tooltip("这里填写你刚刚创建的开场视频场景的名字")]
	public string introVideoSceneName = "IntroVideoScene";

	[Header("UI 组件")]
	public Image fadeBlackOverlay;

	public GameObject logoImage;

	public RectTransform leftGrayBar;

	public RectTransform rightGrayBar;

	[Header("第一幕：黑场与 Logo")]
	public float fadeOutBlackDuration = 0.8f;

	public float logoShootDuration = 0.35f;

	public float logoStayDuration = 1.2f;

	[Header("第二幕：斜框运动")]
	public float barsCombineDuration = 0.35f;

	public float barsSplitDuration = 0.5f;

	public float logoMoveDuration = 0.5f;

	[Header("位移锚点")]
	public Vector3 logoRightTargetPosition = new Vector3(450f, 250f, 0f);

	public float leftBarOutOffset = -1500f;

	public float rightBarOutOffset = 1500f;

	public float rightBarStayOffset = 450f;

	private Vector3 logoInitialCenterPos;

	private bool isTransitioning;

	private void Start()
	{
		if (leftGrayBar == null || rightGrayBar == null || logoImage == null)
		{
			Debug.LogError("[MainMenuDirector] 入场动画所需的 UI 引用未配置完整。", this);
			return;
		}
		if (fadeBlackOverlay != null)
		{
			fadeBlackOverlay.gameObject.SetActive(value: true);
			fadeBlackOverlay.color = Color.black;
			fadeBlackOverlay.transform.SetAsLastSibling();
		}
		logoImage.SetActive(value: true);
		logoImage.transform.localScale = Vector3.zero;
		logoInitialCenterPos = logoImage.transform.localPosition;
		leftGrayBar.localPosition = new Vector3(leftBarOutOffset, 0f, 0f);
		rightGrayBar.localPosition = new Vector3(rightBarOutOffset, 0f, 0f);
		StartCoroutine(ExecuteMainMenuFlowRoutine());
	}

	private IEnumerator ExecuteMainMenuFlowRoutine()
	{
		float elapsed3 = 0f;
		StartCoroutine(FadeOutBlackRoutine());
		if (logoShootDuration <= 0f)
		{
			logoShootDuration = 0.1f;
		}
		while (elapsed3 < logoShootDuration)
		{
			elapsed3 += Mathf.Min(Time.deltaTime, 0.1f);
			float num = elapsed3 / logoShootDuration;
			float num2 = 1.25f;
			num -= 1f;
			float num3 = num * num * ((num2 + 1f) * num + num2) + 1f;
			if (logoImage != null)
			{
				logoImage.transform.localScale = new Vector3(num3, num3, 1f);
			}
			yield return null;
		}
		if (logoImage != null)
		{
			logoImage.transform.localScale = Vector3.one;
		}
		yield return new WaitForSeconds(logoStayDuration);
		elapsed3 = 0f;
		if (barsCombineDuration <= 0f)
		{
			barsCombineDuration = 0.1f;
		}
		while (elapsed3 < barsCombineDuration)
		{
			elapsed3 += Mathf.Min(Time.deltaTime, 0.1f);
			float t = Mathf.SmoothStep(0f, 1f, elapsed3 / barsCombineDuration);
			if (leftGrayBar == null || rightGrayBar == null)
			{
				yield break;
			}
			leftGrayBar.localPosition = new Vector3(Mathf.Lerp(leftBarOutOffset, 0f, t), 0f, 0f);
			rightGrayBar.localPosition = new Vector3(Mathf.Lerp(rightBarOutOffset, 0f, t), 0f, 0f);
			yield return null;
		}
		elapsed3 = 0f;
		if (barsSplitDuration <= 0f)
		{
			barsSplitDuration = 0.1f;
		}
		if (logoMoveDuration <= 0f)
		{
			logoMoveDuration = 0.1f;
		}
		while (elapsed3 < barsSplitDuration)
		{
			elapsed3 += Mathf.Min(Time.deltaTime, 0.1f);
			float t2 = Mathf.SmoothStep(0f, 1f, elapsed3 / barsSplitDuration);
			if (leftGrayBar == null || rightGrayBar == null || logoImage == null)
			{
				break;
			}
			leftGrayBar.localPosition = new Vector3(Mathf.Lerp(0f, leftBarOutOffset, t2), 0f, 0f);
			rightGrayBar.localPosition = new Vector3(Mathf.Lerp(0f, rightBarStayOffset, t2), 0f, 0f);
			float t3 = Mathf.Clamp01(elapsed3 / logoMoveDuration);
			float t4 = Mathf.SmoothStep(0f, 1f, t3);
			logoImage.transform.localPosition = Vector3.Lerp(logoInitialCenterPos, logoRightTargetPosition, t4);
			yield return null;
		}
	}

	private IEnumerator FadeOutBlackRoutine()
	{
		float elapsed = 0f;
		if (fadeOutBlackDuration <= 0f)
		{
			fadeOutBlackDuration = 0.1f;
		}
		while (elapsed < fadeOutBlackDuration)
		{
			elapsed += Mathf.Min(Time.deltaTime, 0.1f);
			float a = Mathf.Lerp(1f, 0f, elapsed / fadeOutBlackDuration);
			if (fadeBlackOverlay != null)
			{
				fadeBlackOverlay.color = new Color(0f, 0f, 0f, a);
			}
			yield return null;
		}
		if (fadeBlackOverlay != null)
		{
			fadeBlackOverlay.gameObject.SetActive(value: false);
		}
	}

    /// <summary>
    /// 开始游戏：先播放按钮反馈，再在入场动画完成后切换至视频场景。
    /// </summary>
	public void OnClickStartGame()
	{
		if (!isTransitioning)
		{
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
			}
			StartCoroutine(JumpToVideoSceneRoutine());
		}
	}

	private IEnumerator JumpToVideoSceneRoutine()
	{
		isTransitioning = true;
		if (fadeBlackOverlay != null)
		{
			fadeBlackOverlay.gameObject.SetActive(value: true);
			fadeBlackOverlay.transform.SetAsLastSibling();
			float elapsed = 0f;
			float fadeInDuration = 0.5f;
			while (elapsed < fadeInDuration)
			{
				elapsed += Mathf.Min(Time.deltaTime, 0.1f);
				fadeBlackOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 1f, elapsed / fadeInDuration));
				yield return null;
			}
		}
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayBGM(AudioManager.Instance.battleBGM);
		}
		SceneManager.LoadScene(introVideoSceneName);
	}

    /// <summary>退出应用。</summary>
	public void OnClickExitGame()
	{
		if (!isTransitioning)
		{
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
			}
			Application.Quit();
		}
	}
}
