using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Mine_Difficulty { Easy, Normal, Hard, None }

[System.Serializable]
public struct DifficultySetting
{
    public Mine_Difficulty difficulty;
    public float Panel_Width; // UI 패널 크기 조정용
    public float Panel_Height;
    public int width;
    public int height;
    public int mines;
}

public class MinesweeperGridView : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    Mine_Difficulty startDifficulty = Mine_Difficulty.Easy;
    private Mine_Difficulty _lastDifficulty = Mine_Difficulty.None;

    [SerializeField] private MinesweeperHUD hud;

    [Header("Root_UI")]
    [SerializeField] private RectTransform _panelRoot;

    [Header("UI")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private RectTransform gridRoot;
    [SerializeField] private MineCell cellPrefab;

    [Header("Difficulties")]
    [SerializeField] private DifficultySetting[] settings;

    private readonly List<MineCell> cellViews = new();
    private BoardData board;

    private int curW, curH, curCount;
    private bool gameOver;

    private const int MAX_W = 30;
    private const int MAX_H = 16;
    private const int MAX_CELLS = MAX_W * MAX_H; // 480

    private void Awake()
    {
        WarmupCells(MAX_CELLS);
        
    }

    private void OnEnable()
    {
        if (_lastDifficulty == Mine_Difficulty.None)
            _lastDifficulty = startDifficulty;

        SetDifficulty(_lastDifficulty); // 기본 난이도
    }

    private void WarmupCells(int count)
    {
        // 최초 1회 생성
        for (int i = 0; i < count; i++)
        {
            var cell = Instantiate(cellPrefab, gridRoot);
            cell.Init(this);
            cellViews.Add(cell);
        }
    }

    public void Restart()
    {
        if (board == null) return;

        board.Reset();
        gameOver = false;

        // 현재 활성화된 셀들만 리셋
        for (int i = 0; i < curCount; i++)
            cellViews[i].ResetVisual();

        // HUD 리셋
        hud.ResetHUD(board.mineCount);
        hud.StopTimer();
    }

    public void Change_Difficulty(int i)
    {
        switch (i)
        {
            case 0:
                _lastDifficulty = Mine_Difficulty.Easy;
                SetDifficulty(Mine_Difficulty.Easy);
                break;

            case 1:
                _lastDifficulty = Mine_Difficulty.Normal;
                SetDifficulty(Mine_Difficulty.Normal);
                break;

            case 2:
                _lastDifficulty = Mine_Difficulty.Hard;
                SetDifficulty(Mine_Difficulty.Hard);
                break;
        }
    }


    public void SetDifficulty(Mine_Difficulty diff)
    {
        var s = GetSetting(diff);

        _panelRoot.sizeDelta = new Vector2(s.Panel_Width, s.Panel_Height);

        curW = s.width;
        curH = s.height;
        curCount = curW * curH;

        // GridLayoutGroup: 열 개수 = width
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = curW;

        // BoardData 새로 생성(또는 기존 보드 재사용하고 Reset)
        board = new BoardData(curW, curH, s.mines);
        gameOver = false;

        // 셀 활성/비활성 + 좌표 바인딩 + 비주얼 리셋
        for (int i = 0; i < cellViews.Count; i++)
        {
            bool active = i < curCount;
            cellViews[i].gameObject.SetActive(active);

            if (!active) continue;

            int x = i % curW;
            int y = i / curW;
            cellViews[i].Bind(x, y);
            cellViews[i].ResetVisual();
        }

        // HUD 리셋
        hud.ResetHUD(s.mines);
    }

    private DifficultySetting GetSetting(Mine_Difficulty diff)
    {
        for (int i = 0; i < settings.Length; i++)
            if (settings[i].difficulty == diff)
                return settings[i];

        // 못 찾으면 첫 번째
        return settings[0];
    }

    // --------- 입력 콜백 ----------
    public void OnCellLeftClick(int x, int y)
    {
        if (gameOver) return;

        // 첫 클릭이면 지뢰 생성(첫 클릭 안전)
        if (!board.generated)
        {
            board.GenerateMines(new Vector2Int(x, y));
            hud.StartTimer();
        }
            
        var cell = board.Get(x, y);

        // 이미 열린 칸이면 chord 시도
        if (cell.state == CellState.Revealed)
        {
            var chordChanged = board.Chord(x, y);
            ApplyChanged(chordChanged);

            if (board.exploded)
            {
                gameOver = true;
                hud.StopTimer();
            }

            return;
        }

        if (cell.state == CellState.Flagged || cell.state == CellState.Revealed)
            return;

        // 여기서 Reveal 로직(지뢰면 게임오버, 0이면 퍼짐)으로 이어지면 됨
        var changed = board.Reveal(x, y);
        ApplyChanged(changed);

        if (board.exploded)
        {
            // 게임오버 연출/입력잠금은 여기서
            hud.StopTimer();
            hud.SetFaceLose();
            return;
        }

        if (board.CheckWin())
        {
            // 클리어 처리
            hud.StopTimer();
            hud.SetFaceWin();
            //SteamAchievementManager.Unlock("");
        }

        Debug.Log($"[gridView] Left Clicked Cell ({cell.state})");
    }

    public void OnCellRightClick(int x, int y)
    {
        // 첫 클릭 전에도 깃발은 가능하게(보통 허용)
        var changed = board.ToggleFlag(x, y);
        ApplyChanged(changed);
        RefreshHudCounters();
    }

    public void SetChordPreview(int cx, int cy, bool on)
    {
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = cx + dx;
                int ny = cy + dy;

                if (nx < 0 || nx >= curW || ny < 0 || ny >= curH)
                    continue;

                int idx = ny * curW + nx;
                if (idx < 0 || idx >= curCount) continue;

                // Hidden 셀만 눌림 처리하고 싶으면 MineCell 내부에서 cover 체크하고 있으니 그대로 호출 OK
                cellViews[idx].SetNeighborPreview(on);
            }
    }

    private void ApplyChanged(List<Vector2Int> changed)
    {
        for (int i = 0; i < changed.Count; i++)
        {
            var p = changed[i];
            int idx = p.y * curW + p.x;
            if (idx < 0 || idx >= curCount) continue;

            cellViews[idx].Apply(board.Get(p.x, p.y));
        }
    }

    private void RefreshHudCounters()
    {
        hud.SetMinesRemaining(board.RemainingMines);
    }
}
