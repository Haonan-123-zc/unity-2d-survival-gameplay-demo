using UnityEngine;

/// <summary>
/// 经验宝石拾取物，支持被磁铁提前激活后加速飞向玩家。
/// </summary>
/// <remarks>
/// 速度随距离缩短而提高，使宝石先慢后快地汇聚，既清晰可读，
/// 又能快速清空大量掉落物。
/// </remarks>
public class ExpGem : MonoBehaviour
{
	public int expValue = 10;

	public float startSpeed = 5f;

	public float acceleration = 15f;

	private Transform playerTransform;

	private bool isBeingCollected;

	private float currentSpeed;

	private void Start()
	{
		currentSpeed = startSpeed;
	}

	private void Update()
	{
		if (isBeingCollected && playerTransform != null)
		{
			float num = Vector3.Distance(base.transform.position, playerTransform.position);
			float num2 = 1f + 1f / (num + 0.1f);
			currentSpeed += acceleration * Time.deltaTime;
			float num3 = currentSpeed * num2;
			base.transform.position = Vector3.MoveTowards(base.transform.position, playerTransform.position, num3 * Time.deltaTime);
			if (num < 0.2f)
			{
				Collect();
			}
		}
	}

    /// <summary>
    /// 开始追踪指定玩家；重复调用不会创建第二条飞行状态。
    /// </summary>
    /// <param name="player">玩家 Transform。</param>
	public void StartFlyToPlayer(Transform player)
	{
		if (!isBeingCollected)
		{
			playerTransform = player;
			isBeingCollected = true;
		}
	}

	private void Collect()
	{
		if (!(playerTransform == null))
		{
			PlayerStats component = playerTransform.GetComponent<PlayerStats>();
			if (component != null)
			{
				component.GainExp(expValue);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
