using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色进化演出控制器，通过时缓、镜头缩放、粒子和白屏过渡强调阶段变化。
/// </summary>
/// <remarks>
/// 演出过程使用 unscaledDeltaTime，并在极小 timeScale 下运行；
/// 因此不会因为游戏时缓而拖长动画，也不会让 UI 动画与粒子停住。
/// </remarks>
public class PlayerEvolutionFX : MonoBehaviour
{
	[Header("白屏过渡 UI")]
	public Image whiteScreen;

	[Header("进化粒子")]
	public GameObject evolutionParticlePrefab;

	[Header("镜头控制")]
	private Camera mainCamera;

	private float originalCamSize;

	public float zoomInSize = 4f;

	[Header("虚拟相机（可选）")]
	public GameObject virtualCameraObject;

	[Header("粒子位置")]
	public float particleOffsetY = 1.2f;

	private bool isEvolving;

	private void Start()
	{
		mainCamera = Camera.main;
		if (mainCamera != null)
		{
			originalCamSize = mainCamera.orthographicSize;
		}
		if (whiteScreen != null)
		{
			whiteScreen.gameObject.SetActive(value: true);
			SetWhiteScreenAlpha(0f);
		}
	}

    /// <summary>
    /// 播放一次进化演出，并在白屏遮蔽角色时切换外观。
    /// </summary>
    /// <param name="changeSkinCallback">外观切换回调。</param>
	public void PlayEvolutionAnimation(Action changeSkinCallback)
	{
		if (!isEvolving)
		{
			StartCoroutine(EvolutionRoutine(changeSkinCallback));
		}
	}

	private void SetCameraSize(float size)
	{
		mainCamera = Camera.main;
		if (mainCamera != null)
		{
			mainCamera.orthographicSize = size;
		}
		if (virtualCameraObject != null)
		{
			virtualCameraObject.SendMessage("set_Lens", size, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void SetWhiteScreenAlpha(float alpha)
	{
		if (!(whiteScreen == null))
		{
			Color color = whiteScreen.color;
			color.a = Mathf.Clamp01(alpha);
			whiteScreen.color = color;
		}
	}

	private IEnumerator EvolutionRoutine(Action changeSkinCallback)
	{
		isEvolving = true;
		mainCamera = Camera.main;
		if (mainCamera != null)
		{
			originalCamSize = mainCamera.orthographicSize;
		}
		if (virtualCameraObject != null)
		{
			virtualCameraObject.SetActive(value: false);
		}
		Time.timeScale = 0.02f;
		float currentCamSize = ((mainCamera != null) ? mainCamera.orthographicSize : originalCamSize);
		GameObject spawnFX = null;
		if (evolutionParticlePrefab != null)
		{
			Vector3 position = base.transform.position + new Vector3(0f, particleOffsetY, 0f);
			spawnFX = UnityEngine.Object.Instantiate(evolutionParticlePrefab, position, Quaternion.identity);
			ParticleSystem.MainModule main = spawnFX.GetComponent<ParticleSystem>().main;
			main.useUnscaledTime = true;
		}
		float elapsed3 = 0f;
		float zoomDuration = 2f;
		while (elapsed3 < zoomDuration)
		{
			elapsed3 += Time.unscaledDeltaTime;
			float t = elapsed3 / zoomDuration;
			float cameraSize = Mathf.Lerp(currentCamSize, zoomInSize, t);
			SetCameraSize(cameraSize);
			yield return null;
		}
		elapsed3 = 0f;
		float flashInDuration = 0.15f;
		while (elapsed3 < flashInDuration)
		{
			elapsed3 += Time.unscaledDeltaTime;
			float whiteScreenAlpha = elapsed3 / flashInDuration;
			SetWhiteScreenAlpha(whiteScreenAlpha);
			yield return null;
		}
		SetWhiteScreenAlpha(1f);
		changeSkinCallback?.Invoke();
		yield return new WaitForSecondsRealtime(0.2f);
		if (spawnFX != null)
		{
			UnityEngine.Object.Destroy(spawnFX);
		}
		elapsed3 = 0f;
		float fadeOutDuration = 0.5f;
		while (elapsed3 < fadeOutDuration)
		{
			elapsed3 += Time.unscaledDeltaTime;
			float num = elapsed3 / fadeOutDuration;
			SetWhiteScreenAlpha(1f - num);
			float cameraSize2 = Mathf.Lerp(zoomInSize, originalCamSize, num);
			SetCameraSize(cameraSize2);
			yield return null;
		}
		SetWhiteScreenAlpha(0f);
		SetCameraSize(originalCamSize);
		if (virtualCameraObject != null)
		{
			virtualCameraObject.SetActive(value: true);
		}
		Time.timeScale = 1f;
		isEvolving = false;
	}
}
