using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家生命值、受伤、治疗和死亡流程的唯一入口。
/// </summary>
/// <remarks>
/// 伤害来源只调用 <see cref="TakeDamage"/>，道具只调用 <see cref="Heal"/>，
/// 从而避免通过传入负数伤害来治疗，保证生命值规则的单一来源。
/// </remarks>
public class PlayerHealth : MonoBehaviour
{
	[Header("生命配置")]
	public int maxHealth = 100;

	private int currentHealth;

	[Header("UI 关联")]
	public Slider hpSlider;

	[HideInInspector]
	public bool isParryInvincible;

    /// <summary>当前生命值。只读，外部不能绕过受伤或治疗接口直接修改。</summary>
    public int CurrentHealth => currentHealth;

    /// <summary>角色是否仍可承受伤害。</summary>
    public bool IsAlive => currentHealth > 0;

	private void Start()
	{
        maxHealth = Mathf.Max(1, maxHealth);
		currentHealth = maxHealth;
		UpdateUI();
	}

    /// <summary>
    /// 对玩家造成伤害；格挡无敌、已死亡或非正伤害都会被忽略。
    /// </summary>
    /// <param name="amount">伤害值，必须大于 0。</param>
	public void TakeDamage(int amount)
	{
		if (amount > 0 && IsAlive && !isParryInvincible)
		{
			currentHealth = Mathf.Max(0, currentHealth - amount);
			UpdateUI();
			if (GetComponent<HitFlash>() != null)
			{
				GetComponent<HitFlash>().Flash(Color.red);
			}
			if (currentHealth <= 0)
			{
				Die();
			}
		}
	}

    /// <summary>
    /// 恢复生命值，不会超过最大生命值。
    /// </summary>
    /// <param name="amount">恢复值，必须大于 0。</param>
    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }

	private void Die()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.StopBGMWithFade();
			AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
		}
        DeathSceneDirector deathDirector = FindAnyObjectByType<DeathSceneDirector>();
		if (deathDirector != null)
		{
			deathDirector.TriggerPlayerDeathFlow();
		}
		else
		{
			Debug.LogWarning("[PlayerHealth] 场景中未找到 DeathSceneDirector，无法播放死亡流程。", this);
		}
		Time.timeScale = 0f;
	}

	private void UpdateUI()
	{
		if (hpSlider != null)
		{
			hpSlider.maxValue = maxHealth;
			hpSlider.value = currentHealth;
		}
	}
}
