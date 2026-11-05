using UnityEngine;

/// <summary>
/// 鬼魂发射的直线弹丸，负责移动、寿命管理和玩家命中结算。
/// </summary>
/// <remarks>
/// 方向可以由发射者预先注入；未注入时在 Start 中朝玩家所在位置初始化，
/// 保证预制体既能被精确瞄准，也能作为普通场景弹丸独立使用。
/// </remarks>
public class GhostBullet : MonoBehaviour
{
	[Header("弹丸参数")]
	public float speed = 8f;

	public float lifeTime = 5f;

	public int damageValue = 10;

	private Vector2 flyDirection;

	private bool hasDirection;

	private void Start()
	{
		Object.Destroy(base.gameObject, lifeTime);
		if (!hasDirection)
		{
			GameObject gameObject = GameObject.Find("Player");
			if (gameObject != null)
			{
				flyDirection = ((Vector2)gameObject.transform.position - (Vector2)base.transform.position).normalized;
				hasDirection = true;
			}
		}
		Collider2D component = GetComponent<Collider2D>();
		if (component != null)
		{
			component.isTrigger = true;
		}
	}

    /// <summary>
    /// 设置弹丸飞行方向；零向量会被忽略。
    /// </summary>
    /// <param name="newDirection">世界空间中的期望飞行方向。</param>
	public void SetBulletDirection(Vector2 newDirection)
	{
		if (!(newDirection == Vector2.zero))
		{
			flyDirection = newDirection.normalized;
			hasDirection = true;
			float z = Mathf.Atan2(flyDirection.y, flyDirection.x) * 57.29578f;
			base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	private void Update()
	{
		if (hasDirection)
		{
			base.transform.position += (Vector3)(flyDirection * speed * Time.deltaTime);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
        // 兼容碰撞体挂在 Player 子节点上的情况，避免子弹穿过角色但无法造成伤害。
		if (other.CompareTag("Player") || other.name == "Player")
		{
			PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
			if (playerHealth != null)
			{
				playerHealth.TakeDamage(damageValue);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
