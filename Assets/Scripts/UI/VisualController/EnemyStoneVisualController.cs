using UnityEngine;
using System.Collections.Generic;

public class EnemyStoneVisualController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject _GolemStonePrefab;
    [SerializeField] private GameObject _dragon1StonePrefab;
    [SerializeField] private GameObject _dragon2StonePrefab;
    [SerializeField] private GameObject _lightningStonePrefab;
    [SerializeField] private GameObject _icePrefab;
    [SerializeField] private GameObject _darkStonePrefab;
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

    private GameObject Choose_stone(int stage, bool is_hard)
    {
        if (is_hard)
        {
            switch (stage)
            {
                case 1:
                    return _lightningStonePrefab;
                case 2:
                    return _icePrefab;
                case 3:
                    return _darkStonePrefab;
                default:
                    return null;
            }
        }
        else
        {
            switch (stage)
            {
                case 4:
                    return _GolemStonePrefab;
                case 5:
                    return _dragon1StonePrefab;
                case 6:
                    return _dragon2StonePrefab;
                default:
                    return null;
            }
        }
    }

    private void CreateNewStones(List<int> stoneSectors)
    {
        GameObject stone = null;
        int stageID = 0;
        bool is_hard = false;

        if (GameManager.Instance != null)
        {
            stageID = GameManager.Instance.CurrentStageData.StageNumber;
            is_hard = GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard;
        }

        stone = Choose_stone(stageID, is_hard);

        foreach (int sectorNum in stoneSectors)
        {
            if (_activeStoneObjects.ContainsKey(sectorNum)) continue;

            if (_mapSystem != null && stone != null)
            {
                Vector3 spawnPos = _mapSystem.GetSectorPosition(sectorNum) + _offset;

                GameObject stoneObj = null;

                stoneObj = Instantiate(stone, spawnPos, Quaternion.identity);


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
