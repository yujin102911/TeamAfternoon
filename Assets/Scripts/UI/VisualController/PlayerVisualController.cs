using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 이동 비주얼을 담당하는 스크립트
/// GameManager가 갖고있음
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    [System.Serializable]
    public struct ActionPreviewMapping
    {
        public ActionType action;
        public Sprite visualSprite;
    }

    [Header("설정")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private float _moveDuration = 0.35f;
    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    [Header("프리뷰 설정")]
    [SerializeField] private GameObject _playerGhostPrefab;
    [SerializeField] private float _ghostYOffset = 0.8f;
    [SerializeField] private List<ActionPreviewMapping> _previewMappings;
 
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
    private ParticleSystem _playerAttackParticle;
    private GameObject _currentGhost;

    private Coroutine _moveCoroutine;
    private MapSystem _mapSystem;
    private BattleSystem _battleSystem;

    // 플레이어 애니메이터
    private Animator _playerAnimator;
    private string _currentAnimation = "";

    private PlayerAttectEffect _playerEffector;

    public Transform CurrentPlayerTransform => _playerInstance != null ? _playerInstance.transform : null;

    /// <summary>
    /// GameManager 가 호출
    /// </summary>
    public void Initialize(BattleSystem battleSystem, MapSystem mapSystem)
    {
        _mapSystem = mapSystem;
        _battleSystem = battleSystem;
        _battleSystem.OnEnemySideChanged += HandleEnemySideChanged;
    }

    public void OnDestroy()
    {
        _battleSystem.OnEnemySideChanged -= HandleEnemySideChanged;
    }

    /// <summary>
    /// 이벤트 구독용 함수
    /// OnMove이벤트가 발행될때마다 호출됨
    /// </summary>
    public void OnPlayerMoved(int sectorIndex, MoveDirection direction, bool is_wind = false)
    {
        if (_mapSystem != null)
        {
            Transform targetTransform = _mapSystem.GetSectorTransform(sectorIndex);
            if (targetTransform != null)
            {
                MoveTo(targetTransform, direction, is_wind);
                UpdatePlayerSortingOrder(sectorIndex);
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
            Vector3 targetPos = sectorTr.position + _offset;
            if (_playerInstance != null)
                Destroy(_playerInstance);

            if (_playerPrefab != null)
            {
                _playerInstance = Instantiate(_playerPrefab, targetPos, Quaternion.identity);
                _playerInstance.transform.SetParent(sectorTr);

                _playerEffector = _playerInstance.GetComponent<PlayerAttectEffect>();

                _playerAnimator = _playerInstance.GetComponentInChildren<Animator>();

                _playerRenderer = _playerInstance.GetComponentInChildren<SpriteRenderer>();

                UpdatePlayerSortingOrder(sectorIndex);

                _playerAttackParticle = _playerInstance.GetComponentInChildren<ParticleSystem>();
            }
        }
    }

    #region Visual Effects Methods - public

    public void PlayerFlip(bool isLeft)
    {
        if (_playerRenderer != null)
            _playerRenderer.flipX = !isLeft;

        Debug.Log($"[PlayerVisualController] 플레이어 플립: {(isLeft ? "왼쪽" : "오른쪽")}");

        if (_playerEffector != null)
        {
            _playerEffector.Flip_currentCharge();
        }
    }

    public void ChangeAnim(string animation)
    {
        if(_playerAnimator != null)
        {
            _currentAnimation = animation;
            _playerAnimator.CrossFade(animation, 0.2f);
        }

        if (animation == "4_Hurt" && SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.Player_hit);
    }

    public void Play_PlayerIdle()
    {
        if (_playerAnimator != null)
        {
            _playerAnimator.enabled = true;
        }
        UpdateFacing();
            //_playerAnimator.SetTrigger("Play");
    }

    public void Stop_PlayerIdle()
    {
        if (_playerAnimator != null)
        {
            _playerAnimator.enabled = false;
        }
        //_playerAnimator.SetTrigger("Pause");
    }

    public void Change_Trigger(string trigger)
    {
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger(trigger);
    }

    public void PlayAttackShake()
    {
        if (_playerInstance == null) return;

        StartCoroutine(ShakeRoutine(false));
    }

    public void PlayAttackEffect()
    {
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Attack_Sword");
        UpdateFacing();
        //트레일 렌더러 코드    
        //if (trailEffectPrefab != null)
        //{
        //    GameObject newTrail = Instantiate(
        //        trailEffectPrefab,
        //        Vector3.zero,
        //        Quaternion.identity
        //    );
        //}
        
    }

    public void PlayDeath()
    {
        if (_playerInstance == null) return;
        _playerAnimator.SetTrigger("Death");
        UpdateFacing();
    }

    public void PlaySwordAttack()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Attack_Sword");

    }

    public void PlayBowCharging()
    {
        if (_playerInstance == null) return;
        UpdateFacing() ;
            _playerAnimator.SetTrigger("Charging_Bow");
    }

    public void PlayBowMiddle()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        _playerAnimator.SetTrigger("Bow_Middle");
    }

    public void PlayBowAttack()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Attack_Bow");
    }

    public void PlayMeleeStart()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Sword_Charge");
    }

    public void PlayMeleeMiddle()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Sword_Middle");
    }



    public void PlayMeleeEnd()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Sword_End");
    }

    public void PlayGuard()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Guard");
    }

    public void PlayIdle()
    {
        if (_playerInstance == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
            _playerAnimator.SetTrigger("Idle");
    }

    public void PlayHitEffect()
    {
        if (_playerRenderer == null) return;
        UpdateFacing();
        if (_playerAnimator != null)
        {
            _playerAnimator.SetTrigger("Hurt");
        }
        else
        {
            StartCoroutine(HitFlashRoutine());
        }
    }

    public void PlayCureShake()
    {
        if (_playerInstance == null) return;
        if (_playerAttackParticle != null)
        {
            _playerAttackParticle.Play();
        }
        StartCoroutine(ShakeRoutine(true));
    }
    #endregion

    #region Private Effect Routines
    private IEnumerator ShakeRoutine(bool is_cure)
    {
        if (SoundManager.Instance != null && is_cure == true)
            SoundManager.Instance.Play(SoundID.SFX_Cure);

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
    public void ShowPlayerPreview(int sectorIndex, ActionType action, MoveDirection direction = MoveDirection.None, bool isEnemyLeft = false)
    {
        if (_mapSystem == null || sectorIndex <= 0)
        {
            HidePlayerPreview();
            return;
        }

        if (_playerInstance != null)
            _playerInstance.SetActive(false);

        if (_currentGhost == null && _playerGhostPrefab != null)
            _currentGhost = Instantiate(_playerGhostPrefab);

        if (_currentGhost != null)
        {
            Vector3 targetPos = _mapSystem.GetSectorPosition(sectorIndex);
            //targetPos.y += _ghostYOffset;
            targetPos += _offset;
            _currentGhost.transform.position = targetPos;
            _currentGhost.SetActive(true);
            SpriteRenderer ghostSR = _currentGhost.GetComponentInChildren<SpriteRenderer>();
            if (ghostSR != null)
            {
                ghostSR.sortingOrder = (sectorIndex * 10) + 3;
                ghostSR.flipX = GetFacingFlipWithSide(isEnemyLeft, direction);
            }
            UpdateGhostVisual(action);
        }
    }
    private void UpdateGhostVisual(ActionType currentAction)
    {
        Sprite targetSprite = null;
        foreach (ActionPreviewMapping mapping in _previewMappings)
        {
            if (mapping.action == currentAction)
            {
                targetSprite = mapping.visualSprite;
                break;
            }
        }
        if (_currentGhost != null)
        {
            SpriteRenderer ghostSR = _currentGhost.GetComponentInChildren<SpriteRenderer>();

            if (ghostSR != null)
            {
                if (targetSprite != null)
                {
                    ghostSR.sprite = targetSprite;
                }
                else
                {
                    Debug.LogWarning($"[PlayerVisualController] {currentAction}에 해당하는 프리뷰 스프라이트가 없습니다.");
                }
            }

        }
    }
    /// <summary>
    /// 고스트 숨기기
    /// </summary>
    public void HidePlayerPreview()
    {
        if (_currentGhost != null)
            _currentGhost.SetActive(false);

        if (_playerInstance != null)
            _playerInstance.SetActive(true);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 내부 이동 함수
    /// </summary>
    private void MoveTo(Transform targetSector, MoveDirection direction, bool is_wind = false)
    {
        if (_playerInstance == null) return;
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(targetSector, direction, is_wind));

    }

    private IEnumerator MoveRoutine(Transform targetSector, MoveDirection direction, bool is_wind)
    {
        if (is_wind)
        {
            ChangeAnim("8_Wind");
        }
        else
        {
            ChangeAnim("3_1_ Run");
            UpdateFacing(direction);
        }
            

        

        _playerInstance.transform.SetParent(null);

        Vector3 startPosition = _playerInstance.transform.position;
        float elapsedTime = 0f;

        _moveDuration = TimelineManager.Instance.Tick_interval * 0.85f;

        // 정해진 시간(_moveDuration) 동안 루프
        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            // 0 ~ 1 사이의 진행률(t) 계산
            float t = elapsedTime / _moveDuration;

            Vector3 currentDestPos = targetSector.position + _offset;

            _playerInstance.transform.position = Vector3.Lerp(startPosition, currentDestPos, t);
            yield return null;
        }

        // 시간 끝나면 목표 지점에 정확히 안착
        _playerInstance.transform.SetParent(targetSector);

        _playerInstance.transform.localPosition = _offset;

        _moveCoroutine = null;

        ChangeAnim("1_Idle");

        //if (_playerAnimator != null)
        //_playerAnimator.SetTrigger("Run_Stop");

        UpdateFacing();
    }

    // 플레이어 오더 인 레이어 조정
    private void UpdatePlayerSortingOrder(int sectorIndex)
    {
        if (_playerRenderer != null)
        {
            _playerRenderer.sortingOrder = (sectorIndex * 10) + 2;
        }
    }
    #endregion

    #region Player Direction Helper Methods
    private void HandleEnemySideChanged(bool isEnemyLeft)
    {
        // 이동 중이 아닐 때 즉시 방향 전환
        if (_moveCoroutine == null)
        {
            UpdateFacing();
        }
    }

    /// <summary>
    /// 플레이어가 바라봐야할 방향을 결정하는 함수
    /// </summary>
    private bool GetFacingFlip(MoveDirection direction = MoveDirection.None)
    {
        if (_battleSystem == null || _battleSystem.Enemies.Count == 0) return false;

        bool enemyIsLeft = _battleSystem.Enemies[0].IsLeft;
        bool shouldFaceLeft = enemyIsLeft;

        if (enemyIsLeft) // 적이 왼쪽에 있을 때 기준 (앞으로 이동 -> 뒤로 이동)
        {
            if (direction == MoveDirection.Front ||
                direction == MoveDirection.DiagonalLu ||
                direction == MoveDirection.DiagonalRu)
            {
                shouldFaceLeft = false;
            }
        }
        else // 적이 오른쪽에 있을 때 기준 (뒤로 이동 -> 뒤로 이동)
        {
            if (direction == MoveDirection.Back ||
                direction == MoveDirection.DiagonalLd ||
                direction == MoveDirection.DiagonalRd)
            {
                shouldFaceLeft = true;
            }
        }

        return shouldFaceLeft;
    }

    private bool GetFacingFlipWithSide(bool isEnemyLeft, MoveDirection direction)
    {
        bool shouldFaceLeft = isEnemyLeft;

        if (isEnemyLeft)
        {
            if (direction == MoveDirection.Front || direction == MoveDirection.DiagonalLu || direction == MoveDirection.DiagonalRu)
                shouldFaceLeft = false;
        }
        else
        {
            if (direction == MoveDirection.Back || direction == MoveDirection.DiagonalLd || direction == MoveDirection.DiagonalRd)
                shouldFaceLeft = true;
        }
        return shouldFaceLeft;
    }

    // 모든 액션 시작 시 호출하여 방향을 고정하는 ㅔㅁ서드
    public void UpdateFacing(MoveDirection direction = MoveDirection.None)
    {
        if (_playerRenderer != null)
            _playerRenderer.flipX = GetFacingFlip(direction);
    }
    #endregion

}
