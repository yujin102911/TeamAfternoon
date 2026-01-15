using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public enum DesktopTutorialCondition
{
    ButtonClicked,
    RecieveSignal,
}

[System.Serializable]
public class DesktopTutorialStep
{
    public DesktopTutorialCondition condition;
    [TabGroup("UI 설정")] public List<GameObject> hideUIs;
    [TabGroup("UI 설정")] public List<GameObject> showUIs;
    public string signal;
}


public class NailController : MonoBehaviour
{
    public static NailController Instance { get; private set; }

    [Title("튜토리얼 단계 설정")]
    public DesktopTutorialStep[] steps;
    private int currentIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void OnTutorialButtonClicked(string button)
    {
        if (currentIndex >= steps.Length) return;

        DesktopTutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition == DesktopTutorialCondition.ButtonClicked)
        {
            if (!string.IsNullOrEmpty(currentStep.signal) && currentStep.signal != button) return;

            CompleteStep();
        }
    }

    public void OnGetSignal(string signal)
    {
        if (currentIndex >= steps.Length) return ;

        DesktopTutorialStep currentStep = steps[currentIndex];
        if (currentStep.condition == DesktopTutorialCondition.RecieveSignal)
        {
            if (!string.IsNullOrEmpty(currentStep.signal) && currentStep.signal !=  signal) return;

            CompleteStep();
        }
    }

    public void CompleteStep()
    {
        Debug.Log($"{currentIndex} 단계 완료");
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

        DesktopTutorialStep currentStep = steps[index];

        if (currentStep.hideUIs != null)
        {
            foreach (var ui in currentStep.hideUIs)
                if (ui != null) ui.SetActive(false);
        }
        if (currentStep.showUIs != null)
        {
            foreach (var ui in currentStep.showUIs)
                if (ui != null) ui.SetActive(true);
        }
    }


}
