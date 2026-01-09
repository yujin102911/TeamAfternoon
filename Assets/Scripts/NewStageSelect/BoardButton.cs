using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BoardButton : MonoBehaviour
    ,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _buttonTitleText;

    [Header("폰트 설정")]
    [SerializeField] private TMP_FontAsset regul;
    [SerializeField] private TMP_FontAsset bold;

    [Header("스케일 설정")]
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

    public void Setup(StageData stage ,BoardPanel panel)
    {
        _stageData = stage;
        _panel = panel;
        _buttonTitleText.text = $"Day {stage.StageNumber}";

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => _panel.SelectDay(_stageData));
    }

    public void UpdateVisual(bool isSelected)
    {
        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser == null) return;

        bool isRead = currentUser.IsBoardRead(_stageData.StageNumber);

        _buttonTitleText.font = isSelected ? bold : regul;
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
        return _stageData.StageNumber;
    }

}
