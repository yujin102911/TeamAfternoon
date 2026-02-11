using UnityEngine;
using UnityEngine.UI;

public class ShortcutBootstrap : MonoBehaviour
{
    [SerializeField] private ShortcutManager _shortcuts;
    [SerializeField] private ScrollRect _handScroll;
    [SerializeField] private ScrollRect _effectScroll;

    [Header("필터 토글")]
    [SerializeField] private Toggle[] _filterToggles;

    [Header("최적화 조건")]
    [SerializeField]
    private GameObject _qeustPanel;
    [SerializeField]
    private GameObject _qeustBtn;

    [Header("패널 버튼")]
    [SerializeField]
    private Button _handPanelBtn;
    [SerializeField]
    private Button _effectPanelBtn;
    private Button _currentBtn;
    private void Awake()
    {
        _shortcuts.Register(new ScrollMoveCommand("ui.scroll.up", GetActiveScroll, +1000.0f));

        _shortcuts.Register(new ScrollMoveCommand("ui.scroll.down", GetActiveScroll, -1000.0f));
        // 여기서 다른 흩어진 기능들도 다 Register로 모음
        _shortcuts.Register(new ActionCommand(
            "ui.qeust",
            QeustPanel_Toggle
        ));

        _shortcuts.Register(new ActionCommand(
            "ui.tab",
            Panel_Toggle
        ));

        for (int i = 0; i < _filterToggles.Length; i++)
        {
            int index = i;

            _shortcuts.Register(
                new ToggleFilterCommand(
                    $"filter.toggle.{index}",
                    _filterToggles[index]
                )
            );
        }
    }

    private void QeustPanel_Toggle()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsBattleEnded)
        {
            return;
        }

        bool isActive = _qeustPanel.activeSelf;
        _qeustPanel.SetActive(!isActive);
        _qeustBtn.SetActive(isActive);
    }

    private void Panel_Toggle()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsBattleEnded))
        {
            return;
        }

        if (_handPanelBtn.gameObject.activeInHierarchy)
            _handPanelBtn.onClick.Invoke();
        else
            _effectPanelBtn.onClick.Invoke();
    }

    private ScrollRect GetActiveScroll()
    {
        if (_handScroll.gameObject.activeInHierarchy)
            return _handScroll;

        if (_effectScroll.gameObject.activeInHierarchy)
            return _effectScroll;

        return null;
    }
}
