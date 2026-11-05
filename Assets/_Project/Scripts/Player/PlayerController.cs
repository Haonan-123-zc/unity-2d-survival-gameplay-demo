using UnityEngine;

/// <summary>
/// 玩家角色移动控制器，负责读取移动输入、驱动 Rigidbody2D 并同步移动动画。
/// </summary>
/// <remarks>
/// 输入采样放在 Update 中，实际位置更新放在 FixedUpdate 中，从而消除帧率波动对物理移动距离的影响。
/// 场景需要提供名为 Horizontal、Vertical 的输入轴，Animator 需要 Speed（float）和 isRunning（bool）参数。
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("角色每秒移动的 Unity 世界单位。")]
	public float moveSpeed = 5f;

	private Rigidbody2D rb;

	private Vector2 moveInput;

	private bool isFacingRight = true;

	private Animator anim;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponentInChildren<Animator>();
	}

	private void Update()
	{
        // 暂停或播放剧情时停止输入，避免恢复游戏后角色继续沿旧输入移动。
		if (Time.timeScale == 0f)
		{
			moveInput = Vector2.zero;
			if (anim != null)
			{
				anim.SetFloat("Speed", 0f);
				anim.SetBool("isRunning", value: false);
			}
			return;
		}
		moveInput.x = Input.GetAxisRaw("Horizontal");
		moveInput.y = Input.GetAxisRaw("Vertical");
        // 归一化保证斜向移动速度不会达到水平或垂直移动的约 1.4 倍。
		moveInput.Normalize();
		if (moveInput.x > 0f && !isFacingRight)
		{
			Flip();
		}
		else if (moveInput.x < 0f && isFacingRight)
		{
			Flip();
		}
		if (anim == null || !anim.gameObject.activeInHierarchy)
		{
			anim = GetComponentInChildren<Animator>();
		}
		if (anim != null)
		{
			float magnitude = moveInput.magnitude;
			bool value = magnitude > 0f;
			anim.SetFloat("Speed", magnitude);
			anim.SetBool("isRunning", value);
		}
	}

	private void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
            // MovePosition 保留刚体碰撞检测，比直接修改 Transform.position 更适合角色移动。
			rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
		}
	}

	private void Flip()
	{
		isFacingRight = !isFacingRight;
		Vector3 localScale = base.transform.localScale;
		localScale.x *= -1f;
		base.transform.localScale = localScale;
	}

    /// <summary>
    /// 角色外观切换后重新获取 Animator。
    /// </summary>
    /// <remarks>
    /// 进化系统会启用不同的角色模型；旧 Animator 被隐藏后需要重新绑定当前可见模型。
    /// </remarks>
	public void RefreshAnimator()
	{
		anim = GetComponentInChildren<Animator>();
	}
}
