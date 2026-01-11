using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TutorialCondition 
{ 
    PlacedBlock, 
    HoverEnemySlot,
    ButtonClicked,
    FilterState,
    ChangeDirection,
    SliderTick,
    EffectPlaced,
    ComplexPlacement,
}

[System.Serializable]
public class TutorialStep
{
    public TutorialCondition condition;
    public string targetButtonName;
    public int targetTick;
    public int targetBlockID;
    public List<HandFilterType> targetFilters;
    public MoveDirection targetDirection;
    public EffectType targetEffectType;
}

public class BattleTutorialManager : MonoBehaviour
{
    public TutorialStep[] steps;
    private int currentIndex = 0;
    [SerializeField] private TimelineUI _timelineUI;
    [SerializeField] private FilmHand_Panel _handPanel;
    [SerializeField] private StepSlider _stepSlider;

    [Header("12 -> 19")]
    [SerializeField] private GameObject TwelveDesc;
    [SerializeField] private GameObject ThirteenDesc;
    [SerializeField] private GameObject FourteenDesc;
    [SerializeField] private GameObject FifteenDesc;
    [SerializeField] private GameObject SixteenDesc;
    [SerializeField] private GameObject SeventeenDesc;
    [SerializeField] private GameObject EighteenDesc;
    [SerializeField] private GameObject NineteenDesc;

    [Header("23 -> 35")]
    [SerializeField] private GameObject TwentythreeDesc;
    [SerializeField] private GameObject TwentyFourDesc;
    [SerializeField] private GameObject TwentyFiveDesc;
    [SerializeField] private GameObject TwentysixDesc;
    [SerializeField] private GameObject Desc_27;
    [SerializeField] private GameObject Desc_28;
    [SerializeField] private GameObject Desc_29;
    [SerializeField] private GameObject Desc_30;
    [SerializeField] private GameObject Desc_31;
    [SerializeField] private GameObject Desc_32;
    [SerializeField] private GameObject Desc_33;
    [SerializeField] private GameObject Desc_34;
    [SerializeField] private GameObject Desc_35;
    [SerializeField] private GameObject Nail_6;
    [SerializeField] private GameObject Nail_7;

    [Header("35 -> 59")]
    [SerializeField] private GameObject D_35;
    [SerializeField] private GameObject D_36;
    [SerializeField] private GameObject D_37;
    [SerializeField] private GameObject D_38;
    [SerializeField] private GameObject D_39;
    [SerializeField] private GameObject D_40;
    [SerializeField] private GameObject D_41;
    [SerializeField] private GameObject D_42;
    [SerializeField] private GameObject D_43;
    [SerializeField] private GameObject D_44;
    [SerializeField] private GameObject D_45;
    [SerializeField] private GameObject D_46;
    [SerializeField] private GameObject D_47;
    [SerializeField] private GameObject D_48;
    [SerializeField] private GameObject D_49;
    [SerializeField] private GameObject D_50;
    [SerializeField] private GameObject D_51;
    [SerializeField] private GameObject D_52;
    [SerializeField] private GameObject D_53;
    [SerializeField] private GameObject D_54;
    [SerializeField] private GameObject D_55;
    [SerializeField] private GameObject D_56;
    [SerializeField] private GameObject D_57;
    [SerializeField] private GameObject D_58;
    [SerializeField] private GameObject D_59;
    [SerializeField] private GameObject Nail_9;
    [SerializeField] private GameObject Nail_10;

    #region 튜토리얼 내부용 로직
    private void Start()
    {
        _timelineUI.OnEnemySlotHovered += CheckHoverCondition;
        _handPanel.OnFilterChanged += CheckFilterCondition;
        TimelineManager.Instance.OnTimelineChanged += CheckPlacementCondition;
        TimelineManager.Instance.OnTimelineChanged += CheckDirectionCondition;
        _stepSlider.OnStepSelected += CheckSliderCondition;
        TimelineManager.Instance.OnEffectChanged += CheckEffectCondition;
        TimelineManager.Instance.OnTimelineChanged += (blocks, prev) => CheckComplexCondition();
        TimelineManager.Instance.OnEffectChanged += (effects) => CheckComplexCondition();
    }

