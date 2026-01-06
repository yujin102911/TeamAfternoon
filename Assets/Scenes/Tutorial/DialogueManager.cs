using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 10)]
    public string sentence;          // 대사 내용
    public Vector2 bubblePosition;   // 말풍선 좌표
    public GameObject targetIcon;    // 하이라이트 아이콘 (직접 클릭 유도용)

    [Header("오브젝트 제어")]
    public GameObject activeObject;      // 이번 단계 시작 시 켤 오브젝트
    public GameObject deactivateObject;  // 이번 단계 시작 시 끌 오브젝트
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
        // 씬 시작 시 튜토리얼 자동 시작
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

    // 말풍선에 붙은 버튼이 호출할 함수
    public void OnClickNext()
    {
        // 하이라이트 대상이 있다면 말풍선 클릭으로 넘기기 방지
        if (currentHighlight != null) return;

        DisplayNextStep();
    }

    public void DisplayNextStep()
    {
        // 1. 이전 단계 하이라이트 해제
        ResetHighlight();

        if (steps.Count == 0)
        {
            EndTutorial();
            return;
        }

        // 2. 현재 단계 데이터 가져오기
        TutorialStep currentStep = steps.Dequeue();

        // 3. 오브젝트 활성화/비활성화 처리 (MSN 패널 제어 등)
        if (currentStep.activeObject != null)
            currentStep.activeObject.SetActive(true);

        if (currentStep.deactivateObject != null)
            currentStep.deactivateObject.SetActive(false);

        // 4. UI 갱신
        dialogueText.text = currentStep.sentence;
        bubbleRect.anchoredPosition = currentStep.bubblePosition;

        // 5. 하이라이트 설정
        if (currentStep.targetIcon != null)
        {
            SetHighlight(currentStep.targetIcon);
        }
    }

    void SetHighlight(GameObject target)
    {
        currentHighlight = target;

        // 블로커보다 위로 올리기
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

    void EndTutorial()
    {
        bubbleObject.SetActive(false);
        if (clickBlocker != null) clickBlocker.SetActive(false);
        Debug.Log("튜토리얼 종료");
    }
}