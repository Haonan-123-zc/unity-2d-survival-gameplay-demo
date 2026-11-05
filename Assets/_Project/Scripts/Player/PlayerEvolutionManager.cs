using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按等级节点切换角色外观、武器和地图主题。
/// </summary>
/// <remarks>
/// 进化配置通过有序列表暴露给 Inspector。列表索引即阶段索引，
/// 切换时只激活目标阶段模型，保证同一角色不会同时显示两套模型。
/// </remarks>
public class PlayerEvolutionManager : MonoBehaviour
{
	[Header("进化阶段配置")]
	public List<EvolutionStage> evolutionStages;

	private int currentStageIndex;

	private void Start()
	{
		ApplyEvolution(0);
	}

    /// <summary>
    /// 进入下一进化阶段；已处于最终阶段时保持最终阶段不变。
    /// </summary>
	public void EvolveToNextStage()
	{
		currentStageIndex++;
		if (currentStageIndex < evolutionStages.Count)
		{
			PlayerEvolutionFX component = GetComponent<PlayerEvolutionFX>();
			if (component != null)
			{
				component.PlayEvolutionAnimation(delegate
				{
					ApplyEvolution(currentStageIndex);
					GetComponent<HitFlash>()?.Flash(Color.white);
					if (AudioManager.Instance != null)
					{
						AudioManager.Instance.PlaySFX(AudioManager.Instance.evolveSound);
					}
					CheckAndSwitchBattleBGM();
				});
			}
			else
			{
				ApplyEvolution(currentStageIndex);
				GetComponent<HitFlash>()?.Flash(Color.white);
				if (AudioManager.Instance != null)
				{
					AudioManager.Instance.PlaySFX(AudioManager.Instance.evolveSound);
				}
				CheckAndSwitchBattleBGM();
			}
		}
		else
		{
			currentStageIndex = evolutionStages.Count - 1;
		}
	}

	private void CheckAndSwitchBattleBGM()
	{
		if (currentStageIndex == 3 && AudioManager.Instance != null)
		{
			AudioManager.Instance.SwitchToEvolvedBGM();
		}
	}

	private void ApplyEvolution(int stageIndex)
	{
		if (evolutionStages == null || evolutionStages.Count == 0 || stageIndex >= evolutionStages.Count)
		{
			return;
		}
		GameObject gameObject = null;
		for (int i = 0; i < evolutionStages.Count; i++)
		{
			if (evolutionStages[i].stageObject != null)
			{
				bool flag = i == stageIndex;
				evolutionStages[i].stageObject.SetActive(flag);
				if (flag)
				{
					gameObject = evolutionStages[i].stageObject;
				}
			}
		}
		if (gameObject != null)
		{
            // 武器引用位于当前阶段模型子节点中，因此必须在模型切换完成后再重新绑定。
			OrbitWeaponController componentInChildren = gameObject.GetComponentInChildren<OrbitWeaponController>();
			if (componentInChildren != null)
			{
				componentInChildren.ResetWeaponCount(3, evolutionStages[stageIndex].stageWeaponPrefab);
			}
		}
		if (MapManager.Instance != null && evolutionStages[stageIndex].stageGroundSprite != null)
		{
			MapManager.Instance.ChangeMapTheme(evolutionStages[stageIndex].stageGroundSprite);
		}
	}
}
