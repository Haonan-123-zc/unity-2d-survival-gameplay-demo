using UnityEngine;

/// <summary>
/// 通用掉落物行为，通过枚举配置磁铁吸收或生命恢复效果。
/// </summary>
/// <remarks>
/// 两种道具共享碰撞、特效和销毁流程，仅效果部分按类型分支，
/// 便于继续添加新的拾取物类型。
/// </remarks>
public class ItemPickup : MonoBehaviour
{
	public enum ItemType
	{
		Magnet,
		HealthPot
	}

	[Header("掉落物类型")]
	public ItemType itemType;

	[Header("拾取特效（可选）")]
	public GameObject pickupEffectPrefab;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			TriggerItemEffect(collision.gameObject);
			if (pickupEffectPrefab != null)
			{
				Object.Instantiate(pickupEffectPrefab, base.transform.position, Quaternion.identity);
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void TriggerItemEffect(GameObject playerObj)
	{
		switch (itemType)
		{
		case ItemType.Magnet:
		{
			ExpGem[] array = Object.FindObjectsByType<ExpGem>(FindObjectsSortMode.None);
			foreach (ExpGem expGem in array)
			{
				if (expGem != null)
				{
					expGem.StartFlyToPlayer(playerObj.transform);
				}
			}
			break;
		}
		case ItemType.HealthPot:
		{
			PlayerHealth component = playerObj.GetComponent<PlayerHealth>();
			if (component == null)
			{
				break;
			}
            // 生命值组件拥有上限约束；道具层只表达“恢复最大生命值”的意图。
			component.Heal(component.maxHealth);
			break;
		}
		}
	}
}