    public void OnTutorialButtonClicked(string button)
    {
        if (currentIndex >= steps.Length) return;

        TutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition == TutorialCondition.ButtonClicked)
        {
            if (!string.IsNullOrEmpty(currentStep.targetButtonName) && currentStep.targetButtonName != button)
                return;
            CompleteStep();
        }
    }

    private void CheckHoverCondition(int hoveredTick)
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep currentStop = steps[currentIndex];

        if (currentStop.condition == TutorialCondition.HoverEnemySlot && hoveredTick == currentStop.targetTick)
        {
            CompleteStep();
        }
    }

    private void CheckFilterCondition(HashSet<HandFilterType> currentFilters)
    {
        if (currentIndex >= steps.Length) return;

        TutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition != TutorialCondition.FilterState) return;

        if (currentFilters.Count != currentStep.targetFilters.Count) return;

        bool isMatch = true;
        foreach (HandFilterType target in currentStep.targetFilters)
        {
            if (!currentFilters.Contains(target))
            {
                isMatch = false;
                break;
            }
        }
        if (isMatch)
        {
            CompleteStep();
        }
    }

    private void CheckPlacementCondition(IReadOnlyList<PlacedBlock> placedBlocks, IReadOnlyList<PlacedBlock> prevBlocks)
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition != TutorialCondition.PlacedBlock) return;
        foreach (PlacedBlock block in placedBlocks)
        {
            if (block.linkedRuntimeBlock.BlockID == currentStep.targetBlockID &&
                block.startTick == currentStep.targetTick)
            {
                CompleteStep();
                break;
            }
        }
    }

    private void CheckDirectionCondition(IReadOnlyList<PlacedBlock> placedBlocks, IReadOnlyList<PlacedBlock> prevBlocks)
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition != TutorialCondition.ChangeDirection) return;
        foreach (PlacedBlock block in placedBlocks)
        {
            if (block.IsActiveAt(currentStep.targetTick))
            {
                if (block.linkedRuntimeBlock.BlockID == currentStep.targetBlockID)
                {
                    int localIndex = currentStep.targetTick - block.startTick;
                    MoveDirection currentDir = block.linkedRuntimeBlock.CurrentMoveDirections[localIndex];
                    if (currentDir == currentStep.targetDirection)
                    {
                        CompleteStep();
                        break;
                    }
                }
            }
        }

    }

    private void CheckSliderCondition(int currentTick)
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep currentStep = steps[currentIndex];

        if (currentStep.condition != TutorialCondition.SliderTick) return;

        if (currentTick == currentStep.targetTick)
        {
            CompleteStep();
        }
    }

    private void CheckEffectCondition(IReadOnlyList<Additional_Effect> currentEffects)
    {
        Debug.Log($"효과 변경 감지! 현재 스텝 조건: {steps[currentIndex].condition}, {currentEffects}");
        if (currentIndex >= steps.Length) return;
        TutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition != TutorialCondition.EffectPlaced) return;

        int effectIndex = currentStep.targetTick - 1;
        if (effectIndex >= 0 && effectIndex < currentEffects.Count)
        {
            Additional_Effect placedEffect = currentEffects[effectIndex];
            if (placedEffect != null && placedEffect.effectType == currentStep.targetEffectType)
            {
                CompleteStep();
            }
        }

    }

    private void CheckComplexCondition()
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep step = steps[currentIndex];
        if (step.condition != TutorialCondition.ComplexPlacement) return;
        // 1. 블록 조건 체크
        bool isBlockCorrect = TimelineManager.Instance.PlacedBlocks.Any(pb =>
            pb.linkedRuntimeBlock.BlockID == step.targetBlockID &&
            pb.startTick == step.targetTick);

        // 2. 특수효과 조건 체크
        bool isEffectCorrect = false;
        var currentEffects = TimelineManager.Instance.additional_Effects;
        int effectIdx = step.targetTick - 1;

        if (effectIdx >= 0 && effectIdx < currentEffects.Count)
        {
            if (currentEffects[effectIdx] != null &&
                currentEffects[effectIdx].effectType == step.targetEffectType)
            {
                isEffectCorrect = true;
            }
        }

        // 3. 두 조건이 모두 만족되면 다음 단계로!
        if (isBlockCorrect && isEffectCorrect)
        {
            CompleteStep();
        }
    }

    #endregion

    // ================ 스텝 넘어가는 함수들 모음
    private void CompleteStep()
    {
        Debug.Log($"{currentIndex}단계 완료");
        if (currentIndex == 1)
        {
            TwelveToThirteen();
        }
        else if (currentIndex == 2)
        {
            FourteenToFifteen();
        }
        else if (currentIndex == 3)
        {
            FifteenToSixteen();
        }
        else if (currentIndex == 4)
        {
            SixteenToSeventeen();
        }
        else if (currentIndex == 5)
        {
            SeventeenToEighteen();
        }
        else if (currentIndex == 6)
        {
            EighteenToNineteen();
        }
        else if (currentIndex == 7)
        {
            TwentythreeToTwentyFour();
        }
        else if (currentIndex == 8)
        {
            TwentyFourToTwentyFive();
        }
        else if (currentIndex == 9)
        {
            TwentyfiveToTwentysix();
        }
        else if (currentIndex == 11)
        {
            Change27_28();
        }
        else if (currentIndex == 12)
        {
            Change28_29();
        }
        else if (currentIndex == 13)
        {
            Change29_30();
        }
        else if (currentIndex == 15)
        {
            Change31_32();
        }
        else if (currentIndex == 17)
        {
            Change33_34();
        }
        else if (currentIndex == 18)
        {
            Change34_35();
        }
        else if (currentIndex == 19)
        {
            C35_36();
        }
        else if (currentIndex == 20)
        {
            C38_39();
        }
        else if (currentIndex == 21)
        {
            C39_40();
        }
        else if (currentIndex == 22)
        {
            C40_41();
        }
        else if (currentIndex == 23)
        {
            C41_42();
        }
        else if (currentIndex == 24)
        {
            C42_43();
        }
        else if (currentIndex == 25)
        {
            C43_44();
        }
        else if (currentIndex == 26)
        {
            C44_45();
        }
        else if (currentIndex == 27)
        {
            C45_46();
        }
        else if (currentIndex == 28)
        {
            C46_47();
        }
        else if(currentIndex == 29)
        {
            C51_52();
        }else if (currentIndex == 30)
        {
            C52_53();
        }else if(currentIndex == 31)
        {
            C53_54();
        }else if(currentIndex == 33)
        {
            C55_56();
        }else if(currentIndex == 34)
        {
            C58_59  ();
        }
            currentIndex++;
    }

    private void TwelveToThirteen()
    {
        TwelveDesc.SetActive(false);
        ThirteenDesc.SetActive(true);
    }

    private void FourteenToFifteen()
    {
        FourteenDesc.SetActive(false);
        FifteenDesc.SetActive(true);
    }

    private void FifteenToSixteen()
    {
        FifteenDesc.SetActive(false);
        SixteenDesc.SetActive(true);
    }

    private void SixteenToSeventeen()
    {
        SixteenDesc.SetActive(false);
        SeventeenDesc.SetActive(true);
    }

    private void SeventeenToEighteen()
    {
        SeventeenDesc.SetActive(false);
        EighteenDesc.SetActive(true);
    }

    private void EighteenToNineteen()
    {
        EighteenDesc.SetActive(false);
        NineteenDesc.SetActive(true);
    }

    private void TwentythreeToTwentyFour()
    {
        TwentythreeDesc.SetActive(false);
        TwentyFourDesc.SetActive(true);
    }

    private void TwentyFourToTwentyFive()
    {
        TwentyFourDesc.SetActive(false);
        TwentyFiveDesc.SetActive(true);
    }

    private void TwentyfiveToTwentysix()
    {
        TwentyFiveDesc.SetActive(false);
        TwentysixDesc.SetActive(true);
    }

    private void Change27_28()
    {
        Desc_27.SetActive(false);
        Desc_28.SetActive(true);
    }

    private void Change28_29()
    {
        Desc_28.SetActive(false);
        Desc_29.SetActive(true);
    }

    private void Change29_30()
    {
        Desc_29.SetActive(false);
        Desc_30.SetActive(true);
    }

    private void Change31_32()
    {
        Desc_31.SetActive(false);
        Desc_32.SetActive(true);
    }

    private void Change33_34()
    {
        Desc_33.SetActive(false);
        Desc_34.SetActive(true);
    }

    private void Change34_35()
    {
        Desc_34.SetActive(false);
        Desc_35.SetActive(true);
        Nail_6.SetActive(false);
        Nail_7.SetActive(true);
    }

    private void C35_36()
    {
        D_35.SetActive(false);
        D_36.SetActive(true);
    }

    private void C38_39()
    {
        D_38.SetActive(false);
        D_39.SetActive(true);
    }

    private void C39_40()
    {
        D_39.SetActive(false);
        D_40.SetActive(true);
    }

    private void C40_41()
    {
        D_40.SetActive(false);
        D_41.SetActive(true);
    }

    private void C41_42()
    {
        D_41.SetActive(false) ;
        D_42.SetActive(true) ;
    }

    private void C42_43()
    {
        D_42.SetActive(false) ;
        D_43.SetActive(true) ;
    }

    private void C43_44()
    {
        D_43.SetActive(false) ;
        D_44.SetActive(true) ;
    }

    private void C44_45()
    {
        D_44.SetActive(false) ;
        D_45.SetActive(true) ;
    }

    private void C45_46()
    {
        D_45.SetActive(false) ;
        D_46.SetActive(true) ;
    }

    private void C46_47()
    {
        D_46.SetActive(false) ;
        D_47.SetActive(true) ;
    }

    private void C51_52()
    {
        D_51.SetActive(false) ;
        D_52.SetActive(true) ;
    }

    private void C52_53()
    {
        D_52.SetActive(false);
        D_53.SetActive(true) ;
    }

    private void C53_54()
    {
        D_53.SetActive(false) ;
        D_54.SetActive(true) ;
    }

    private void C55_56()
    {
        Debug.Log("56으로 전환");
        D_55.SetActive(false) ;
        D_56.SetActive(true) ;
        Nail_9.SetActive(false) ;
        Nail_10.SetActive(true) ;
    }

    private void C58_59()
    {
        D_58.SetActive(false);
        D_59.SetActive(true) ;
    }

}
