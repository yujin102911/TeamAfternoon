using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    PlacementWithDirection,
    RoundExecutionFinished,
    FinalComplexCondition,
}

[System.Serializable]
public class TutorialStep
{
    public TutorialCondition condition;

    [TabGroup("UI 설정")] public List<GameObject> hideUIs; // 시작 시 끌 UI들
    [TabGroup("UI 설정")] public List<GameObject> showUIs; // 시작 시 켤 UI들

    public string targetButtonName;
    public int targetTick;
    public int targetBlockID;
    public int checkDirectionTick;
    public List<HandFilterType> targetFilters;
    public MoveDirection targetDirection;
    public EffectType targetEffectType;
    public string StepSummary
    {
        get
        {
            string summary = $"[{condition}] ";
            summary += condition switch
            {
                TutorialCondition.ButtonClicked => targetButtonName,
                TutorialCondition.HoverEnemySlot => $"Tick: {targetTick}",
                TutorialCondition.PlacedBlock => $"ID: {targetBlockID} at T{targetTick}",
                TutorialCondition.FilterState => $"Filters: {targetFilters?.Count ?? 0}",
                TutorialCondition.ChangeDirection => $"Tick: {targetTick} to {targetDirection}",
                TutorialCondition.PlacementWithDirection => $"Block: {targetBlockID} at T{targetTick} (Dir T{checkDirectionTick})",
                TutorialCondition.EffectPlaced => $"{targetEffectType} at T{targetTick}",
                _ => "Detail Data"
            };
            return summary;
        }
    }
}

public class BattleTutorialManager : MonoBehaviour
{
    [Title("튜토리얼 단계 설정")]
    [ListDrawerSettings(ListElementLabelName = "StepSummary", ShowIndexLabels = true)]
    public TutorialStep[] steps;
    private int currentIndex = 0;
    [SerializeField] private TimelineUI _timelineUI;
    [SerializeField] private FilmHand_Panel _handPanel;
    [SerializeField] private StepSlider _stepSlider;

    [Header("튜토리얼 끝나고 지급될 블록들")]
    [SerializeField] private List<int> _addBlocks;
    [SerializeField] private Button _lastButton;

    private void Awake()
    {
        _lastButton.onClick.AddListener(ButtonClicked);
    }


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
        TimelineManager.Instance.OnTimelineChanged += (blocks, prev) => CheckPlacementAndDirection();
        TimelineManager.Instance.OnExecutionFinished += CheckExecutionFinished;

        ApplyStepUI(0);
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

    private void CheckPlacementAndDirection()
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep step = steps[currentIndex];

        PlacedBlock targetBlock = TimelineManager.Instance.PlacedBlocks.FirstOrDefault(pb =>
        pb.linkedRuntimeBlock.BlockID == step.targetBlockID && pb.startTick == step.targetTick);

        if (targetBlock == null) return;

        int localIndex = step.checkDirectionTick - targetBlock.startTick;

        if (localIndex >= 0 && localIndex < targetBlock.linkedRuntimeBlock.CurrentMoveDirections.Length)
        {
            MoveDirection currentDir = targetBlock.linkedRuntimeBlock.CurrentMoveDirections[localIndex];
            if (currentDir == step.targetDirection)
            {
                CompleteStep();
            }
        }

    }

    private void CheckExecutionFinished()
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep currentStep = steps[currentIndex];

        if (currentStep.condition == TutorialCondition.RoundExecutionFinished)
        {
            CompleteStep();
        }
    }

    private void CheckFinalComplexCondition()
    {
        if (currentIndex >= steps.Length) return;
        TutorialStep step = steps[currentIndex];

        if (step.condition != TutorialCondition.FinalComplexCondition) return;

        PlacedBlock targetBlock = TimelineManager.Instance.PlacedBlocks.FirstOrDefault(pb =>
        pb.linkedRuntimeBlock.BlockID == step.targetBlockID &&
        pb.startTick == step.targetTick);

        if (targetBlock == null) return;

        int localIndex = step.checkDirectionTick - targetBlock.startTick;
        bool isDirectionCorrect = false;

        if (localIndex >= 0 && localIndex < targetBlock.linkedRuntimeBlock.CurrentMoveDirections.Length)
        {
            if (targetBlock.linkedRuntimeBlock.CurrentMoveDirections[localIndex] == step.targetDirection)
                isDirectionCorrect = true;
        }
        if (!isDirectionCorrect) return;
        bool isEffectCorrect = false;
        var currentEffects = TimelineManager.Instance.additional_Effects;
        int effectIdx = step.targetTick - 1; // 틱 번호 -> 리스트 인덱스 보정

        if (effectIdx >= 0 && effectIdx < currentEffects.Count)
        {
            var effect = currentEffects[effectIdx];
            if (effect != null && effect.effectType == step.targetEffectType)
            {
                isEffectCorrect = true;
            }
        }
        if (isEffectCorrect)
        {
            CompleteStep();
        }
    }

    private void ButtonClicked()
    {
        foreach (int block in _addBlocks)
        {
            ServiceLocator.Instance.CurrentUser.AddUnlockedBlock(block);
        }
    }

    #endregion

    // ================ 스텝 넘어가는 함수들 모음
    private void CompleteStep()
    {
        Debug.Log($"{currentIndex}단계 완료");
        if (currentIndex < steps.Length)
        {
            ApplyStepUI(currentIndex);
        }
        else
        {
            Debug.Log("모든 튜토리얼 종료");
        }
        currentIndex++;

    }


    private void ApplyStepUI(int index)
    {
        if (index >= steps.Length) return;

        TutorialStep currentStep = steps[index];

        // 1. 꺼야 할 UI들 처리
        if (currentStep.hideUIs != null)
        {
            foreach (var ui in currentStep.hideUIs)
                if (ui != null) ui.SetActive(false);
        }

        // 2. 켜야 할 UI들 처리
        if (currentStep.showUIs != null)
        {
            foreach (var ui in currentStep.showUIs)
                if (ui != null) ui.SetActive(true);
        }
    }
    

}
