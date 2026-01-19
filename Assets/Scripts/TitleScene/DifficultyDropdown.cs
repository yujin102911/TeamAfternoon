using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class DifficultyDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    // 난이도 → 로컬라이즈 키 매핑
    private readonly Dictionary<Difficulty, string> keyMap = new()
    {
        { Difficulty.Easy, "TITLE_DIFFICULTY_NORMAL" },
        { Difficulty.Hard,   "TITLE_DIFFICULTY_HARD"   },
    };

    private List<Difficulty> values;

    void Awake()
    {
        values = new List<Difficulty>(keyMap.Keys);

        dropdown.onValueChanged.AddListener(OnChanged);

        BuildOptions();
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    void OnLocaleChanged(Locale _)
    {
        int prev = dropdown.value;      // 현재 선택 유지
        BuildOptions();
        dropdown.SetValueWithoutNotify(prev);
        dropdown.RefreshShownValue();
    }

    void BuildOptions()
    {
        dropdown.ClearOptions();

        values = new List<Difficulty>();
        values.Add(Difficulty.Easy); // 노말은 항상 포함

        if (ServiceLocator.Instance.GlobalData.IsGameCleared)
        {
            values.Add(Difficulty.Hard);
            Debug.Log("[DifficultyDropdown] 하드 모드가 포함되었습니다.");
        }
        else
        {
            Debug.Log("[DifficultyDropdown] 하드 모드가 제외되었습니다.");
        }

        var options = new List<string>(values.Count);

        // 동기 로딩(테이블이 프리로드되어 있거나 작은 규모면 OK)
        foreach (var diff in values)
        {
            var ls = new LocalizedString("Title Table", keyMap[diff]); // ✅ 테이블 이름 맞춰
            options.Add(ls.GetLocalizedString());
        }

        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();
    }

    void OnChanged(int index)
    {
        Difficulty selected = values[index];
        // 여기서 게임 설정에 반영 + 저장
        Debug.Log("Selected difficulty: " + selected);
    }
}
