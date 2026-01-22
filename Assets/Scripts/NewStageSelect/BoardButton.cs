using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class BoardButton : MonoBehaviour
    , IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _buttonTitleText;

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset EN_KR;
    [SerializeField] private TMP_FontAsset Ja_Zhan;

    [Header("Animation Settings")]
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _animationSpeed = 10f;

    private StageData _stageData;
    private BoardPanel _panel;

    private Vector3 _initialScale;
    private Coroutine _scaleCoroutine;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    // 로컬라이제이션 이벤트 구독 관련 함수들(OnEnable, OnDisable 등)을 모두 제거했습니다.

    public void Setup(StageData stage, BoardPanel panel)
    {
        _stageData = stage;
        _panel = panel;

        // 다시 직접 문자열을 조합하여 출력합니다.
        if (_buttonTitleText != null)
        {
            if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                _buttonTitleText.text = $"Day {stage.StageNumber}";
            }
            else if(ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                _buttonTitleText.text = $"<size=25>Week</size> {stage.StageNumber}";
            }

            Font_SetUp(LocalizationSettings.SelectedLocale);
        }

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => _panel.SelectDay(_stageData));
    }

    public void UpdateVisual(bool isSelected)
    {
        if (_stageData == null) return;

        // 선택 여부에 따른 폰트 및 크기 변경 로직 유지
        //_buttonTitleText.font = isSelected ? bold : regul;
        _buttonTitleText.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
        _buttonTitleText.fontSize = isSelected ? 36 : 32;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopScaleCoroutine();
        _scaleCoroutine = StartCoroutine(ScaleTo(_initialScale * _hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopScaleCoroutine();
        _scaleCoroutine = StartCoroutine(ScaleTo(_initialScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * _animationSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private void StopScaleCoroutine()
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
    }

    public int GetStageNumber()
    {
        return _stageData != null ? _stageData.StageNumber : -1;
    }

    private void Font_SetUp(Locale locale)
    {
        if (locale == null) return;

        string code = locale.Identifier.Code;

        if (code == "ja" || code == "zh-Hans")
        {
            _buttonTitleText.font = Ja_Zhan;
        }
        else
        {
            _buttonTitleText.font = EN_KR;
        }
    }
}