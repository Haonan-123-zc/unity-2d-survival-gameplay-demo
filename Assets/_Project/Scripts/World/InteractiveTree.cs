using UnityEngine;

/// <summary>
/// 可破坏场景树：受到足够伤害后朝玩家方向倒伏，并在落地后对敌人造成一次范围伤害。
/// </summary>
/// <remarks>
/// 倒伏期间关闭伤害判定，只有倒下动作完成后才启用 Trigger，
/// 防止树木在动画中途提前命中敌人。
/// </remarks>
public class InteractiveTree : MonoBehaviour
{
	[Header("树木属性")]
	public int treeHealth = 20;

	public float damageToEnemies = 50f;

	[Header("倒伏与伤害")]
	public Transform PivotObj;

	public Collider2D damageCollider;

	private bool isFalling;

	private float fallRotation;

	private float fallSpeed = 300f;

	private int fallDirection = 1;

	private void Start()
	{
		if (damageCollider != null)
		{
			damageCollider.enabled = false;
			damageCollider.isTrigger = true;
		}
	}

	private void Update()
	{
		if (isFalling)
		{
			float num = fallSpeed * Time.deltaTime * (float)fallDirection;
			fallRotation += Mathf.Abs(num);
			if (PivotObj != null)
			{
				PivotObj.Rotate(0f, 0f, 0f - num);
			}
			else
			{
				base.transform.Rotate(0f, 0f, 0f - num);
			}
			if (fallRotation >= 90f)
			{
				isFalling = false;
				OnTreeHitGround();
			}
		}
	}

    /// <summary>
    /// 对树木造成伤害，生命值归零后开始倒伏。
    /// </summary>
    /// <param name="damage">伤害值，必须大于 0。</param>
	public void TakeDamage(int damage)
	{
		if (!isFalling && damage > 0)
		{
			treeHealth -= damage;
			HitFlash component = GetComponent<HitFlash>();
			if (component != null)
			{
				component.Flash(Color.white);
			}
			if (treeHealth <= 0)
			{
				StartFalling();
			}
		}
	}

	private void StartFalling()
	{
		isFalling = true;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
		if (gameObject != null)
		{
			fallDirection = ((!(gameObject.transform.position.x > base.transform.position.x)) ? 1 : (-1));
		}
	}

	private void OnTreeHitGround()
	{
		if (damageCollider != null)
		{
			damageCollider.enabled = true;
		}
		Invoke("DestroyTree", 0.1f);
	}

	private void DestroyTree()
	{
		Object.Destroy(base.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (damageCollider == null || !damageCollider.enabled)
		{
			return;
		}
		EnemyController component = collision.GetComponent<EnemyController>();
		if (component != null)
		{
			component.TakeDamage((int)damageToEnemies);
			return;
		}
		GhostController component2 = collision.GetComponent<GhostController>();
		if (component2 != null)
		{
			component2.TakeDamage((int)damageToEnemies);
		}
	}
}
