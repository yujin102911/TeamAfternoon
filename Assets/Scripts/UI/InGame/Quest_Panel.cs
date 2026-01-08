using UnityEngine;
using UnityEngine.UI;

public class Quest_Panel : MonoBehaviour
{
    [Header("아드레날린 UI")]
    [SerializeField]
    private Toggle _hitToggle;
    [SerializeField]
    private Toggle _eightToggle;
    [SerializeField]
    private Toggle _twiceToggle;

    private void Start()
    {
        TimelineManager.Instance.QuestOptionState.OnChanged += OnOptionChanged;
    }

    private void OnDestroy()
    {
        TimelineManager.Instance.QuestOptionState.OnChanged -= OnOptionChanged;
    }

    private void OnOptionChanged(QuestOptionState state)
    {
        _hitToggle.isOn = !state.IsHit;
        _eightToggle.isOn = state.IsEight;
        _twiceToggle.isOn = state.IsTwice;
    }
}
