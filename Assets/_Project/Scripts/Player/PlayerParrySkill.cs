using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家格挡技能，负责无敌窗口、冷却时间和护盾 UI 状态。
/// </summary>
/// <remarks>
/// 技能状态只在本组件中推进，生命值组件只读取 isParryInvincible；
/// 这样伤害判断和技能冷却互不侵入，便于后续替换为 ScriptableObject 技能配置。
/// </remarks>
public class PlayerParrySkill : MonoBehaviour
{
	[Header("格挡参数")]
	public KeyCode parryKey = KeyCode.E;

	public float invincibleDuration = 0.5f;

	public float cooldownDuration = 3f;

	[Header("视觉与 UI 组件")]
	public GameObject quantumShieldObject;

	public Image cooldownDarkMask;

	public Color parryFlashColor = new Color(1f, 0.92f, 0.016f, 0.6f);

	private PlayerHealth playerHealth;

	private HitFlash hitFlash;

	private bool isCooldown;

	private float cooldownTimer;

	private void Start()
	{
		playerHealth = GetComponent<PlayerHealth>();
		hitFlash = GetComponent<HitFlash>();
		if (quantumShieldObject != null)
		{
			quantumShieldObject.SetActive(value: false);
		}
		if (cooldownDarkMask != null)
		{
			cooldownDarkMask.fillAmount = 0f;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(parryKey) && !isCooldown && Time.timeScale > 0f)
		{
			StartCoroutine(TriggerParryFlow());
		}
		UpdateCooldownUI();
	}

	private IEnumerator TriggerParryFlow()
	{
		isCooldown = true;
		cooldownTimer = cooldownDuration;
		if (playerHealth != null)
		{
			playerHealth.isParryInvincible = true;
		}
		if (quantumShieldObject != null)
		{
			quantumShieldObject.SetActive(value: true);
		}
		if (hitFlash != null)
		{
			hitFlash.Flash(parryFlashColor);
		}
		yield return new WaitForSeconds(invincibleDuration);
		if (playerHealth != null)
		{
			playerHealth.isParryInvincible = false;
		}
		if (quantumShieldObject != null)
		{
			quantumShieldObject.SetActive(value: false);
		}
		while (cooldownTimer > 0f)
		{
			cooldownTimer -= Time.deltaTime;
			yield return null;
		}
		isCooldown = false;
	}

	private void UpdateCooldownUI()
	{
		if (!(cooldownDarkMask == null))
		{
			if (isCooldown)
			{
				cooldownDarkMask.fillAmount = cooldownTimer / cooldownDuration;
			}
			else
			{
				cooldownDarkMask.fillAmount = 0f;
			}
		}
	}
}
