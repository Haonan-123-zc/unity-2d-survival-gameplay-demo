using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 跨场景音频服务，统一管理 BGM 切换、淡入淡出、SFX 音量与高频音效限流。
/// </summary>
/// <remarks>
/// 单例只在 Awake 中接管实例，并在场景切换时保持；重复实例立即销毁。
/// BGM 使用协程淡出后再更换 Clip，避免背景音乐硬切导致爆音。
/// 高频命中音效通过时间窗口限流，防止一帧内多个敌人受击时叠加失真。
/// </remarks>
public class AudioManager : MonoBehaviour
{
	[Header("核心音频源")]
	public AudioSource bgmSource;

	public AudioSource sfxSource;

	[Header("背景音乐（BGM）")]
	public AudioClip mainMenuBGM;

	public AudioClip battleBGM;

	public AudioClip evolvedBattleBGM;

	[Header("基础音效（SFX）")]
	public AudioClip buttonClickSound;

	public AudioClip magnetSound;

	public AudioClip healthPotSound;

	public AudioClip gemPickupSound;

	[Header("战斗打击与进化音效")]
	public AudioClip hitSound;

	public AudioClip levelUpSound;

	public AudioClip evolveSound;

	[Header("怪物死亡音效")]
	public AudioClip zombieDeathSound;

	public AudioClip impDeathSound;

	public AudioClip skeletonDeathSound;

	public AudioClip ghostDeathSound;

	[Header("结算音效")]
	public AudioClip victorySound;

	public AudioClip gameOverSound;

	[Header("独立音量")]
	[Range(0f, 1f)]
	public float mainMenuBgmVolume = 0.5f;

	[Range(0f, 1f)]
	public float battleBgmVolume = 0.6f;

	[Range(0f, 1f)]
	public float evolvedBattleBgmVolume = 0.65f;

	[Space(5f)]
	[Range(0f, 1f)]
	public float hitVolume = 0.25f;

	[Range(0f, 1f)]
	public float buttonClickVolume = 0.8f;

	[Range(0f, 1f)]
	public float magnetVolume = 0.8f;

	[Range(0f, 1f)]
	public float healthPotVolume = 0.8f;

	[Range(0f, 1f)]
	public float gemPickupVolume = 0.7f;

	[Range(0f, 1f)]
	public float levelUpVolume = 0.8f;

	[Range(0f, 1f)]
	public float evolveVolume = 0.9f;

	[Space(5f)]
	[Range(0f, 1f)]
	public float zombieDeathVolume = 0.5f;

	[Range(0f, 1f)]
	public float impDeathVolume = 0.5f;

	[Range(0f, 1f)]
	public float skeletonDeathVolume = 0.5f;

	[Range(0f, 1f)]
	public float ghostDeathVolume = 0.5f;

	[Space(5f)]
	[Range(0f, 1f)]
	public float victoryVolume = 0.9f;

	[Range(0f, 1f)]
	public float gameOverVolume = 0.9f;

	[Header("渐入淡出配置")]
	public float fadeDuration = 1.2f;

	[Header("音效限流阈值（秒）")]
	public float hitCooldown = 0.06f;

	public float deathCooldown = 0.05f;

	private float lastHitTime;

	private float lastZombieDeathTime;

	private float lastImpDeathTime;

	private float lastSkeletonDeathTime;

	private float lastGhostDeathTime;

	private Coroutine fadeCoroutine;

	public static AudioManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			if (bgmSource == null)
			{
				bgmSource = base.gameObject.AddComponent<AudioSource>();
			}
			if (sfxSource == null)
			{
				sfxSource = base.gameObject.AddComponent<AudioSource>();
			}
			bgmSource.loop = true;
			sfxSource.volume = 1f;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

