using System.Collections;
using UnityEngine;

/// <summary>
/// 终局 Boss 控制器，以协程实现追击、跳跃砸地、电波脉冲和死亡演出。
/// </summary>
/// <remarks>
/// Boss 使用 isAttacking 锁住移动和重复技能，使整个攻击序列保持原子性。
/// 跳跃砸地会先播放预警，再关闭本体碰撞并从空中落到目标点，
/// 既避免空中穿过玩家，也为玩家留出明确闪避窗口。
/// </remarks>
[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
public class MechaGorillaBoss : MonoBehaviour
{
	public Transform player;

	public GameObject shockwavePrefab;

	public GameObject winUIPanel;

	[Header("终局传送门")]
	[Tooltip("把做好的传送门预制体拖到这里")]
	public GameObject portalPrefab;

	[Tooltip("你的死亡动画播完需要多少秒，就填多少秒（比如 1.5）")]
	public float dieAnimationDuration = 1.5f;

	[Header("落地预警配置")]
	[Tooltip("把做好的半透明红圈预制体拖到这里")]
	public GameObject warningCirclePrefab;

	[Range(0.1f, 2f)]
	public float circleScaleModifier = 0.5f;

	[Range(0f, 1f)]
	public float circleAlpha = 0.5f;

	[Header("基础属性与技能时间")]
	public float moveSpeed = 3f;

	public float maxHealth = 3000f;

	public float attackInterval = 10f;

	public float roarDuration = 1.2f;

	public float smashRadius = 5f;

	[Header("近战伤害配置")]
	public int bodyDamageAmount = 20;

	public float bodyDamageInterval = 1f;

	private float lastBodyDamageTime;

	[Header("朝向配置")]
	public bool isOriginalFacingRight;

	[Header("战利品掉落配置")]
	public GameObject expGemPrefab;

	public GameObject magnetPrefab;

	public GameObject healthPotPrefab;

	[Header("武器 Layer 配置")]
	[Tooltip("玩家武器的层级名字（例如 'PlayerWeapon'），漏填会导致无法扣血！")]
	public string playerWeaponLayerName = "PlayerWeapon";

	private int playerWeaponLayer;

	private float currentHealth;

	private float attackTimer;

	private bool isAttacking;

	private Animator anim;

	private Rigidbody2D rb;

	private Collider2D bodyCollider;

	private bool isDead;

	private Vector3 originalScale;

	private void Start()
	{
		currentHealth = maxHealth;
		attackTimer = attackInterval;
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		bodyCollider = GetComponent<Collider2D>();
		originalScale = base.transform.localScale;
		playerWeaponLayer = LayerMask.NameToLayer(playerWeaponLayerName);
		if (playerWeaponLayer == -1)
		{
			Debug.LogWarning($"[Boss] 未找到名为 '{playerWeaponLayerName}' 的 Layer，Boss 将无法识别玩家武器。", this);
		}
		if (player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player")?.transform;
		}
	}

	private void Update()
	{
		if (!isDead && !(Time.timeScale <= 0f) && !isAttacking)
		{
			if (player != null)
			{
				Vector3 normalized = (player.position - base.transform.position).normalized;
				base.transform.position += normalized * moveSpeed * Time.deltaTime;
				HandleDirection(normalized.x);
			}
			attackTimer -= Time.deltaTime;
			if (attackTimer <= 0f)
			{
				TriggerRandomAttack();
				attackTimer = attackInterval;
			}
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

	private void TriggerRandomAttack()
	{
		isAttacking = true;
		if (Random.Range(0f, 100f) < 50f)
		{
			StartCoroutine(SkySmashFlow());
		}
		else
		{
			StartCoroutine(PureElectricPulseFlow());
		}
	}

	private IEnumerator SkySmashFlow()
	{
		rb.linearVelocity = Vector2.zero;
		if (anim != null)
		{
			anim.SetTrigger("TrRoar");
		}
		yield return new WaitForSeconds(roarDuration);
		if (anim != null)
		{
			anim.SetTrigger("TrJump");
		}
        // 砸地期间关闭本体碰撞，防止 Boss 在空中移动时误伤玩家。
		if (bodyCollider != null)
		{
			bodyCollider.enabled = false;
		}
		rb.bodyType = RigidbodyType2D.Kinematic;
		Vector3 startPos = base.transform.position;
		Vector3 skyPos = startPos + new Vector3(0f, 15f, 0f);
		float t2 = 0f;
		while (t2 < 0.4f)
		{
			t2 += Time.deltaTime;
			base.transform.position = Vector3.Lerp(startPos, skyPos, t2 / 0.4f);
			yield return null;
		}
		Vector3 targetLandPos = ((player != null) ? player.position : base.transform.position);
		GameObject activeWarningCircle = null;
        // 预警圈在伤害结算前生成，尺寸与真正的砸地半径保持一致。
		if (warningCirclePrefab != null)
		{
			activeWarningCircle = Object.Instantiate(warningCirclePrefab, targetLandPos, Quaternion.identity);
			float num = smashRadius * 2f * circleScaleModifier;
			activeWarningCircle.transform.localScale = new Vector3(num, num, 1f);
			SpriteRenderer component = activeWarningCircle.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				Color color = component.color;
				color.a = circleAlpha;
				component.color = color;
			}
		}
		yield return new WaitForSeconds(0.4f);
		base.transform.position = new Vector3(targetLandPos.x, targetLandPos.y + 15f, 0f);
		if (anim != null)
		{
			anim.SetTrigger("TrFall");
		}
		t2 = 0f;
		Vector3 dropStart = base.transform.position;
		while (t2 < 0.3f)
		{
			t2 += Time.deltaTime;
			base.transform.position = Vector3.Lerp(dropStart, targetLandPos, t2 / 0.3f);
			yield return null;
		}
		base.transform.position = targetLandPos;
		if (activeWarningCircle != null)
		{
			Object.Destroy(activeWarningCircle);
		}
		rb.bodyType = RigidbodyType2D.Dynamic;
		if (bodyCollider != null)
		{
			bodyCollider.enabled = true;
		}
		if (anim != null)
		{
			anim.SetTrigger("TrLand");
		}
		ExecuteImpactEffects(targetLandPos);
		yield return new WaitForSeconds(0.6f);
		isAttacking = false;
	}

	private IEnumerator PureElectricPulseFlow()
	{
		rb.linearVelocity = Vector2.zero;
		if (shockwavePrefab != null)
		{
			Object.Instantiate(shockwavePrefab, base.transform.position, Quaternion.identity);
		}
		yield return new WaitForSeconds(0.3f);
		isAttacking = false;
	}

	private void ExecuteImpactEffects(Vector3 center)
	{
		if (shockwavePrefab != null)
		{
			Object.Instantiate(shockwavePrefab, center, Quaternion.identity);
		}
        // 冲击波碰撞体负责视觉和伤害，这里额外应用击退，强化命中反馈。
		Collider2D[] array = Physics2D.OverlapCircleAll(center, smashRadius);
		foreach (Collider2D collider2D in array)
		{
			if (!collider2D.CompareTag("Player"))
			{
				continue;
			}
			PlayerHealth component = collider2D.GetComponent<PlayerHealth>();
			if (component != null)
			{
				component.TakeDamage(bodyDamageAmount);
				Rigidbody2D component2 = collider2D.GetComponent<Rigidbody2D>();
				if (component2 != null)
				{
					Vector2 vector = (collider2D.transform.position - center).normalized;
					component2.AddForce(vector * 12f, ForceMode2D.Impulse);
				}
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (!isDead && !(Time.timeScale <= 0f) && collision.gameObject.CompareTag("Player") && Time.time - lastBodyDamageTime >= bodyDamageInterval)
		{
			PlayerHealth component = collision.gameObject.GetComponent<PlayerHealth>();
			if (component != null)
			{
				component.TakeDamage(bodyDamageAmount);
				lastBodyDamageTime = Time.time;
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!isDead && !(Time.timeScale <= 0f) && collision.gameObject.CompareTag("Player") && Time.time - lastBodyDamageTime >= bodyDamageInterval)
		{
			PlayerHealth component = collision.gameObject.GetComponent<PlayerHealth>();
			if (component != null)
			{
				component.TakeDamage(bodyDamageAmount);
				lastBodyDamageTime = Time.time;
			}
		}
	}

    /// <summary>
    /// 对 Boss 造成伤害；死亡后忽略后续伤害。
    /// </summary>
    /// <param name="damage">伤害值。</param>
	public void TakeDamage(float damage)
	{
		if (isDead || damage <= 0f)
		{
			return;
		}
		currentHealth -= damage;
		GetComponent<HitFlash>()?.Flash(Color.white);
		if (currentHealth <= 0f)
		{
			StartCoroutine(BossDieSequence());
		}
	}

	private IEnumerator BossDieSequence()
	{
		if (isDead)
		{
			yield break;
		}
		isDead = true;
		isAttacking = true;
		if (bodyCollider != null)
		{
			bodyCollider.enabled = false;
		}
		if (rb != null)
		{
			rb.linearVelocity = Vector2.zero;
			rb.bodyType = RigidbodyType2D.Kinematic;
		}
		if (anim != null)
		{
			anim.SetTrigger("TrDie");
		}
		if (expGemPrefab != null)
		{
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f), 0f);
				Object.Instantiate(expGemPrefab, base.transform.position + vector, Quaternion.identity);
			}
		}
		if (magnetPrefab != null)
		{
			Object.Instantiate(magnetPrefab, base.transform.position, Quaternion.identity);
		}
		yield return new WaitForSeconds(dieAnimationDuration);
		if (portalPrefab != null)
		{
			Vector3 position = base.transform.position + new Vector3(0f, 0.5f, 0f);
			Object.Instantiate(portalPrefab, position, Quaternion.identity);
		}
        // 传送门已独立实例化，可以安全销毁 Boss 本体及其运行时组件。
		Object.Destroy(base.gameObject);
	}
}
