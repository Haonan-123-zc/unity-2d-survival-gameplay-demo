using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 分步新手教程控制器，以枚举状态驱动提示文本与暂停/恢复流程。
/// </summary>
/// <remarks>
/// 教程期间 timeScale = 0，仍使用 Update 处理 UI 选择状态。
/// 步骤切换集中在 <see cref="EnterStep"/>，方便后续替换为 ScriptableObject 对话数据。
/// </remarks>
public class TutorialManager : MonoBehaviour
{
	public enum TutorialStep
	{
		Welcome,
		MoveGuide,
		HpBarGuide,
		ExpBarGuide,
		WeaponFXGuide,
		Complete
	}

	[Header("当前教程步骤")]
	public TutorialStep currentStep;

	[Header("UI 组件")]
	public GameObject tutorialDialogObj;

	public TMP_Text tutorialText;

	public GameObject nextButton;

	private Slider hpSliderCache;

	private void Start()
	{
		if (tutorialDialogObj == null || tutorialText == null || nextButton == null)
		{
			Debug.LogError("[TutorialManager] 教程 UI 引用未配置完整。", this);
			return;
		}
		Time.timeScale = 0f;
		hpSliderCache = GameObject.Find("HpSlider")?.GetComponent<Slider>();
		if (hpSliderCache == null)
		{
			hpSliderCache = Object.FindAnyObjectByType<Slider>();
		}
		tutorialDialogObj.SetActive(value: true);
		EnterStep(TutorialStep.Welcome);
	}

	private void Update()
	{
		if (Time.timeScale != 0f)
		{
			return;
		}
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		Slider[] array = Object.FindObjectsByType<Slider>(FindObjectsSortMode.None);
		foreach (Slider slider in array)
		{
			if (!slider.gameObject.name.Contains("Tutorial"))
			{
				if (slider.gameObject.name.ToLower().Contains("hp") || slider.gameObject.name.ToLower().Contains("health"))
				{
					slider.value = slider.maxValue;
				}
				else
				{
					slider.value = 0f;
				}
			}
		}
	}

    /// <summary>
    /// 切换到指定教程步骤并刷新提示文案。
    /// </summary>
    /// <param name="step">目标步骤。</param>
	public void EnterStep(TutorialStep step)
	{
		currentStep = step;
		switch (currentStep)
		{
		case TutorialStep.Welcome:
			tutorialText.text = "欢迎来到原始人的世界！\n在这个充满危险的战场上，你需要变得更强。\n\n—— 点击continue开始学习基础生存技能 ——";
			nextButton.SetActive(value: true);
			break;
		case TutorialStep.MoveGuide:
			tutorialText.text = "在游戏，请使用键盘 【W、A、S、D】 控制你的原始人角色移动\n同时按E可以触发量子护盾，使你一定时间内无敌";
			nextButton.SetActive(value: true);
			break;
		case TutorialStep.HpBarGuide:
			tutorialText.text = "注意到界面上的【红色条】了吗？那是你的生命值！\n生命值归零游戏就会失败，一定要注意躲避鬼魂和怪物的撞击！";
			nextButton.SetActive(value: true);
			break;
		case TutorialStep.ExpBarGuide:
			tutorialText.text = "屏幕下方的【黄色条】是你的经验条！\n击败怪物会掉落红色的经验宝石，拾取它们填满黄色条，就能实现升级！";
			nextButton.SetActive(value: true);
			break;
		case TutorialStep.WeaponFXGuide:
			tutorialText.text = "每次升级时，你身上的旋转武器数量都会变多！\n当等级达到特定阶段，武器还会触发终极【进化】，涤荡一切尘埃！";
			nextButton.SetActive(value: true);
			break;
		case TutorialStep.Complete:
			tutorialText.text = "很好，基础生存法则你已全部掌握。\n去开启属于你的涤尘传说吧，原始人！";
			nextButton.SetActive(value: true);
			break;
		}
	}

    /// <summary>
    /// 处理“下一步”按钮；最后一步会结束教程并恢复游戏时间。
    /// </summary>
	public void OnClickNextButton()
	{
		if (currentStep == TutorialStep.Welcome)
		{
			EnterStep(TutorialStep.MoveGuide);
		}
		else if (currentStep == TutorialStep.MoveGuide)
		{
			EnterStep(TutorialStep.HpBarGuide);
		}
		else if (currentStep == TutorialStep.HpBarGuide)
		{
			EnterStep(TutorialStep.ExpBarGuide);
		}
		else if (currentStep == TutorialStep.ExpBarGuide)
		{
			EnterStep(TutorialStep.WeaponFXGuide);
		}
		else if (currentStep == TutorialStep.WeaponFXGuide)
		{
			EnterStep(TutorialStep.Complete);
		}
		else if (currentStep == TutorialStep.Complete)
		{
			FinishAndCloseTutorial();
		}
	}

	private void FinishAndCloseTutorial()
	{
		Time.timeScale = 1f;
		if (tutorialDialogObj != null)
		{
			tutorialDialogObj.SetActive(value: false);
		}
		base.gameObject.SetActive(value: false);
	}
}
