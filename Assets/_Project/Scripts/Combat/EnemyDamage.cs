using UnityEngine;

/// <summary>
/// 近战敌人的持续接触伤害组件。
/// </summary>
/// <remarks>
/// 使用 damageInterval 限流，避免 OnCollisionStay2D 每物理帧结算一次伤害。
/// </remarks>
public class EnemyDamage : MonoBehaviour
{
	public int damageAmount = 10;

	public float damageInterval = 1f;

	private float lastDamageTime;

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player") && Time.time - lastDamageTime >= damageInterval)
		{
			PlayerHealth component = collision.gameObject.GetComponent<PlayerHealth>();
			if (component != null)
			{
				component.TakeDamage(damageAmount);
				lastDamageTime = Time.time;
			}
		}
	}
}
