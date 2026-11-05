using UnityEngine;

/// <summary>
/// 将挂载物同步到指定骨骼节点，并在出现无效坐标时恢复初始姿态。
/// </summary>
/// <remarks>
/// 主要用于挂接武器或特效。同步放在 LateUpdate 中，
/// 能确保骨骼动画已经完成后再计算最终位置。
/// </remarks>
public class ForceSkinSync : MonoBehaviour
{
	[Header("目标骨骼")]
	public Transform targetBone;

	private Vector3 initialLocalPos;

	private Quaternion initialLocalRot;

	private void Start()
	{
		if (targetBone == null)
		{
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name.Contains("bone_2") || transform.name.Contains("bone_1"))
				{
					targetBone = transform;
					break;
				}
			}
		}
		initialLocalPos = base.transform.localPosition;
		initialLocalRot = base.transform.localRotation;
	}

	private void LateUpdate()
	{
		if (!(targetBone == null))
		{
			base.transform.position = targetBone.position;
			base.transform.rotation = targetBone.rotation;
			if (float.IsNaN(base.transform.position.x) || float.IsNaN(base.transform.position.y))
			{
				base.transform.localPosition = initialLocalPos;
				base.transform.localRotation = initialLocalRot;
			}
		}
	}
}
