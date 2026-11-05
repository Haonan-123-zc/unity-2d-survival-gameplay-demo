using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 玩家死亡演出导演，负责暗场、字幕缩放、返回提示和场景切换。
/// </summary>
/// <remarks>
/// 死亡流程由 isPlayerDead 保证只触发一次；暂停后的所有动画使用 unscaledDeltaTime，
/// 避免 Time.timeScale = 0 时 UI 协程停滞。
/// </remarks>
public class DeathSceneDirector : MonoBehaviour
{
	[Header("死亡表现 UI")]
	public Image deathImage;

	public Image grayFilter;

	[Header("背景滤镜")]
	[Range(0f, 1f)]
	public float darkFilterAlpha = 0.8f;

	[Header("失败字幕缩放动画")]
	public float maxScaleX = 1.8f;

	public float maxScaleY = 1.2f;

	[Space(5f)]
	public float zoomInDuration = 0.4f;

	[Header("延时交互")]
	[Tooltip("大字完全砸完后，静止留白多少秒才开始淡入‘点击左键返回’提示")]
	public float waitTimeBeforePrompt = 1f;

	[Tooltip("提示文字从隐形到完全显现所需的渐变时间（秒）")]
	public float promptFadeInDuration = 0.5f;

	[Header("底部提示 UI")]
	[Tooltip("把你新建的“点击左键返回主菜单”的文字物体（Text/TMP）拖到这里")]
	public GameObject returnPromptText;

	[Header("返回场景")]
	public string mainMenuSceneName = "MainMenu";

	private bool isPlayerDead;

	private bool canClickToReturn;

	private CanvasGroup promptCanvasGroup;

	private void Start()
	{
		if (deathImage != null)
		{
			deathImage.gameObject.SetActive(value: false);
		}
		if (grayFilter != null)
		{
			grayFilter.gameObject.SetActive(value: false);
		}
		if (returnPromptText != null)
		{
			promptCanvasGroup = returnPromptText.GetComponent<CanvasGroup>();
			if (promptCanvasGroup == null)
			{
				promptCanvasGroup = returnPromptText.AddComponent<CanvasGroup>();
			}
			promptCanvasGroup.alpha = 0f;
			returnPromptText.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (canClickToReturn && Input.GetMouseButtonDown(0))
		{
			ReturnToMainMenuAction();
		}
	}

    /// <summary>
    /// 启动一次玩家死亡演出；重复调用会被忽略。
    /// </summary>
	public void TriggerPlayerDeathFlow()
	{
		if (!isPlayerDead)
		{
			isPlayerDead = true;
			StartCoroutine(ExecuteDeathSequenceRoutine());
		}
	}

	private IEnumerator ExecuteDeathSequenceRoutine()
	{
		Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(1f);
		if (grayFilter != null)
		{
			grayFilter.gameObject.SetActive(value: true);
			grayFilter.color = new Color(0.05f, 0.05f, 0.05f, darkFilterAlpha);
			grayFilter.transform.SetAsLastSibling();
			Canvas.ForceUpdateCanvases();
		}
		yield return new WaitForSecondsRealtime(0.05f);
		if (deathImage != null)
		{
			RectTransform rectTransform = deathImage.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				deathImage.transform.SetAsLastSibling();
				rectTransform.localScale = Vector3.zero;
				deathImage.gameObject.SetActive(value: true);
				float elapsed2 = 0f;
				while (elapsed2 < zoomInDuration)
				{
					elapsed2 += Time.unscaledDeltaTime;
					float t = elapsed2 / zoomInDuration;
					float t2 = Mathf.SmoothStep(0f, 1f, t);
					float x = Mathf.Lerp(0f, maxScaleX, t2);
					float y = Mathf.Lerp(0f, maxScaleY, t2);
					rectTransform.localScale = new Vector3(x, y, 1f);
					yield return null;
				}
				rectTransform.localScale = new Vector3(maxScaleX, maxScaleY, 1f);
				Canvas.ForceUpdateCanvases();
			}
		}
		yield return new WaitForSecondsRealtime(waitTimeBeforePrompt);
		if (returnPromptText != null && promptCanvasGroup != null)
		{
			promptCanvasGroup.alpha = 0f;
			returnPromptText.SetActive(value: true);
			returnPromptText.transform.SetAsLastSibling();
			float elapsed2 = 0f;
			if (promptFadeInDuration <= 0f)
			{
				promptFadeInDuration = 0.1f;
			}
			while (elapsed2 < promptFadeInDuration)
			{
				elapsed2 += Time.unscaledDeltaTime;
				float alpha = Mathf.Clamp01(elapsed2 / promptFadeInDuration);
				promptCanvasGroup.alpha = alpha;
				yield return null;
			}
			promptCanvasGroup.alpha = 1f;
			Canvas.ForceUpdateCanvases();
		}
		canClickToReturn = true;
	}

	private void ReturnToMainMenuAction()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(mainMenuSceneName);
	}
}
