using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 이동 비주얼을 담당하는 스크립트
/// GameManager가 갖고있음
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private float _moveDuration = 0.35f;
    [SerializeField] private float _yOffset = 0.8f;

    [Header("프리뷰 설정")]
    [SerializeField] private GameObject _playerGhostPrefab;
    [SerializeField] private float _ghostYOffset = 0.8f;

    [Header("연출 설정")]
    [SerializeField] private float _hitFlashDuration = 0.2f;
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeAngle = 15f;
    [SerializeField] private int _shakeCount = 3;

    [Header("베기 이펙트 설정")]
    // 1. Hierarchy에 있는 SlashEffect 오브젝트를 여기에 드래그하여 연결합니다.
    public GameObject trailEffectPrefab;

    private GameObject _playerInstance;
    private SpriteRenderer _playerRenderer;
    private GameObject _currentGhost;

    private Coroutine _moveCoroutine;
    private MapSystem _mapSystem;

    public Transform CurrentPlayerTransform => _playerInstance != null ? _playerInstance.transform : null;

    /// <summary>
    /// GameManager 가 호출
    /// </summary>
    public void Initialize(MapSystem mapSystem)
    {
        _mapSystem = mapSystem;
    }

    /// <summary>
    /// 이벤트 구독용 함수
    /// OnMove이벤트가 발행될때마다 호출됨
    /// </summary>
    public void OnPlayerMoved(int sectorIndex)
    {
        if (_mapSystem != null)
        {
            Transform targetTransform = _mapSystem.GetSectorTransform(sectorIndex);
            if (targetTransform != null)
            {
                MoveTo(targetTransform);
            }
        }
    }

    /// <summary>
    /// 최초 플레이어 생성 함수
    /// GameManager가 호출
    /// </summary>
    public void SpawnPlayer(int sectorIndex)
    {
        if (_mapSystem != null)
        {
            Transform sectorTr = _mapSystem.GetSectorTransform(sectorIndex);
            if (sectorTr == null) return;

            Vector3 targetPos = sectorTr.position + new Vector3(0, _yOffset, 0);
            if (_playerInstance != null)
                Destroy(_playerInstance);

            if (_playerPrefab != null)
            {
                _playerInstance = Instantiate(_playerPrefab, targetPos, Quaternion.identity);
                _playerInstance.transform.SetParent(sectorTr);

                _playerRenderer = _playerInstance.GetComponentInChildren<SpriteRenderer>();
                if (_playerRenderer == null) _playerRenderer = _playerInstance.GetComponent<SpriteRenderer>();
            }
        }
    }

    #region Visual Effects Methods - public
    public void PlayAttackShake()
    {
        if (_playerInstance == null) return;
        StartCoroutine(ShakeRoutine());
    }

    public void PlayHitEffect()
    {
        if (_playerRenderer == null) return;
        StartCoroutine(HitFlashRoutine());
    }
    #endregion

    #region Private Effect Routines
    private IEnumerator ShakeRoutine()
    {
        //트레일 렌더러 코드    
        if (trailEffectPrefab != null)
        {
            GameObject newTrail = Instantiate(
                trailEffectPrefab,
                Vector3.zero,
                Quaternion.identity
            );
        }

        Quaternion originalRot = _playerInstance.transform.localRotation;

        float speed = _shakeDuration / _shakeCount;

        for (int i = 0; i < _shakeCount; i++)
        {
            float t = 0;
            while (t < speed / 2)
            {
                t += Time.deltaTime;
                float z = Mathf.Lerp(0, -_shakeAngle, t / (speed / 2));
                _playerInstance.transform.rotation = Quaternion.Euler(0, 0, z);
                yield return null;
            }
            t = 0;
            while (t < speed)
            {
                t += Time.deltaTime;
                float z = Mathf.Lerp(-_shakeAngle, _shakeAngle, t / speed);
                _playerInstance.transform.rotation = Quaternion.Euler(0, 0, z);
                yield return null;
            }
        }
        _playerInstance.transform.rotation = originalRot;
    }

    private IEnumerator HitFlashRoutine()
    {
        Color originalColor = Color.white;
        _playerRenderer.color = _hitColor;
        yield return new WaitForSeconds(_hitFlashDuration);
        _playerRenderer.color = originalColor;
    }
    #endregion

    #region Preview Visual Methods - public
    /// <summary>
    /// 고스트 위치 표시
    /// </summary>
    public void ShowPlayerPreview(int sectorIndex)
    {
        if (_mapSystem == null) return;

        if (_currentGhost == null && _playerGhostPrefab != null)
            _currentGhost = Instantiate(_playerGhostPrefab);
        if (_currentGhost != null)
        {
            Vector3 targetPos = _mapSystem.GetSectorPosition(sectorIndex);
            targetPos.y += _ghostYOffset;
            _currentGhost.transform.position = targetPos;
            _currentGhost.SetActive(true);
        }
    }
    /// <summary>
    /// 고스트 숨기기
    /// </summary>
    public void HidePlayerPreview()
    {
        if (_currentGhost != null)
            _currentGhost.SetActive(false);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 내부 이동 함수
    /// </summary>
    private void MoveTo(Transform targetSector)
    {
        if (_playerInstance == null) return;
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(targetSector));
    }

    private IEnumerator MoveRoutine(Transform targetSector)
    {
        _playerInstance.transform.SetParent(null);

        Vector3 startPosition = _playerInstance.transform.position;
        float elapsedTime = 0f;

        // 정해진 시간(_moveDuration) 동안 루프
        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            // 0 ~ 1 사이의 진행률(t) 계산
            float t = elapsedTime / _moveDuration;

            Vector3 currentDestPos = targetSector.position + new Vector3(0, _yOffset, 0);

            _playerInstance.transform.position = Vector3.Lerp(startPosition, currentDestPos, t);
            yield return null;
        }

        // 시간 끝나면 목표 지점에 정확히 안착
        _playerInstance.transform.SetParent(targetSector);

        _playerInstance.transform.localPosition = new Vector3(0, _yOffset, 0);

        _moveCoroutine = null;
    }
    #endregion



}
