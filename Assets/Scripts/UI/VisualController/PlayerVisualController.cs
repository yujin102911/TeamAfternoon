using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 플레이어 이동 비주얼을 담당하는 스크립트
/// GameManager가 갖고있음
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private float _moveDuration = 0.35f;

    private GameObject _playerInstance;
    private Coroutine _moveCoroutine;
    private MapSystem _mapSystem;

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
            Vector3 targetPos = _mapSystem.GetSectorPosition(sectorIndex);
            MoveTo(targetPos);
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
            Vector3 targetPos = _mapSystem.GetSectorPosition(sectorIndex);
            if (_playerInstance != null)
                Destroy(_playerInstance);
            if (_playerPrefab != null)
                _playerInstance = Instantiate(_playerPrefab, targetPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// 내부 이동 함수
    /// </summary>
    private void MoveTo(Vector3 targetPosition)
    {
        if (_playerInstance == null) return;
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(targetPosition));
    }

    private IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 startPosition = _playerInstance.transform.position;
        float elapsedTime = 0f;

        // 정해진 시간(_moveDuration) 동안 루프
        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            // 0 ~ 1 사이의 진행률(t) 계산
            float t = elapsedTime / _moveDuration;

            _playerInstance.transform.position = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }

        // 시간 끝나면 목표 지점에 정확히 안착
        _playerInstance.transform.position = target;
        _moveCoroutine = null;
    }
            
                
               

}
