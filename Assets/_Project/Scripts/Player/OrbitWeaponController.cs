using System;
using UnityEngine;

/// <summary>
/// 将武器预制体按等角度实例化到玩家周围，并持续旋转武器挂点。
/// </summary>
/// <remarks>
/// 武器数量变化时整体重建挂点，避免维护复杂的增删状态；当前规模较小，
/// 重建成本远低于状态同步成本。若武器数量继续增长，应改为对象池复用。
/// </remarks>
public class OrbitWeaponController : MonoBehaviour
{
	public float rotateSpeed = 360f;

	public int weaponCount = 1;

	public GameObject weaponPrefab;

	public float orbitRadius = 2f;

	private void Start()
	{
		SpawnWeapons();
	}

	private void Update()
	{
		base.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
	}

    /// <summary>
    /// 按当前数量和半径重新创建全部武器挂点。
    /// </summary>
	public void SpawnWeapons()
	{
        // 先清理旧挂点，确保武器数量、角度和进化外观始终与配置一致。
		foreach (Transform item in base.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (weaponCount > 0 && !(weaponPrefab == null))
		{
			float num = 360f / (float)weaponCount;
			for (int i = 0; i < weaponCount; i++)
			{
				float f = (float)i * num * Mathf.Deg2Rad;
				Vector3 position = base.transform.position + new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * orbitRadius;
				GameObject obj = UnityEngine.Object.Instantiate(weaponPrefab, position, Quaternion.identity, base.transform);
				Vector3 up = obj.transform.position - base.transform.position;
				obj.transform.up = up;
				obj.transform.Rotate(0f, 0f, 30f);
			}
		}
	}

    /// <summary>
    /// 使用进化阶段配置重置武器数量和外观。
    /// </summary>
    /// <param name="count">新阶段的武器数量。</param>
    /// <param name="newWeaponPrefab">新阶段的武器预制体。</param>
	public void ResetWeaponCount(int count, GameObject newWeaponPrefab)
	{
		weaponCount = count;
		weaponPrefab = newWeaponPrefab;
		SpawnWeapons();
	}
}
