using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通过监听生命条变化触发屏幕震动、红色边框和命中特效。
/// </summary>
/// <remarks>
/// 震动采用 unscaledDeltaTime，确保暂停或顿帧时 UI 反馈仍能正常结束并恢复。
/// 该组件不直接修改生命值，避免表现层反向控制战斗规则。
/// </remarks>
public class HealthShakeTrigger : MonoBehaviour
{
	[Header("生命值 UI")]
	public Slider hpSlider;

	[Header("屏幕震动参数")]
	public float shakeDuration = 0.15f;

	public float shakeMagnitude = 0.8f;

	[Header("受击视觉效果")]
	public GameObject hitEffectPrefab;

	public Image redBorderImage;

	[Range(0f, 1f)]
	public float maxAlpha = 0.5f;

	private float lastSliderValue;

	private bool isShaking;

	private Coroutine borderCoroutine;

	private void Start()
	{
		if (hpSlider == null)
		{
			hpSlider = GetComponentInChildren<Slider>();
		}
		if (hpSlider != null)
		{
			lastSliderValue = hpSlider.value;
		}
		if (redBorderImage != null)
		{
			Color color = redBorderImage.color;
			color.a = 0f;
			redBorderImage.color = color;
			redBorderImage.gameObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (hpSlider == null)
		{
			return;
		}
		float value = hpSlider.value;
        // 仅在生命值下降时触发反馈，治疗与 UI 初始化不会造成误震屏。
		if (value < lastSliderValue)
		{
			if (!isShaking)
			{
				StartCoroutine(ShaderGlobalShakeCoroutine());
			}
			if (redBorderImage != null)
			{
				if (borderCoroutine != null)
				{
					StopCoroutine(borderCoroutine);
				}
				borderCoroutine = StartCoroutine(RedBorderFlashCoroutine());
			}
			if (hitEffectPrefab != null)
			{
				GameObject obj = Object.Instantiate(hitEffectPrefab, base.transform.position, Quaternion.identity);
				obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
				Object.Destroy(obj, 0.2f);
			}
		}
		lastSliderValue = value;
	}

	private IEnumerator ShaderGlobalShakeCoroutine()
	{
		isShaking = true;
		float elapsed = 0f;
		while (elapsed < shakeDuration)
		{
			float num = Random.Range(-1f, 1f) * shakeMagnitude * 0.05f;
			float num2 = Random.Range(-1f, 1f) * shakeMagnitude * 0.05f;
			Shader.SetGlobalVector("_GlobalScreenShakeOffset", new Vector4(num, num2, 0f, 0f));
			if (Camera.main != null)
			{
				Matrix4x4 projectionMatrix = Camera.main.projectionMatrix;
				projectionMatrix.m02 = num;
				projectionMatrix.m12 = num2;
				Camera.main.projectionMatrix = projectionMatrix;
			}
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		Shader.SetGlobalVector("_GlobalScreenShakeOffset", Vector4.zero);
		if (Camera.main != null)
		{
			Camera.main.ResetProjectionMatrix();
		}
		isShaking = false;
	}

	private IEnumerator RedBorderFlashCoroutine()
	{
		Color tempColor = redBorderImage.color;
		tempColor.a = maxAlpha;
		redBorderImage.color = tempColor;
		float elapsed = 0f;
		while (elapsed < shakeDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			tempColor.a = Mathf.Lerp(maxAlpha, 0f, elapsed / shakeDuration);
			redBorderImage.color = tempColor;
			yield return null;
		}
		tempColor.a = 0f;
		redBorderImage.color = tempColor;
	}

	private void OnDisable()
	{
		Shader.SetGlobalVector("_GlobalScreenShakeOffset", Vector4.zero);
		if (Camera.main != null)
		{
			Camera.main.ResetProjectionMatrix();
		}
	}
}
