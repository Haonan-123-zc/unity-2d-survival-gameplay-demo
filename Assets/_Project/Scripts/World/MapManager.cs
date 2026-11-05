using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无限地图分块管理器，以玩家所在格为中心动态维护 5x5 地图块。
/// </summary>
/// <remarks>
/// 只有玩家跨过 tileSize 对应格点时才重建可见区域，避免每帧实例化地图。
/// 使用集合完成增删差集计算，保证已有地块不会被重复创建。
/// </remarks>
public class MapManager : MonoBehaviour
{
	public Transform player;

	public GameObject tilePrefab;

	public float tileSize = 10f;

	private Vector2 currentTilePos;

	private readonly Dictionary<Vector2, GameObject> activeTiles = new Dictionary<Vector2, GameObject>();

	private Sprite currentThemeSprite;

	public static MapManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start()
	{
		if (player == null || tilePrefab == null)
		{
			Debug.LogError("[MapManager] player 和 tilePrefab 必须在 Inspector 中配置。", this);
			enabled = false;
			return;
		}

		currentTilePos = GetTileCoordinate(player.position);
		UpdateMap();
	}

	private void Update()
	{
		Vector2 tilePosition = GetTileCoordinate(player.position);
		if (tilePosition != currentTilePos)
		{
			currentTilePos = tilePosition;
			UpdateMap();
		}
	}

	private Vector2 GetTileCoordinate(Vector3 worldPosition)
	{
		return new Vector2(
			Mathf.Round(worldPosition.x / tileSize),
			Mathf.Round(worldPosition.y / tileSize));
	}

	private void UpdateMap()
	{
		var desiredTiles = new HashSet<Vector2>();
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				Vector2 tilePosition = currentTilePos + new Vector2(i, j);
				desiredTiles.Add(tilePosition);
				if (activeTiles.ContainsKey(tilePosition))
				{
					continue;
				}

				GameObject gameObject = Object.Instantiate(
					position: new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0f),
					original: tilePrefab,
					rotation: Quaternion.identity,
					parent: base.transform);
				if (currentThemeSprite != null)
				{
					SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
					if (component != null)
					{
						component.sprite = currentThemeSprite;
					}
				}
				activeTiles.Add(tilePosition, gameObject);
			}
		}

		List<Vector2> expiredTiles = new List<Vector2>();
		foreach (KeyValuePair<Vector2, GameObject> activeTile in activeTiles)
		{
			if (!desiredTiles.Contains(activeTile.Key))
			{
				Object.Destroy(activeTile.Value);
				expiredTiles.Add(activeTile.Key);
			}
		}
		foreach (Vector2 tilePosition in expiredTiles)
		{
			activeTiles.Remove(tilePosition);
		}
	}

    /// <summary>
    /// 立即切换当前已加载地图块的贴图主题。
    /// </summary>
    /// <param name="newGroundSprite">新主题地面 Sprite。</param>
	public void ChangeMapTheme(Sprite newGroundSprite)
	{
		currentThemeSprite = newGroundSprite;
		foreach (KeyValuePair<Vector2, GameObject> activeTile in activeTiles)
		{
			if (activeTile.Value != null)
			{
				SpriteRenderer component = activeTile.Value.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					component.sprite = currentThemeSprite;
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
