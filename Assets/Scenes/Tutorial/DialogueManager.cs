using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 10)]
    public string sentence;
    public Vector2 bubblePosition;
    public GameObject targetIcon;

    [Header("오브젝트 제어 (필요할 때만 사용)")]
    public GameObject activeObject;      // 비워두면 아무것도 켜지 않습니다.
    public GameObject deactivateObject;  // 이번 단계 대사가 '나올 때' 꺼질 오브젝트
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public GameObject bubbleObject;
    public RectTransform bubbleRect;
    public GameObject clickBlocker;

    [Header("Tutorial Content")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    private Queue<TutorialStep> steps;
    private GameObject currentHighlight;

    void Awake()
    {
        steps = new Queue<TutorialStep>();
        bubbleObject.SetActive(false);
        if (clickBlocker != null) clickBlocker.SetActive(false);
    }

    void Start()
    {
        TriggerTutorial();
    }

    public void TriggerTutorial()
    {
        steps.Clear();
        foreach (var step in tutorialSteps)
        {
            steps.Enqueue(step);
        }

        bubbleObject.SetActive(true);
        if (clickBlocker != null) clickBlocker.SetActive(true);

        DisplayNextStep();
    }

    public void OnClickNext()
    {
        // 아이콘을 클릭해야 하는 단계(targetIcon 존재)라면 말풍선 클릭 무시
        if (currentHighlight != null) return;

        DisplayNextStep();
    }

    public void DisplayNextStep()
    {
        ResetHighlight();

        if (steps.Count == 0)
        {
            EndTutorial();
            return;
        }

        TutorialStep currentStep = steps.Dequeue();

        // 1. 오브젝트 켜기 (비어있으면 무시됨)
        if (currentStep.activeObject != null)
            currentStep.activeObject.SetActive(true);

        // 2. 오브젝트 끄기 
        // TIP: 아이콘을 더블클릭하자마자 다음 대사로 넘어올 때, 
        // 그 다음 대사 리스트에 자기 자신(패널)이 deactivateObject로 들어있으면 즉시 꺼집니다.
        // 그게 아니라면 여기서 정상적으로 작동합니다.
        if (currentStep.deactivateObject != null)
            currentStep.deactivateObject.SetActive(false);

        // 3. UI 갱신
        dialogueText.text = currentStep.sentence;
        bubbleRect.anchoredPosition = currentStep.bubblePosition;

        // 4. 하이라이트 설정
        if (currentStep.targetIcon != null)
        {
            SetHighlight(currentStep.targetIcon);
        }
    }

    public void SetHighlight(GameObject target)
    {
        currentHighlight = target;
        Canvas targetCanvas = target.GetComponent<Canvas>();
        if (targetCanvas == null) targetCanvas = target.AddComponent<Canvas>();

        targetCanvas.overrideSorting = true;
        targetCanvas.sortingOrder = 101;

        if (target.GetComponent<GraphicRaycaster>() == null)
            target.AddComponent<GraphicRaycaster>();
    }

    void ResetHighlight()
    {
        if (currentHighlight != null)
        {
            Canvas c = currentHighlight.GetComponent<Canvas>();
            if (c != null) c.overrideSorting = false;
            currentHighlight = null;
        }
    }

    public void RegisterDynamicTarget(GameObject target)
    {
        // 현재 대사 단계에서 하이라이트를 즉시 적용
        SetHighlight(target);
    }

    void EndTutorial()
    {
        bubbleObject.SetActive(false);
        if (clickBlocker != null) clickBlocker.SetActive(false);
        Debug.Log("튜토리얼 종료");
    }
}