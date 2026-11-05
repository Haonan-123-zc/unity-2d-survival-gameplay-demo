using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家经验和等级成长系统，并在升级时联动武器数量与角色进化。
/// </summary>
/// <remarks>
/// 经验值使用 while 循环结算，因此单次获得大量经验时也能连续升级，
/// 不会出现只升一级以及剩余经验被错误丢弃的问题。
/// </remarks>
public class PlayerStats : MonoBehaviour
{
	public int currentLevel = 1;

	public int currentExp;

	public int expToNextLevel = 50;

	[Header("UI 自动关联")]
	public Slider expSlider;

	public TextMeshProUGUI levelText;

	[Header("核心组件参考")]
	public OrbitWeaponController weaponController;

	private void Start()
	{
		if (expSlider == null)
		{
			expSlider = GameObject.Find("Slider")?.GetComponent<Slider>();
		}
		if (levelText == null)
		{
			levelText = GameObject.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
		}
		UpdateUI();
	}

    /// <summary>
    /// 增加经验并处理可能发生的连续升级。
    /// </summary>
    /// <param name="amount">本次获得的经验值。</param>
	public void GainExp(int amount)
	{
        if (amount <= 0)
        {
            return;
        }

		currentExp += amount;
		while (currentExp >= expToNextLevel)
		{
			LevelUp();
		}
		UpdateUI();
	}

	private void LevelUp()
	{
		currentLevel++;
		currentExp -= expToNextLevel;
		expToNextLevel += 20;
		UpdateUI();
		if (currentLevel % 5 == 0)
		{
			PlayerEvolutionManager component = GetComponent<PlayerEvolutionManager>();
			if (component != null)
			{
				component.EvolveToNextStage();
			}
			return;
		}
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(AudioManager.Instance.levelUpSound);
		}
		weaponController = GetComponentInChildren<OrbitWeaponController>();
		if (weaponController != null)
		{
			weaponController.weaponCount++;
			weaponController.rotateSpeed += 40f;
			weaponController.SpawnWeapons();
		}
	}

	private void UpdateUI()
	{
		if (expSlider != null)
		{
			expSlider.maxValue = expToNextLevel;
			expSlider.value = currentExp;
		}
		if (levelText != null)
		{
			levelText.text = "LV. " + currentLevel;
		}
	}
}
