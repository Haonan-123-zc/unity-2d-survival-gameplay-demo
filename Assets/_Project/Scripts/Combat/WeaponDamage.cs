using UnityEngine;

/// <summary>
/// 玩家武器命中分发器，将伤害传递给可受击的敌人、Boss 和场景树木。
/// </summary>
/// <remarks>
/// 命中优先级从敌人到 Boss、再到场景物件，命中一个目标后立即返回，
/// 防止父级碰撞体导致同一次挥击重复结算。
/// </remarks>
public class WeaponDamage : MonoBehaviour
{
	public int damageAmount = 10;

	private void OnTriggerEnter2D(Collider2D other)
	{
        // Boss 的受击碰撞体可能位于子节点，因此需要向父级和子级补查一次。
		EnemyController component = other.GetComponent<EnemyController>();
		if (component != null)
		{
			component.TakeDamage(damageAmount);
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound);
			}
			return;
		}
		GhostController component2 = other.GetComponent<GhostController>();
		if (component2 != null)
		{
			component2.TakeDamage(damageAmount);
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound);
			}
			return;
		}
		MechaGorillaBoss mechaGorillaBoss = other.GetComponentInParent<MechaGorillaBoss>() ?? other.GetComponentInChildren<MechaGorillaBoss>() ?? other.GetComponent<MechaGorillaBoss>();
		if (mechaGorillaBoss != null)
		{
			mechaGorillaBoss.TakeDamage(damageAmount);
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound);
			}
			return;
		}
		InteractiveTree component3 = other.GetComponent<InteractiveTree>();
		if (component3 != null)
		{
			component3.TakeDamage(damageAmount);
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound);
			}
		}
	}
}
