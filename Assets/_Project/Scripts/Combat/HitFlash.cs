using System.Collections;
using UnityEngine;

/// <summary>
/// 受击反馈组件，负责瞬时减速与命中特效实例化。
/// </summary>
/// <remarks>
/// 减速状态用 isSlowed 做重入保护，连续命中时不会叠加协程导致速度无法恢复。
/// flashColor 保留给后续材质/着色器闪烁实现，当前反馈由特效预制体承担。
/// </remarks>
public class HitFlash : MonoBehaviour
{
	public GameObject hitEffectPrefab;

	private EnemyController enemyController;

	private float originalSpeed;

	private bool isSlowed;

	private void Start()
	{
		enemyController = GetComponent<EnemyController>();
		if (enemyController != null)
		{
			originalSpeed = enemyController.moveSpeed;
		}
	}

    /// <summary>
    /// 播放一次受击反馈。
    /// </summary>
    /// <param name="flashColor">预留的闪烁颜色，供自定义受击 Shader 使用。</param>
	public void Flash(Color flashColor)
	{
		if (enemyController != null && !isSlowed)
		{
			StartCoroutine(SlowRoutine());
		}
		if (hitEffectPrefab != null)
		{
			GameObject obj = Object.Instantiate(hitEffectPrefab, base.transform.position, Quaternion.identity);
			obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
			Object.Destroy(obj, 0.15f);
		}
	}

	private IEnumerator SlowRoutine()
	{
		isSlowed = true;
		enemyController.moveSpeed = originalSpeed * 0.75f;
		yield return new WaitForSeconds(0.1f);
		enemyController.moveSpeed = originalSpeed;
		isSlowed = false;
	}
}
