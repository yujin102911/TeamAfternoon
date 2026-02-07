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
    [SerializeField] private Mine_Difficulty startDifficulty = Mine_Difficulty.Easy;
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

    // MineCell에서 체크하니 public 유지(프로퍼티로 바꿔도 OK)
    public bool gameOver;

    private const int MAX_W = 30;
    private const int MAX_H = 16;
    private const int MAX_CELLS = MAX_W * MAX_H; // 480

    public MineCell currentPressedCell { get; private set; }

    private void Awake()
    {
        WarmupCells(MAX_CELLS);
    }

    private void OnEnable()
    {
        if (_lastDifficulty == Mine_Difficulty.None)
            _lastDifficulty = startDifficulty;

        SetDifficulty(_lastDifficulty);
    }

    private void WarmupCells(int count)
    {
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

        ClearAllPreviews();

        board.Reset();
        gameOver = false;

        for (int i = 0; i < curCount; i++)
            cellViews[i].ResetVisual();

        hud.ResetHUD(board.mineCount);
        hud.StopTimer();
    }

    public void Change_Difficulty(int i)
    {
        switch (i)
        {
            case 0: _lastDifficulty = Mine_Difficulty.Easy; break;
            case 1: _lastDifficulty = Mine_Difficulty.Normal; break;
            case 2: _lastDifficulty = Mine_Difficulty.Hard; break;
            default: return;
        }

        SetDifficulty(_lastDifficulty);
    }

    public void SetDifficulty(Mine_Difficulty diff)
    {
        var s = GetSetting(diff);

        ClearAllPreviews();

        _panelRoot.sizeDelta = new Vector2(s.Panel_Width, s.Panel_Height);

        curW = s.width;
        curH = s.height;
        curCount = curW * curH;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = curW;

        board = new BoardData(curW, curH, s.mines);

        gameOver = false;

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

        hud.ResetHUD(s.mines);
        hud.StopTimer();
    }

    private DifficultySetting GetSetting(Mine_Difficulty diff)
    {
        for (int i = 0; i < settings.Length; i++)
            if (settings[i].difficulty == diff)
                return settings[i];

        return settings[0];
    }

    // =========================================================
    // 게임 입력
    // =========================================================

    public void OnCellLeftClick(int x, int y)
    {
        if (gameOver) return;

        if (!board.generated)
        {
            board.GenerateMines(new Vector2Int(x, y));
            hud.StartTimer();
        }

        var cell = board.Get(x, y);

        // ✅ 네 규칙: Revealed 칸 좌클릭이면 chord 시도
        if (cell.state == CellState.Revealed)
        {
            var chordChanged = board.Chord(x, y);
            ApplyChanged(chordChanged);

            if (board.exploded)
            {
                gameOver = true;
                hud.StopTimer();
                hud.SetFaceLose();
                return;
            }

            if (board.CheckWin())
            {
                gameOver = true;
                hud.StopTimer();
                hud.SetFaceWin();
                if (_lastDifficulty == Mine_Difficulty.Hard)
                    SteamAchievementManager.Unlock("ACHIEVEMENT_MINE_CLEAR");
                return;
            }

            return;
        }

        if (cell.state == CellState.Flagged)
            return;

        var changed = board.Reveal(x, y);
        ApplyChanged(changed);

        if (board.exploded)
        {
            gameOver = true;
            hud.StopTimer();
            hud.SetFaceLose();
            return;
        }

        if (board.CheckWin())
        {
            gameOver = true;
            hud.StopTimer();
            hud.SetFaceWin();
            if (_lastDifficulty == Mine_Difficulty.Hard)
                SteamAchievementManager.Unlock("ACHIEVEMENT_MINE_CLEAR");
        }
    }

    public void OnCellRightClick(int x, int y)
    {
        if (gameOver) return; // ✅ 추가

        var changed = board.ToggleFlag(x, y);
        ApplyChanged(changed);
        RefreshHudCounters();
    }

    // =========================================================
    // Chord Preview: 주변 8칸 눌림 표시
    // =========================================================

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

    // =========================================================
    // 드래그 프레스 UX (MineCell이 호출)
    // =========================================================

    public void BeginPress(MineCell cell)
    {
        if (gameOver || cell == null) return;

        if (currentPressedCell != null && currentPressedCell != cell)
            EndPressInternal(currentPressedCell);

        currentPressedCell = cell;

        cell.SetPreview(true);

        if (cell.IsRevealedNumberCell())
            SetChordPreview(cell.x, cell.y, true);
    }

    public void MovePress(MineCell cell)
    {
        if (gameOver || cell == null) return;
        if (currentPressedCell == cell) return;

        if (currentPressedCell != null)
            EndPressInternal(currentPressedCell);

        currentPressedCell = cell;

        cell.SetPreview(true);

        if (cell.IsRevealedNumberCell())
            SetChordPreview(cell.x, cell.y, true);
    }

    // MineCell의 PointerUp에서 호출: cancel=false이면 실행
    public void EndPress(MineCell cell, bool cancel)
    {
        if (gameOver || cell == null) return;
        if (currentPressedCell != cell) return;

        EndPressInternal(cell);

        if (cancel) return;

        OnCellLeftClick(cell.x, cell.y);
    }

    private void EndPressInternal(MineCell cell)
    {
        SetChordPreview(cell.x, cell.y, false);
        cell.SetPreview(false);

        if (currentPressedCell == cell)
            currentPressedCell = null;
    }

    private void ClearAllPreviews()
    {
        // 난이도 변경/재시작 때 프리뷰가 남는 것 방지
        for (int i = 0; i < cellViews.Count; i++)
        {
            if (!cellViews[i].gameObject.activeSelf) continue;

            cellViews[i].SetPreview(false);
            cellViews[i].SetNeighborPreview(false);
        }

        currentPressedCell = null;
    }

    public void CancelPress()
    {
        if (currentPressedCell == null)
            return;

        EndPress(currentPressedCell, cancel: true);
    }
}
