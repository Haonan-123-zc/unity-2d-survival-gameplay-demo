using System;
using UnityEngine;

/// <summary>
/// 单个角色进化阶段的序列化配置。
/// </summary>
/// <remarks>
/// 每个阶段同时描述角色模型、武器外观和地图主题；
/// 由 <see cref="PlayerEvolutionManager"/> 按索引统一应用。
/// </remarks>
[Serializable]
public class EvolutionStage
{
	public string stageName;

	public GameObject stageObject;

	public GameObject stageWeaponPrefab;

	public Sprite stageGroundSprite;
}
