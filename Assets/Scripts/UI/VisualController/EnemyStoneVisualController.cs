using UnityEngine;
using System.Collections.Generic;

public class EnemyStoneVisualController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject _stonePrefab;
    [SerializeField] private GameObject _icePrefab;
    [SerializeField] private Vector3 _offset = new Vector3(0, 0.5f, 0);

    private MapSystem _mapSystem;
    private BattleSystem _battleSystem;

    private Dictionary<int, GameObject> _activeStoneObjects = new Dictionary<int, GameObject>();

    public void Initialize(MapSystem mapSystem, BattleSystem battleSystem)
    {
        _mapSystem = mapSystem;
        _battleSystem = battleSystem;

        _battleSystem.OnStoneUpdated += HandleStoneUpdated;
    }
    private void OnDestroy()
    {
        _battleSystem.OnStoneUpdated -= HandleStoneUpdated;
    }

    private void HandleStoneUpdated(List<int> stoneSectors, bool isCreated)
    {
        if (isCreated)
        {
            CreateNewStones(stoneSectors);
        }
        else
        {
            ClearAllStones();
        }
    }
    private void CreateNewStones(List<int> stoneSectors)
    {
        int stageID = 0;

        if (GameManager.Instance != null && GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard)
            stageID = GameManager.Instance.CurrentStageData.StageNumber;

        foreach (int sectorNum in stoneSectors)
        {
            if (_activeStoneObjects.ContainsKey(sectorNum)) continue;

            if (_mapSystem != null && _stonePrefab != null)
            {
                Vector3 spawnPos = _mapSystem.GetSectorPosition(sectorNum) + _offset;

                GameObject stoneObj = null;

                if (_icePrefab != null && stageID == 2 && GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard)
                {
                    stoneObj = Instantiate(_icePrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    stoneObj = Instantiate(_stonePrefab, spawnPos, Quaternion.identity);
                }


                SpriteRenderer sr = stoneObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = (sectorNum * 10) + 1;
                }

                stoneObj.transform.SetParent(_mapSystem.GetSectorTransform(sectorNum));
                stoneObj.name = $"Stone_Sector_{sectorNum}";

                _activeStoneObjects.Add(sectorNum, stoneObj);
                Debug.Log($"[EnemyStoneVisualController] {sectorNum}번 섹터에 돌 모델 생성");
            }
        }

        Play_Spawn_Sound();
    }
    private void ClearAllStones()
    {
        foreach (GameObject stone in _activeStoneObjects.Values)
        {
            if (stone != null)
                stone.GetComponent<Animator>().CrossFade("Stone_breaking",0.2f);
                //Destroy(stone);
        }
        _activeStoneObjects.Clear();

        Play_Break_Sound();

        Debug.Log("[EnemyStoneVisualController] 모든 돌 모델 제거 완료");
    }

    private void Play_Spawn_Sound()
    {
        bool is_ice = false;

        if (GameManager.Instance != null)
        {
            is_ice = (GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard) && GameManager.Instance.CurrentStageData.StageNumber == 2;
        }

        if (SoundManager.Instance != null)
        {
            SoundID id = is_ice ? SoundID.Ice : SoundID.Stone;

            SoundManager.Instance.Play(id);
        }
    }

    private void Play_Break_Sound() 
    {
        bool is_ice = false;

        if (GameManager.Instance != null)
        {
            is_ice = (GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard) && GameManager.Instance.CurrentStageData.StageNumber == 2;
        }

        if (SoundManager.Instance != null)
        {
            SoundID id = is_ice ? SoundID.Ice_Break : SoundID.Stone_Break;

            SoundManager.Instance.Play(id);
        }
    }

}
