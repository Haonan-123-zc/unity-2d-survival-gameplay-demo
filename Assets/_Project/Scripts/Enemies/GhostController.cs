using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// 远程鬼魂控制器，实现保持距离、定时射击、接触伤害和死亡掉落。
/// </summary>
/// <remarks>
/// 行为相当于一个状态机：距离过远时追击，过近时后撤，在攻击区间内保持位置并射击。
/// 弹丸伤害与接触伤害分离，便于分别调参。
/// </remarks>
public class GhostController : MonoBehaviour
{
	[Header("基础属性")]
	public int health = 30;

	public GameObject expGemPrefab;

	public bool isOriginalFacingRight;

	public float moveSpeed = 2f;

	[Header("距离与风筝配置")]
	public float attackRange = 12f;

	public float retreatRange = 8f;

	[Header("远程攻击")]
	public GameObject projectilePrefab;

	public float fireRate = 1.5f;

	[Header("接触伤害")]
	public int damageAmount = 10;

	public float damageInterval = 1f;

	private float lastDamageTime;

	private Transform playerTransform;

	private Vector3 originalScale;

	private float fireTimer;

	private bool isDead;

	private void Start()
	{
		originalScale = base.transform.localScale;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
		if (gameObject != null)
		{
			playerTransform = gameObject.transform;
		}
		fireTimer = fireRate;
		Collider2D component = GetComponent<Collider2D>();
		if (component != null)
		{
			component.isTrigger = true;
		}
		Rigidbody2D component2 = GetComponent<Rigidbody2D>();
		if (component2 != null)
		{
			component2.bodyType = RigidbodyType2D.Kinematic;
			component2.simulated = true;
		}
	}

	private void Update()
	{
		if (isDead || Time.timeScale <= 0f)
		{
			return;
		}
		if (playerTransform == null)
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
			if (gameObject != null)
			{
				playerTransform = gameObject.transform;
			}
			return;
		}
		fireTimer -= Time.deltaTime;
		if (fireTimer <= 0f)
		{
			SpawnGhostBullet();
			fireTimer = fireRate;
		}
	}

	private void LateUpdate()
	{
		if (!isDead && !(Time.timeScale <= 0f) && !(playerTransform == null))
		{
			Vector3 normalized = (playerTransform.position - base.transform.position).normalized;
			float num = Vector3.Distance(base.transform.position, playerTransform.position);
			if (num > attackRange)
			{
				base.transform.position += normalized * moveSpeed * Time.deltaTime;
			}
			else if (num < retreatRange)
			{
				base.transform.position -= normalized * (moveSpeed * 0.8f) * Time.deltaTime;
			}
			HandleDirection(normalized.x);
		}
	}

	private void HandleDirection(float moveX)
	{
		if (Mathf.Abs(moveX) < 0.05f)
		{
			return;
		}
		if (isOriginalFacingRight)
		{
			if (moveX > 0f)
			{
				base.transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
			}
			else if (moveX < 0f)
			{
				base.transform.localScale = new Vector3(0f - originalScale.x, originalScale.y, originalScale.z);
			}
		}
		else if (moveX > 0f)
		{
			base.transform.localScale = new Vector3(0f - originalScale.x, originalScale.y, originalScale.z);
		}
		else if (moveX < 0f)
		{
			base.transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
		}
	}

	private void SpawnGhostBullet()
	{
		if (!(projectilePrefab == null))
		{
			if (AudioManager.Instance != null && AudioManager.Instance.buttonClickSound != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
			}
			GameObject obj = Object.Instantiate(projectilePrefab, base.transform.position, Quaternion.identity);
			Vector2 normalized = ((Vector2)playerTransform.position - (Vector2)base.transform.position).normalized;
            // 直接调用组件接口，避免 SendMessage 的反射开销和字符串耦合。
			obj.GetComponent<GhostBullet>()?.SetBulletDirection(normalized);
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!isDead && !(Time.timeScale <= 0f) && (other.CompareTag("Player") || other.name == "Player") && Time.time - lastDamageTime >= damageInterval)
		{
			PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
			if (playerHealth != null)
			{
				playerHealth.TakeDamage(damageAmount);
				lastDamageTime = Time.time;
			}
		}
	}

    /// <summary>
    /// 扣除鬼魂生命值；非正伤害会被忽略。
    /// </summary>
    /// <param name="damage">伤害值。</param>
	public void TakeDamage(int damage)
	{
		if (!isDead && damage > 0)
		{
			health -= damage;
			HitFlash component = GetComponent<HitFlash>();
			if (component != null)
			{
				component.Flash(Color.white);
			}
			if (health <= 0)
			{
				Die();
			}
		}
	}

	private void Die()
	{
		if (isDead)
		{
			return;
		}
		isDead = true;
		Animator component = GetComponent<Animator>();
		if (component != null)
		{
			component.enabled = false;
		}
		SpriteSkin[] componentsInChildren = GetComponentsInChildren<SpriteSkin>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		SpriteRenderer[] componentsInChildren2 = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		if (AudioManager.Instance != null)
		{
			if (AudioManager.Instance.ghostDeathSound != null)
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.ghostDeathSound);
			}
			else
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.zombieDeathSound);
			}
		}
		if (expGemPrefab != null)
		{
			Object.Instantiate(expGemPrefab, base.transform.position, Quaternion.identity);
		}
		Object.Destroy(base.gameObject);
	}
}
