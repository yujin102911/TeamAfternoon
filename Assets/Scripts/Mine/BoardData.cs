using System;
using System.Collections.Generic;
using UnityEngine;

public enum CellState { Hidden, Revealed, Flagged }

public class BoardData
{
    public readonly int width;
    public readonly int height;
    public readonly int mineCount;

    public bool generated { get; private set; }
    public bool exploded { get; private set; }   // 지뢰 클릭 여부

    private Cell[,] cells;

    public int FlagCount { get; private set; }
    public int RemainingMines => mineCount - FlagCount;


    public struct Cell
    {
        public bool isMine;
        public int adjacent;
        public CellState state;
    }

    public BoardData(int w, int h, int mines)
    {
        width = w;
        height = h;
        mineCount = mines;
        cells = new Cell[width, height];
        Reset();
    }

    public void Reset()
    {
        generated = false;
        exploded = false;

        FlagCount = 0;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                cells[x, y] = new Cell { isMine = false, adjacent = 0, state = CellState.Hidden };
    }

    public Cell Get(int x, int y) => cells[x, y];

    public bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    // --- 1) 첫 클릭 시 지뢰 생성 (3x3 안전영역 제외) ---
    public void GenerateMines(Vector2Int firstClick)
    {
        if (generated) return;

        var blocked = new HashSet<int>();
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = firstClick.x + dx;
                int ny = firstClick.y + dy;
                if (InBounds(nx, ny))
                    blocked.Add(ToIndex(nx, ny));
            }

        var candidates = new List<int>(width * height - blocked.Count);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int idx = ToIndex(x, y);
                if (!blocked.Contains(idx))
                    candidates.Add(idx);
            }

        Shuffle(candidates);
        int placeCount = Mathf.Min(mineCount, candidates.Count);

        for (int i = 0; i < placeCount; i++)
        {
            var (mx, my) = FromIndex(candidates[i]);
            var c = cells[mx, my];
            c.isMine = true;
            cells[mx, my] = c;
        }

        // adjacent 계산
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var c = cells[x, y];
                if (c.isMine) continue;

                int count = 0;
                ForEachNeighbor(x, y, (nx, ny) =>
                {
                    if (cells[nx, ny].isMine) count++;
                });

                c.adjacent = count;
                cells[x, y] = c;
            }

        generated = true;
    }

    // --- 2) 우클릭: 깃발 토글 ---
    public List<Vector2Int> ToggleFlag(int x, int y)
    {
        var changed = new List<Vector2Int>(1);
        if (!InBounds(x, y) || exploded) return changed;

        var c = cells[x, y];
        if (c.state == CellState.Revealed) return changed;

        if (c.state == CellState.Flagged)
        {
            c.state = CellState.Hidden;
            FlagCount--;
        }
        else
        {
            c.state = CellState.Flagged;
            FlagCount++;
        }

        cells[x, y] = c;

        changed.Add(new Vector2Int(x, y));
        return changed;
    }

    // --- 3) 좌클릭: Reveal + 0이면 퍼짐(BFS) ---
    // 반환: UI가 갱신해야 할 좌표들
    public List<Vector2Int> Reveal(int x, int y)
    {
        var changed = new List<Vector2Int>();
        if (!InBounds(x, y) || exploded) return changed;

        var start = cells[x, y];
        if (start.state == CellState.Flagged || start.state == CellState.Revealed) return changed;

        // 지뢰 클릭 -> 게임오버 처리
        if (start.isMine)
        {
            start.state = CellState.Revealed;
            cells[x, y] = start;
            changed.Add(new Vector2Int(x, y));

            exploded = true;
            changed.AddRange(RevealAllMines()); // 전체 지뢰 공개(선택)
            return changed;
        }

        // BFS Flood Fill
        var q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x, y));

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            var c = cells[p.x, p.y];

            if (c.state == CellState.Revealed || c.state == CellState.Flagged) continue;
            if (c.isMine) continue;

            c.state = CellState.Revealed;
            cells[p.x, p.y] = c;
            changed.Add(p);

            // 0이면 주변도 열기
            if (c.adjacent == 0)
            {
                ForEachNeighbor(p.x, p.y, (nx, ny) =>
                {
                    var nc = cells[nx, ny];
                    if (nc.state == CellState.Hidden && !nc.isMine)
                        q.Enqueue(new Vector2Int(nx, ny));
                });
            }
        }

        return changed;
    }

    public List<Vector2Int> Chord(int x, int y)
    {
        var changed = new List<Vector2Int>();

        if (!InBounds(x, y)) return changed;

        var center = cells[x, y];
        if (center.state != CellState.Revealed) return changed;
        if (center.adjacent <= 0) return changed;

        int flagCount = 0;

        ForEachNeighbor(x, y, (nx, ny) =>
        {
            if (cells[nx, ny].state == CellState.Flagged)
                flagCount++;
        });

        if (flagCount != center.adjacent)
            return changed;

        // 주변 자동 reveal
        ForEachNeighbor(x, y, (nx, ny) =>
        {
            var n = cells[nx, ny];
            if (n.state == CellState.Hidden)
            {
                changed.AddRange(Reveal(nx, ny));
            }
        });

        return changed;
    }


    // 게임오버 시 지뢰 공개
    public List<Vector2Int> RevealAllMines()
    {
        var changed = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var c = cells[x, y];
                if (!c.isMine) continue;
                if (c.state == CellState.Revealed) continue;

                c.state = CellState.Revealed;
                cells[x, y] = c;
                changed.Add(new Vector2Int(x, y));
            }
        return changed;
    }

    // 승리 판정: 안전칸이 전부 Revealed인지
    public bool CheckWin()
    {
        if (exploded) return false;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var c = cells[x, y];
                if (!c.isMine && c.state != CellState.Revealed)
                    return false;
            }
        return true;
    }

    // ---------- util ----------
    private void ForEachNeighbor(int x, int y, Action<int, int> action)
    {
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (InBounds(nx, ny))
                    action(nx, ny);
            }
    }

    private int ToIndex(int x, int y) => y * width + x;
    private (int x, int y) FromIndex(int idx) => (idx % width, idx / width);

    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
