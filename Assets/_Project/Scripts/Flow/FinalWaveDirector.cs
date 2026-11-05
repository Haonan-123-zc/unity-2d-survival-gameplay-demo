using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 终局关卡导演，负责停止普通刷怪、生成最后一波、检测清场并播放胜利流程。
/// </summary>
/// <remarks>
/// 流程使用多个布尔状态锁描述：普通刷怪停止、终波开始、生成完成和胜利结算。
/// 这样既能避免协程重入，也能在没有引入完整状态机框架时保持流程可读。
/// 胜利演出使用 unscaledDeltaTime，因为结算时游戏会被暂停。
/// </remarks>
public class FinalWaveDirector : MonoBehaviour
{
	[Header("关卡进度")]
	public int targetLevel = 25;

	public string playerObjectName = "Player";

	[Header("终局波次怪物")]
	public GameObject meleeMonsterPrefab;

	public GameObject ghostPrefab;

	public GameObject bossPrefab;

	[Header("字幕与滤镜 UI")]
	public Image finalWaveImage;

	public Image victoryImage;

	public Image grayFilter;

	[Header("背景滤镜")]
	[Range(0f, 1f)]
	public float darkFilterAlpha = 0.8f;

	[Header("最后一波字幕动画")]
	public float maxScaleX = 1.5f;

	public float maxScaleY = 1.1f;

	[Space(5f)]
	public float zoomInDuration = 0.4f;

	public float stayDuration = 1f;

	public float zoomOutDuration = 0.4f;

	[Header("胜利字幕尺寸")]
	public float victoryMaxScaleX = 1.8f;

	public float victoryMaxScaleY = 1.2f;

	[Header("胜利闪烁效果")]
	public float flashSpeed = 6f;

	[Range(0f, 1f)]
	public float minFlashAlpha = 0.4f;

	[Header("战前节奏")]
	public float waveSpawnDelay = 1.5f;

	[Header("屏幕外生成边距")]
	public float spawnMargin = 2f;

	[Header("胜利交互时序")]
	public float waitTimeBeforePrompt = 1.5f;

	public float promptFadeInDuration = 0.5f;

	[Header("底部提示 UI")]
	public GameObject victoryReturnPromptText;

	[Header("返回场景")]
	public string mainMenuSceneName = "MainMenu";

	private bool isSpawnerStopped;

	private bool isFinalWaveTriggered;

	private bool isSpawnComplete;

	private bool isGameCleared;

	private bool canClickToReturn;

	private GameObject playerObj;

	private EnemySpawner oldSpawnerComponent;

	private Camera mainCam;

	private List<GameObject> finalWaveEnemies = new List<GameObject>();

	private Color originalVictoryColor;

	private CanvasGroup promptCanvasGroup;

	private void Start()
	{
		if (finalWaveImage != null)
		{
			finalWaveImage.gameObject.SetActive(value: false);
		}
		if (victoryImage != null)
		{
			victoryImage.gameObject.SetActive(value: false);
		}
		if (grayFilter != null)
		{
			grayFilter.gameObject.SetActive(value: false);
		}
		if (victoryImage != null)
		{
			originalVictoryColor = victoryImage.color;
		}
		if (victoryReturnPromptText != null)
		{
			promptCanvasGroup = victoryReturnPromptText.GetComponent<CanvasGroup>();
			if (promptCanvasGroup == null)
			{
				promptCanvasGroup = victoryReturnPromptText.AddComponent<CanvasGroup>();
			}
			promptCanvasGroup.alpha = 0f;
			victoryReturnPromptText.SetActive(value: false);
		}
		playerObj = GameObject.Find(playerObjectName);
		oldSpawnerComponent = Object.FindAnyObjectByType<EnemySpawner>();
		mainCam = Camera.main;
	}

