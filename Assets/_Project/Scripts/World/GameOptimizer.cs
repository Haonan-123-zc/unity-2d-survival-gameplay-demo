using UnityEngine;

/// <summary>
/// 应用级运行参数初始化组件，在场景切换时保持唯一实例。
/// </summary>
/// <remarks>
/// 关闭垂直同步并设置目标帧率可以降低输入延迟，但发布构建仍应在目标机器上
/// 用 Profiler 校准；固定 144 帧并不适合所有显卡和移动设备。
/// </remarks>
public class GameOptimizer : MonoBehaviour
{
	private void Awake()
	{
		Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 144;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
