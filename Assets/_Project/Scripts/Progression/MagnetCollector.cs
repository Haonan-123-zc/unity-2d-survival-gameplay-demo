using UnityEngine;

/// <summary>
/// 玩家吸附范围触发器，将进入范围的经验宝石切换为追踪状态。
/// </summary>
/// <remarks>
/// 触发器自身不拥有玩家状态，而是从父节点获取 Transform，
/// 因此可以直接挂到玩家的范围碰撞体子对象上。
/// </remarks>
public class MagnetCollector : MonoBehaviour
{
	private Transform playerTransform;

	private void Start()
	{
		playerTransform = base.transform.parent;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		ExpGem component = collision.GetComponent<ExpGem>();
		if (component != null)
		{
			component.StartFlyToPlayer(playerTransform);
		}
	}
}