	private void Update()
	{
		if (canClickToReturn && Input.GetMouseButtonDown(0))
		{
			ReturnToMainMenuAction();
		}
#if UNITY_EDITOR
		else if (Input.GetKeyDown(KeyCode.Return))
		{
            // 仅编辑器保留快捷通关，避免调试键进入正式构建。
			WinGame();
		}
#endif
		else
		{
			if (isGameCleared || playerObj == null)
			{
				return;
			}
			PlayerStats component = playerObj.GetComponent<PlayerStats>();
        // 达到目标等级后先冻结普通刷怪，再由终波导演接管关卡节奏。
			if (component != null)
			{
				if (component.currentLevel >= targetLevel)
				{
					component.currentLevel = targetLevel;
					component.expToNextLevel = 100000000;
					component.currentExp = 0;
				}
				if (!isSpawnerStopped && component.currentLevel >= targetLevel)
				{
					isSpawnerStopped = true;
					if (oldSpawnerComponent != null)
					{
						oldSpawnerComponent.enabled = false;
					}
					StartCoroutine(ClearNormalAndTriggerFinalWave());
				}
			}
			EnemyController[] array = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
			for (int i = 0; i < array.Length; i++)
			{
				Rigidbody2D component2 = array[i].GetComponent<Rigidbody2D>();
				if (component2 != null && component2.linearVelocity.magnitude > 10f)
				{
					component2.linearVelocity = Vector2.zero;
					component2.angularVelocity = 0f;
				}
			}
			if (!isFinalWaveTriggered || !isSpawnComplete || isGameCleared)
			{
				return;
			}
			finalWaveEnemies.RemoveAll((GameObject item) => item == null);
			int num = 0;
			foreach (GameObject finalWaveEnemy in finalWaveEnemies)
			{
				if (finalWaveEnemy != null && finalWaveEnemy.activeInHierarchy)
				{
					num++;
				}
			}
			if (finalWaveEnemies.Count == 0 || num == 0)
			{
				WinGame();
			}
		}
	}

