using UnityEngine;
using TMPro;
using System.Text;

public class PlayerHeartUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private TextMeshProUGUI _heartText;

    [Header("설정")]
    [SerializeField] private string _filledHeart = "<color=red>♥️</color>";
    [SerializeField] private string _emptyHeart = "<color=white>♡</color>";

    [Header("위치 추적 설정")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 2.0f, 0f);

    private Transform _targetTransform;
    private Camera _mainCamera;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
    }

    public void SetFollowTarget(Transform target)
    {
        _targetTransform = target;
        UpdatePosition();
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_targetTransform == null) return; 
        if (_mainCamera == null) _mainCamera = Camera.main;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetTransform.position + _offset);
        transform.position = screenPos;
    }

    public void Init(int currentHP, int maxHP)
    {
        UpdateHearts(currentHP, maxHP);
    }

    public void UpdateHearts(int currentHP, int maxHP)
    {
        if (_heartText == null) return;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < currentHP; i++)
        {
            sb.Append(_filledHeart);
            sb.Append(" "); // 간격(필요없으면 빼면됨)
        }
        int emptyCount = maxHP - currentHP;
        for (int i = 0; i < emptyCount; i++)
        {
            sb.Append(_emptyHeart);
            sb.Append(" ");
        }
        _heartText.text = sb.ToString();
    }
}
