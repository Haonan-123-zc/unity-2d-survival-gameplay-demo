using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 终局传送门表现与场景转换控制器。
/// </summary>
/// <remarks>
/// 传送门完成展开前不会接受触发，且 isTriggered 保证整个流程只执行一次。
/// 转场白屏和场景切换使用同一协程串联，避免出现画面与场景状态不同步。
/// </remarks>
public class VictoryPortal : MonoBehaviour
{
	[Header("白屏转场 UI")]
	[Tooltip("全屏幕的纯白 UI Image（空着不用拖！代码会自动抓取）")]
	public Image whiteScreenFadeImage;

	[Header("传送门展开配置")]
	[Tooltip("传送门完全展开后的最终大小（如果图片原本偏小，可以改成 2 或 3）")]
	public Vector3 finalScale = new Vector3(1.5f, 1.5f, 1f);

	[Tooltip("从一个点到完全展开需要多少秒")]
	public float openDuration = 1f;

	[Tooltip("传送门平时自转的速度（数值越大转得越快，0则不转）")]
	public float rotateSpeed = 45f;

	[Header("转场时间")]
	[Tooltip("碰了传送门之后，白屏完全吞噬世界的持续时间（秒）")]
	public float fadeDuration = 1.5f;

	[Header("下一剧情场景")]
	public string nextStorySceneName = "FatigueScene";

	private bool isTriggered;

	private bool isFullyOpened;

	private void Start()
	{
		if (whiteScreenFadeImage == null)
		{
			GameObject gameObject = GameObject.Find("WhiteFadeImage");
			if (gameObject != null)
			{
				whiteScreenFadeImage = gameObject.GetComponent<Image>();
			}
			if (whiteScreenFadeImage == null)
			{
				whiteScreenFadeImage = Object.FindFirstObjectByType<Image>();
			}
		}
		if (whiteScreenFadeImage != null)
		{
			Color color = whiteScreenFadeImage.color;
			color.a = 0f;
			whiteScreenFadeImage.color = color;
		}
		base.transform.localScale = Vector3.zero;
		StartCoroutine(AnimatePortalOpening());
	}

	private void Update()
	{
		if (rotateSpeed != 0f)
		{
			base.transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
		}
	}

	private IEnumerator AnimatePortalOpening()
	{
		float t = 0f;
		while (t < openDuration)
		{
			t += Time.deltaTime;
			float t2 = t / openDuration;
			base.transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, t2);
			yield return null;
		}
		base.transform.localScale = finalScale;
		isFullyOpened = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (isFullyOpened && !isTriggered && collision.CompareTag("Player"))
		{
			StartCoroutine(StartPortalVictorySequence(collision.gameObject));
		}
	}

	private IEnumerator StartPortalVictorySequence(GameObject player)
	{
		isTriggered = true;
		Rigidbody2D component = player.GetComponent<Rigidbody2D>();
		if (component != null)
		{
			component.linearVelocity = Vector2.zero;
		}
		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			if (whiteScreenFadeImage != null)
			{
				Color color = whiteScreenFadeImage.color;
				color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
				whiteScreenFadeImage.color = color;
			}
			yield return null;
		}
		if (whiteScreenFadeImage != null)
		{
			whiteScreenFadeImage.color = new Color(1f, 1f, 1f, 1f);
		}
		yield return new WaitForSeconds(0.3f);
		SceneManager.LoadScene(nextStorySceneName);
	}
}