	private IEnumerator ClearNormalAndTriggerFinalWave()
	{
		if (isFinalWaveTriggered)
		{
			yield break;
		}
		isFinalWaveTriggered = true;
		isSpawnComplete = false;
        // 字幕动画结束前不生成敌人，给玩家清楚的阶段切换信号。
		yield return new WaitForSeconds(3f);
		if (finalWaveImage != null)
		{
			RectTransform rectTransform = finalWaveImage.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				rectTransform.localScale = Vector3.zero;
				finalWaveImage.gameObject.SetActive(value: true);
				float elapsed2 = 0f;
				while (elapsed2 < zoomInDuration)
				{
					elapsed2 += Time.deltaTime;
					float t = elapsed2 / zoomInDuration;
					float t2 = Mathf.SmoothStep(0f, 1f, t);
					rectTransform.localScale = new Vector3(Mathf.Lerp(0f, maxScaleX, t2), Mathf.Lerp(0f, maxScaleY, t2), 1f);
					yield return null;
				}
				rectTransform.localScale = new Vector3(maxScaleX, maxScaleY, 1f);
				yield return new WaitForSeconds(stayDuration);
				elapsed2 = 0f;
				while (elapsed2 < zoomOutDuration)
				{
					elapsed2 += Time.deltaTime;
					float t3 = elapsed2 / zoomOutDuration;
					float t4 = Mathf.SmoothStep(0f, 1f, t3);
					rectTransform.localScale = new Vector3(Mathf.Lerp(maxScaleX, 0f, t4), Mathf.Lerp(maxScaleY, 0f, t4), 1f);
					yield return null;
				}
				rectTransform.localScale = Vector3.zero;
				finalWaveImage.gameObject.SetActive(value: false);
			}
		}
		yield return new WaitForSeconds(waveSpawnDelay);
		if (playerObj != null && mainCam != null)
		{
            // 以相机可见区域为基准计算边线，再向外留出 spawnMargin，
            // 保证终波敌人从画面外进入而不是突然出现在玩家身边。
			float num = 2f * mainCam.orthographicSize;
			float num2 = num * mainCam.aspect;
			float num3 = mainCam.transform.position.x - num2 / 2f;
			float num4 = mainCam.transform.position.x + num2 / 2f;
			float num5 = mainCam.transform.position.y - num / 2f;
			float num6 = mainCam.transform.position.y + num / 2f;
			SpawnFinalEnemy(bossPrefab, new Vector3(mainCam.transform.position.x, num6 + spawnMargin, 0f));
			for (int i = 0; i < 3; i++)
			{
				SpawnFinalEnemy(ghostPrefab, new Vector3(Random.Range(num3, num4), num6 + spawnMargin, 0f));
				SpawnFinalEnemy(ghostPrefab, new Vector3(Random.Range(num3, num4), num5 - spawnMargin, 0f));
				SpawnFinalEnemy(ghostPrefab, new Vector3(num3 - spawnMargin, Random.Range(num5, num6), 0f));
				SpawnFinalEnemy(ghostPrefab, new Vector3(num4 + spawnMargin, Random.Range(num5, num6), 0f));
			}
			int num7 = 18;
			for (int j = 0; j < num7; j++)
			{
				SpawnFinalEnemy(meleeMonsterPrefab, new Vector3(Random.Range(num3, num4), num6 + spawnMargin, 0f));
				SpawnFinalEnemy(meleeMonsterPrefab, new Vector3(Random.Range(num3, num4), num5 - spawnMargin, 0f));
				SpawnFinalEnemy(meleeMonsterPrefab, new Vector3(num3 - spawnMargin, Random.Range(num5, num6), 0f));
				SpawnFinalEnemy(meleeMonsterPrefab, new Vector3(num4 + spawnMargin, Random.Range(num5, num6), 0f));
			}
		}
		isSpawnComplete = true;
	}

	private void SpawnFinalEnemy(GameObject prefab, Vector3 position)
	{
		if (!(prefab == null))
		{
			GameObject gameObject = Object.Instantiate(prefab, position, Quaternion.identity);
			if (gameObject != null)
			{
				finalWaveEnemies.Add(gameObject);
			}
		}
	}

	private void WinGame()
	{
		if (!isGameCleared)
		{
			isGameCleared = true;
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.StopBGMWithFade();
			}
			StartCoroutine(ExecuteVictoryFlowRoutine());
		}
	}

	private IEnumerator ExecuteVictoryFlowRoutine()
	{
        // 此时游戏已经暂停，因此所有 UI 等待与插值必须使用真实时间。
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
		if (victoryImage != null)
		{
			victoryImage.transform.SetAsLastSibling();
			RectTransform rectTransform = victoryImage.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				rectTransform.localScale = Vector3.zero;
				victoryImage.gameObject.SetActive(value: true);
				if (AudioManager.Instance != null)
				{
					AudioManager.Instance.PlaySFX(AudioManager.Instance.victorySound);
				}
				float elapsed2 = 0f;
				while (elapsed2 < zoomInDuration)
				{
					elapsed2 += Time.unscaledDeltaTime;
					float t = elapsed2 / zoomInDuration;
					float t2 = Mathf.SmoothStep(0f, 1f, t);
					float x = Mathf.Lerp(0f, victoryMaxScaleX, t2);
					float y = Mathf.Lerp(0f, victoryMaxScaleY, t2);
					rectTransform.localScale = new Vector3(x, y, 1f);
					yield return null;
				}
				rectTransform.localScale = new Vector3(victoryMaxScaleX, victoryMaxScaleY, 1f);
				Canvas.ForceUpdateCanvases();
				StartCoroutine(FlashVictoryTextRoutine());
			}
		}
		yield return new WaitForSecondsRealtime(waitTimeBeforePrompt);
		if (victoryReturnPromptText != null && promptCanvasGroup != null)
		{
			promptCanvasGroup.alpha = 0f;
			victoryReturnPromptText.SetActive(value: true);
			victoryReturnPromptText.transform.SetAsLastSibling();
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

	private IEnumerator FlashVictoryTextRoutine()
	{
		float runningTime = 0f;
		while (true)
		{
			runningTime += Time.unscaledDeltaTime * flashSpeed;
			float t = (Mathf.Sin(runningTime) + 1f) / 2f;
			float a = Mathf.Lerp(minFlashAlpha, 1f, t);
			if (victoryImage != null)
			{
				victoryImage.color = new Color(originalVictoryColor.r, originalVictoryColor.g, originalVictoryColor.b, a);
			}
			yield return null;
		}
	}

	private void ReturnToMainMenuAction()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
		}
		Time.timeScale = 1f;
		SceneManager.LoadScene(mainMenuSceneName);
	}
}
