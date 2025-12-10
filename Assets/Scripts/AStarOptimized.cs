using System.Collections.Generic;
using UnityEngine;

public static class AStarOptimized
{
    static readonly Vector2Int[] DIRS =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    struct Node
    {
        public int g;
        public int h;
        public Vector2Int parent;
        public bool hasParent;
    }

    public static List<Vector2Int> FindPath(
        int[,] grid,
        Vector2Int start,
        Vector2Int target)
    {
        int h = grid.GetLength(0);
        int w = grid.GetLength(1);

        Node[,] nodes = new Node[h, w];
        bool[,] closed = new bool[h, w];
        bool[,] inOpen = new bool[h, w];

        MinHeap open = new MinHeap(w * h);

        nodes[start.y, start.x].h = Heuristic(start, target);
        open.Push(start, nodes[start.y, start.x].h);
        inOpen[start.y, start.x] = true;

        Vector2Int best = start;
        int bestH = nodes[start.y, start.x].h;

        while (open.Count > 0)
        {
            Vector2Int cur = open.Pop();
            inOpen[cur.y, cur.x] = false;

            if (closed[cur.y, cur.x])
                continue;

            closed[cur.y, cur.x] = true;

            int ch = nodes[cur.y, cur.x].h;
            if (ch < bestH)
            {
                best = cur;
                bestH = ch;
            }

            if (cur == target)
                return BuildPath(nodes, cur);

            foreach (var d in DIRS)
            {
                Vector2Int nxt = cur + d;

                if (nxt.x < 0 || nxt.y < 0 ||
                    nxt.x >= w || nxt.y >= h)
                    continue;

                if (grid[nxt.y, nxt.x] != 0 ||
                    closed[nxt.y, nxt.x])
                    continue;

                int newG = nodes[cur.y, cur.x].g + 1;

                if (!inOpen[nxt.y, nxt.x] ||
                    newG < nodes[nxt.y, nxt.x].g)
                {
                    nodes[nxt.y, nxt.x].g = newG;
                    nodes[nxt.y, nxt.x].h = Heuristic(nxt, target);
                    nodes[nxt.y, nxt.x].parent = cur;
                    nodes[nxt.y, nxt.x].hasParent = true;

                    open.Push(nxt, nodes[nxt.y, nxt.x].g + nodes[nxt.y, nxt.x].h);
                    inOpen[nxt.y, nxt.x] = true;
                }
            }
        }

        return BuildPath(nodes, best);
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    static List<Vector2Int> BuildPath(Node[,] nodes, Vector2Int end)
    {
        List<Vector2Int> path = new();
        Vector2Int cur = end;
        path.Add(cur);

        while (nodes[cur.y, cur.x].hasParent)
        {
            cur = nodes[cur.y, cur.x].parent;
            path.Add(cur);
        }

        path.Reverse();
        return path;
    }
    
    public static Vector2Int WorldToGrid(
        Vector2 worldPos,
        Vector2 origin,
        Vector2 cellSize,
        int width,
        int height)
    {
        int x = Mathf.RoundToInt(Mathf.Abs(worldPos.x - origin.x) / cellSize.x);
        int y = Mathf.RoundToInt(Mathf.Abs(worldPos.y - origin.y) / cellSize.y);

        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);

        return new Vector2Int(x, y);
    }
    
    public static Vector2 GridToWorld(Vector2Int gridPos, Vector2 origin, Vector2 cellSize)
    {
        return origin + new Vector2(
            gridPos.x * cellSize.x,
            -gridPos.y * cellSize.y);
    }
}
