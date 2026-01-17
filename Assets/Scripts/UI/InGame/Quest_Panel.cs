using UnityEngine;
using UnityEngine.UI;

public class Quest_Panel : MonoBehaviour
{
    [Header("아드레날린 UI")]
    [SerializeField]
    private Toggle _hitToggle;
    [SerializeField]
    private Image _hitCheckMark;
    [SerializeField]
    private Toggle _eightToggle;
    [SerializeField]
    private Image _eightMark;
    [SerializeField]
    private Toggle _twiceToggle;
    [SerializeField]
    private Image _twiceMark;

    [SerializeField]
    private Sprite _trueMark;
    [SerializeField]
    private Sprite _falseMark;

    private void Start()
    {
        TimelineManager.Instance.QuestOptionState.OnChanged += OnOptionChanged;
        TimelineManager.Instance.OnMarkActiveChanged += Active_Mark;

        Active_Mark(false);
    }

    private void OnDestroy()
    {
        TimelineManager.Instance.QuestOptionState.OnChanged -= OnOptionChanged;
        TimelineManager.Instance.OnMarkActiveChanged -= Active_Mark;
    }

    private void OnOptionChanged(QuestOptionState state)
    {
        _hitToggle.isOn = !state.IsHit;
        _eightToggle.isOn = state.IsEight;
        _twiceToggle.isOn = state.IsTwice;

        if (_hitToggle.isOn)
        {
            _hitCheckMark.sprite = _trueMark;
        }
        else
        {
            _hitCheckMark.sprite= _falseMark;
        }

        if (_eightToggle.isOn)
        {
            _eightMark.sprite = _trueMark;
        }
        else
        {
            _eightMark.sprite = _falseMark;
        }

        if (_twiceToggle.isOn)
        {
            _twiceMark.sprite = _trueMark;
        }
        else
        {
            _twiceMark.sprite = _falseMark;
        }
    }

    private void Active_Mark(bool is_on)
    {
        _hitCheckMark.gameObject.SetActive(is_on);
        _eightMark.gameObject.SetActive(is_on);
        _twiceMark.gameObject.SetActive(is_on);
    }
}
