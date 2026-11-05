using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 材质纹理滚动组件，为血条、能量条等界面提供低成本动态流光。
/// </summary>
/// <remarks>
/// 运行时复制材质，避免修改所有使用同一材质的 Image；OnDestroy 中主动释放副本，
/// 防止频繁进入场景时产生材质内存泄漏。
/// </remarks>
[RequireComponent(typeof(Image))]
public class UIImageFillerFlow : MonoBehaviour
{
	[Header("纹理流动速度")]
	public float speedX = 0.5f;

	public float speedY = 0.2f;

	private Image uiImage;

	private Material m_Material;

	private Vector2 currentOffset = Vector2.zero;

	private void Start()
	{
		uiImage = GetComponent<Image>();
		if (uiImage != null && uiImage.material != null)
		{
			m_Material = new Material(uiImage.material);
			uiImage.material = m_Material;
		}
	}

	private void Update()
	{
		if (!(m_Material == null))
		{
			currentOffset.x += speedX * Time.deltaTime;
			currentOffset.y += speedY * Time.deltaTime;
			currentOffset.x %= 1f;
			currentOffset.y %= 1f;
			m_Material.SetTextureOffset("_MainTex", currentOffset);
		}
	}

	private void OnDestroy()
	{
		if (m_Material != null)
		{
			Object.Destroy(m_Material);
		}
	}
}