    /// <summary>
    /// 根据场景切换背景音乐；视频场景会主动让出音频通道。
    /// </summary>
    /// <remarks>
    /// 当前场景名与 Build Settings 中的名称绑定，移动或重命名场景时需同步更新。
    /// </remarks>
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "IntroVideoScene" || scene.name == "VideoScene")
		{
			StopBGMWithFade();
		}
		else if (scene.name == "MainMenu" || scene.buildIndex == 0)
		{
			PlayBGM(mainMenuBGM);
		}
		else if (scene.name == "SampleScene" || scene.buildIndex == 1)
		{
			PlayBGM(battleBGM);
		}
	}

    /// <summary>
    /// 平滑切换到指定背景音乐。
    /// </summary>
    /// <param name="clip">目标音乐；为空时忽略。</param>
	public void PlayBGM(AudioClip clip)
	{
		if (!(clip == null) && (!(bgmSource.clip == clip) || !bgmSource.isPlaying))
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			fadeCoroutine = StartCoroutine(FadeBGMRoutine(clip));
		}
	}

    /// <summary>切换到角色进化后的战斗音乐。</summary>
	public void SwitchToEvolvedBGM()
	{
		PlayBGM(evolvedBattleBGM);
	}

	private IEnumerator FadeBGMRoutine(AudioClip newClip)
	{
		float timer2 = 0f;
		if (bgmSource.clip != null && bgmSource.isPlaying)
		{
			float startVolume = bgmSource.volume;
			while (timer2 < fadeDuration / 2f)
			{
				timer2 += Time.unscaledDeltaTime;
				bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer2 / (fadeDuration / 2f));
				yield return null;
			}
		}
		bgmSource.Stop();
		bgmSource.clip = newClip;
		bgmSource.volume = 0f;
		bgmSource.Play();
		float targetVolume = battleBgmVolume;
		if (newClip == mainMenuBGM)
		{
			targetVolume = mainMenuBgmVolume;
		}
		else if (newClip == evolvedBattleBGM)
		{
			targetVolume = evolvedBattleBgmVolume;
		}
		timer2 = 0f;
		while (timer2 < fadeDuration / 2f)
		{
			timer2 += Time.unscaledDeltaTime;
			bgmSource.volume = Mathf.Lerp(0f, targetVolume, timer2 / (fadeDuration / 2f));
			yield return null;
		}
		bgmSource.volume = targetVolume;
		fadeCoroutine = null;
	}

    /// <summary>
    /// 播放一次音效，并根据音效类别应用独立音量与限流规则。
    /// </summary>
    /// <param name="clip">音效片段；为空时忽略。</param>
	public void PlaySFX(AudioClip clip)
	{
		if (clip == null)
		{
			return;
		}
		float volumeScale = 1f;
		if (clip == hitSound)
		{
			if (Time.time - lastHitTime < hitCooldown)
			{
				return;
			}
			lastHitTime = Time.time;
			volumeScale = hitVolume;
		}
		else if (clip == zombieDeathSound)
		{
			if (Time.time - lastZombieDeathTime < deathCooldown)
			{
				return;
			}
			lastZombieDeathTime = Time.time;
			volumeScale = zombieDeathVolume;
		}
		else if (clip == impDeathSound)
		{
			if (Time.time - lastImpDeathTime < deathCooldown)
			{
				return;
			}
			lastImpDeathTime = Time.time;
			volumeScale = impDeathVolume;
		}
		else if (clip == skeletonDeathSound)
		{
			if (Time.time - lastSkeletonDeathTime < deathCooldown)
			{
				return;
			}
			lastSkeletonDeathTime = Time.time;
			volumeScale = skeletonDeathVolume;
		}
		else if (clip == ghostDeathSound)
		{
			if (Time.time - lastGhostDeathTime < deathCooldown)
			{
				return;
			}
			lastGhostDeathTime = Time.time;
			volumeScale = ghostDeathVolume;
		}
		else if (clip == buttonClickSound)
		{
			volumeScale = buttonClickVolume;
		}
		else if (clip == magnetSound)
		{
			volumeScale = magnetVolume;
		}
		else if (clip == healthPotSound)
		{
			volumeScale = healthPotVolume;
		}
		else if (clip == gemPickupSound)
		{
			volumeScale = gemPickupVolume;
		}
		else if (clip == levelUpSound)
		{
			volumeScale = levelUpVolume;
		}
		else if (clip == evolveSound)
		{
			volumeScale = evolveVolume;
		}
		else if (clip == victorySound)
		{
			volumeScale = victoryVolume;
		}
		else if (clip == gameOverSound)
		{
			volumeScale = gameOverVolume;
		}
		sfxSource.PlayOneShot(clip, volumeScale);
	}

	private void Update()
	{
		if (fadeCoroutine == null && bgmSource.isPlaying)
		{
			if (bgmSource.clip == mainMenuBGM)
			{
				bgmSource.volume = mainMenuBgmVolume;
			}
			else if (bgmSource.clip == evolvedBattleBGM)
			{
				bgmSource.volume = evolvedBattleBgmVolume;
			}
			else
			{
				bgmSource.volume = battleBgmVolume;
			}
		}
	}

    /// <summary>在短暂淡出后停止背景音乐。</summary>
	public void StopBGMWithFade()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeOutBGMRoutineOnly());
	}

	private IEnumerator FadeOutBGMRoutineOnly()
	{
		float timer = 0f;
		float startVolume = bgmSource.volume;
		while (timer < 0.5f)
		{
			timer += Time.unscaledDeltaTime;
			bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / 0.5f);
			yield return null;
		}
		bgmSource.Stop();
		bgmSource.clip = null;
		fadeCoroutine = null;
	}

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
