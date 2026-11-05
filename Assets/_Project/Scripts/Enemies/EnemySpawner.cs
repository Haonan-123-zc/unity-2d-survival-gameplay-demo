using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时敌人生成器，按玩家等级逐步解锁敌人，并在相机外侧生成障碍物。
/// </summary>
/// <remarks>
/// 生成位置只依赖相机正交尺寸与宽高比，因此不会把敌人直接投放到可见区域。
/// 敌人生成采用等级驱动的概率与数量曲线；障碍物生成额外使用物理重叠检测，
/// 最多尝试三次，避免对象叠放导致寻路和碰撞异常。
/// </remarks>
public class EnemySpawner : MonoBehaviour
{
	[Header("怪物预制体")]
	public GameObject zombiePrefab;

	public GameObject impPrefab;

	public GameObject skeletonPrefab;

	public GameObject ghostPrefab;

	[Header("鬼魂生成配置")]
	[Range(0f, 1f)]
	public float ghostSpawnChance = 0.1f;

	public int maxGhostOnScreen = 3;

	[Header("原有基础配置")]
	public Transform player;

	public float baseSpawnInterval = 1.5f;

	[Header("屏幕外生成缓冲区")]
	public float screenBuffer = 3f;

	[Header("可交互树木生成配置")]
	public GameObject treePrefab;

	public int maxTreesOnScreen = 20;

	public float treeCheckInterval = 2f;

	[Range(0f, 1f)]
	public float treeSpawnChance = 0.3f;

	[Header("石头生成配置")]
	public GameObject stonePrefab;

	public int maxStonesOnScreen = 15;

	public float stoneCheckInterval = 3f;

	[Range(0f, 1f)]
	public float stoneSpawnChance = 0.2f;

	[Header("障碍物防重叠配置")]
	public float obstacleAvoidRadius = 2.5f;

	private float timer;

	private float treeTimer;

	private float stoneTimer;

	private Camera mainCamera;

	private PlayerStats playerStats;

	private readonly List<GameObject> unlockedEnemies = new List<GameObject>();

	private void Start()
	{
		mainCamera = Camera.main;
		playerStats = Object.FindFirstObjectByType<PlayerStats>();
		if (playerStats != null && player == null)
		{
			player = playerStats.transform;
		}
		if (player == null)
		{
			GameObject gameObject = GameObject.Find("Player");
			if (gameObject != null)
			{
				player = gameObject.transform;
			}
		}
		const int initialObstacleCount = 8;
		for (int i = 0; i < initialObstacleCount; i++)
		{
			TrySpawnObstacle();
		}
	}

