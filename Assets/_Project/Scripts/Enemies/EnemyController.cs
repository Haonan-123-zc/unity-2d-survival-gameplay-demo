using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// 近战敌人的移动、受击、死亡和掉落控制器。
/// </summary>
/// <remarks>
/// 敌人采用轻量寻向逻辑：每帧朝玩家位置移动，并按水平位移翻转 2D 精灵。
/// isDead 同时承担死亡流程的状态锁，防止同一帧内多个武器碰撞重复结算掉落。
/// </remarks>
public class EnemyController : MonoBehaviour
{
	[Header("怪物基础属性")]
	public float moveSpeed = 2f;

	public int health = 30;

	public bool isOriginalFacingRight;

	[Header("基础掉落")]
	public GameObject expGemPrefab;

	[Header("稀有掉落")]
	[Tooltip("把做好的磁铁预制体拖到这里")]
	public GameObject magnetPrefab;

	[Tooltip("把做好的血瓶预制体拖到这里")]
	public GameObject healthPotPrefab;

	private Transform playerTransform;

	private Vector3 originalScale;

	private bool isDead;

	private void Start()
	{
		originalScale = base.transform.localScale;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
		if (gameObject != null)
		{
			playerTransform = gameObject.transform;
		}
	}

	private void LateUpdate()
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
		}
		else
		{
			Vector3 normalized = (playerTransform.position - base.transform.position).normalized;
			base.transform.position += normalized * moveSpeed * Time.deltaTime;
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

    /// <summary>
    /// 扣除生命值，并在生命值归零时进入一次死亡流程。
    /// </summary>
    /// <param name="damage">伤害值，必须大于 0。</param>
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
            // 当前怪物共享一个控制器，通过预制体名称选择对应死亡音效。
			string text = base.gameObject.name;
			if (text.Contains("Zombie"))
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.zombieDeathSound);
			}
			else if (text.Contains("Imp"))
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.impDeathSound);
			}
			else if (text.Contains("Skeleton"))
			{
				AudioManager.Instance.PlaySFX(AudioManager.Instance.skeletonDeathSound);
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
		float num = Random.Range(0f, 100f);
		if (num < 0.5f)
		{
			if (magnetPrefab != null)
			{
				Object.Instantiate(magnetPrefab, base.transform.position, Quaternion.identity);
			}
		}
		else if (num >= 0.5f && num < 1.5f && healthPotPrefab != null)
		{
			Object.Instantiate(healthPotPrefab, base.transform.position, Quaternion.identity);
		}
		Object.Destroy(base.gameObject);
	}
}
