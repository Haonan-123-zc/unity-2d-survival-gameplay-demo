using UnityEngine;

/// <summary>
/// Boss 电波冲击波，负责扩散、渐隐、自销毁和单次玩家伤害。
/// </summary>
/// <remarks>
/// 冲击波只在自身 Trigger 命中玩家时造成伤害；销毁由最大缩放半径控制，
/// 因此视觉效果和伤害边界使用同一套时间尺度。
/// </remarks>
public class BossShockwave : MonoBehaviour
{
	public float expandSpeed = 15f;

	public float maxRadius = 25f;

	public float damage = 20f;

	private SpriteRenderer sr;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		base.transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
		if (base.transform.localScale.x >= maxRadius)
		{
			Object.Destroy(base.gameObject);
		}
		else if (base.transform.localScale.x >= maxRadius * 0.7f && sr != null)
		{
			Color color = sr.color;
			color.a = Mathf.Lerp(color.a, 0f, 5f * Time.deltaTime);
			sr.color = color;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			PlayerHealth component = collision.GetComponent<PlayerHealth>();
			if (component != null)
			{
				component.TakeDamage((int)damage);
			}
		}
	}
}