	private void Update()
	{
		if (player == null || playerStats == null || mainCamera == null)
		{
			return;
		}
        // 等级越高，生成间隔越短，但保留 0.4 秒下限以限制峰值对象数量。
		float currentInterval = baseSpawnInterval - (playerStats.currentLevel - 1) * 0.05f;
		currentInterval = Mathf.Max(currentInterval, 0.4f);
		timer += Time.deltaTime;
		if (timer >= currentInterval)
		{
			int spawnCount = Mathf.Min(1 + playerStats.currentLevel / 2, 6);
			for (int i = 0; i < spawnCount; i++)
			{
				SpawnEnemy();
			}
			timer = 0f;
		}
		treeTimer += Time.deltaTime;
		if (treeTimer >= treeCheckInterval)
		{
			treeTimer = 0f;
			if (Object.FindObjectsByType<InteractiveTree>(FindObjectsSortMode.None).Length < maxTreesOnScreen && Random.value <= treeSpawnChance)
			{
				TrySpawnObstacle();
			}
		}
		stoneTimer += Time.deltaTime;
		if (!(stoneTimer >= stoneCheckInterval))
		{
			return;
		}
		stoneTimer = 0f;
		int num = 0;
		GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].name.Contains("Obstacle_Stone"))
			{
				num++;
			}
		}
		if (num < maxStonesOnScreen && Random.value <= stoneSpawnChance)
		{
			TrySpawnObstacle();
		}
	}

	private void SpawnEnemy()
	{
		unlockedEnemies.Clear();
		if (zombiePrefab != null)
		{
			unlockedEnemies.Add(zombiePrefab);
		}
		if (playerStats != null && playerStats.currentLevel >= 5 && impPrefab != null)
		{
			unlockedEnemies.Add(impPrefab);
		}
		if (playerStats != null && playerStats.currentLevel >= 10 && skeletonPrefab != null)
		{
			unlockedEnemies.Add(skeletonPrefab);
		}
		if (unlockedEnemies.Count == 0)
		{
			return;
		}
		GameObject selectedPrefab = unlockedEnemies[Random.Range(0, unlockedEnemies.Count)];
		GhostController[] activeGhosts = Object.FindObjectsByType<GhostController>(FindObjectsSortMode.None);
		if (playerStats != null && playerStats.currentLevel >= 15 && ghostPrefab != null && activeGhosts.Length < maxGhostOnScreen && Random.value <= ghostSpawnChance)
		{
			selectedPrefab = ghostPrefab;
		}
		float orthographicSize = mainCamera.orthographicSize;
		float horizontalExtent = orthographicSize * mainCamera.aspect + screenBuffer;
		float verticalExtent = orthographicSize + screenBuffer;
		Vector3 position = player.position;
        // 随机选择四条屏幕边缘之一，避免敌人从相机内部突然出现。
		switch (Random.Range(0, 4))
		{
		case 0:
			position += new Vector3(horizontalExtent, Random.Range(-verticalExtent, verticalExtent), 0f);
			break;
		case 1:
			position += new Vector3(-horizontalExtent, Random.Range(-verticalExtent, verticalExtent), 0f);
			break;
		case 2:
			position += new Vector3(Random.Range(-horizontalExtent, horizontalExtent), verticalExtent, 0f);
			break;
		case 3:
			position += new Vector3(Random.Range(-horizontalExtent, horizontalExtent), -verticalExtent, 0f);
			break;
		}
		GameObject gameObject = Object.Instantiate(selectedPrefab, position, Quaternion.identity);
		if (playerStats != null)
		{
            // 敌人生命随等级线性增长，保持数量增加之外的持续压力。
			float healthScale = 1f + (playerStats.currentLevel - 1) * 0.15f;
			EnemyController component = gameObject.GetComponent<EnemyController>();
			if (component != null)
			{
				component.health = Mathf.RoundToInt(component.health * healthScale);
			}
			GhostController component2 = gameObject.GetComponent<GhostController>();
			if (component2 != null)
			{
				component2.health = Mathf.RoundToInt(component2.health * healthScale);
			}
		}
	}

	private void TrySpawnObstacle()
	{
		GameObject gameObject = ((Random.value > 0.5f) ? treePrefab : stonePrefab);
		if (gameObject == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			float orthographicSize = mainCamera.orthographicSize;
			float num = orthographicSize * mainCamera.aspect;
			float num2 = screenBuffer + 6f;
			float num3 = num + num2;
			float num4 = orthographicSize + num2;
			Vector3 position = player.position;
			switch (Random.Range(0, 4))
			{
			case 0:
				position += new Vector3(num3 + Random.Range(0f, 4f), Random.Range(0f - num4, num4), 0f);
				break;
			case 1:
				position += new Vector3(0f - (num3 + Random.Range(0f, 4f)), Random.Range(0f - num4, num4), 0f);
				break;
			case 2:
				position += new Vector3(Random.Range(0f - num3, num3), num4 + Random.Range(0f, 4f), 0f);
				break;
			case 3:
				position += new Vector3(Random.Range(0f - num3, num3), 0f - (num4 + Random.Range(0f, 4f)), 0f);
				break;
			}
			if (Physics2D.OverlapCircle(position, obstacleAvoidRadius) == null)
			{
				Object.Instantiate(gameObject, position, Quaternion.identity);
				break;
			}
		}
	}
}
