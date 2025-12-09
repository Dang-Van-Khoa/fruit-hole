using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathFinding
{
    static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
public static List<CellGrid> FindPath(
    List<CellGrid> gridVector2,
    Vector2 start,
    Vector2 target,
    int width,
    int height, float cellSize)
{
    // ✅ Map Vector2Int -> CellGrid
    Dictionary<Vector2Int, CellGrid> cellMap = new();
    for (int i = 0; i < gridVector2.Count; i++)
    {
        int x = i % width;
        int y = i / width;
        if (y >= height)
            break;
        Vector2Int key = new Vector2Int(x, y);
        gridVector2[i].posInt = key;
        cellMap[key] = gridVector2[i];
    }

    Vector2 origin = gridVector2[0].pos; // cell (0,0)

    Vector2Int startI = WorldToGrid(start, origin, cellSize, width, height);
    Vector2Int targetI = WorldToGrid(target, origin, cellSize, width, height);
    //Vector2Int startI = gridVector2.OrderBy(g => Vector2.Distance(start, g.pos)).First().posInt;

    //Vector2Int targetI = gridVector2.OrderBy(g => Vector2.Distance(target, g.pos)).First().posInt;

    Dictionary<Vector2Int, Node> allNodes = new();
    List<Node> openSet = new();
    HashSet<Vector2Int> closedSet = new();

    Node startNode = new(startI)
    {
        gCost = 0,
        hCost = Heuristic(startI, targetI)
    };

    openSet.Add(startNode);
    allNodes[startI] = startNode;

    Node bestNode = startNode;

    while (openSet.Count > 0)
    {
        // lấy node có fCost thấp nhất
        Node current = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].fCost < current.fCost ||
               (openSet[i].fCost == current.fCost &&
                openSet[i].hCost < current.hCost))
            {
                current = openSet[i];
            }
        }

        openSet.Remove(current);
        closedSet.Add(current.pos);

        if (current.hCost < bestNode.hCost)
            bestNode = current;

        if (current.pos == targetI)
            return BuildPath(current, cellMap);

        foreach (var dir in directions)
        {
            Vector2Int next = current.pos + dir;

            if (next.x < 0 || next.x >= width ||
                next.y < 0 || next.y >= height)
                continue;

            if (!cellMap.TryGetValue(next, out var cell))
                continue;

            if (cell.isObstacle)
                continue;

            if (closedSet.Contains(next))
                continue;

            int newG = current.gCost + 1;

            if (!allNodes.TryGetValue(next, out Node neighbor))
            {
                neighbor = new Node(next);
                allNodes[next] = neighbor;
            }
            else if (newG >= neighbor.gCost && openSet.Contains(neighbor))
            {
                continue;
            }

            neighbor.gCost = newG;
            neighbor.hCost = Heuristic(next, targetI);
            neighbor.parent = current;

            if (!openSet.Contains(neighbor))
                openSet.Add(neighbor);
        }
    }

    // ❌ Không tới được target → trả đường gần nhất
    return BuildPath(bestNode, cellMap);
}
static Vector2Int WorldToGrid(
    Vector2 worldPos,
    Vector2 origin,
    float cellSize,
    int width,
    int height)
{
    int x = Mathf.RoundToInt(Mathf.Abs(worldPos.x - origin.x) / cellSize);
    int y = Mathf.RoundToInt(Mathf.Abs(worldPos.y - origin.y) / cellSize);

    x = Mathf.Clamp(x, 0, width - 1);
    y = Mathf.Clamp(y, 0, height - 1);

    return new Vector2Int(x, y);
}

static List<CellGrid> BuildPath(
    Node node,
    Dictionary<Vector2Int, CellGrid> cellMap)
{
    List<CellGrid> path = new();

    while (node != null)
    {
        path.Add(cellMap[node.pos]);
        node = node.parent;
    }

    path.Reverse();
    return path;
}

    public static List<Vector2Int> FindPath(int[,] grid, Vector2Int start, Vector2Int target)
{
    int width = grid.GetLength(1);
    int height = grid.GetLength(0);

    Dictionary<Vector2Int, Node> allNodes = new();
    List<Node> openSet = new();
    HashSet<Vector2Int> closedSet = new();

    Node startNode = new Node(start)
    {
        gCost = 0,
        hCost = Heuristic(start, target)
    };

    openSet.Add(startNode);
    allNodes[start] = startNode;

    // ⭐ Node gần target nhất từng đạt được
    Node bestNode = startNode;

    while (openSet.Count > 0)
    {
        Node current = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].fCost < current.fCost ||
               (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
            {
                current = openSet[i];
            }
        }

        openSet.Remove(current);
        closedSet.Add(current.pos);

        // ✅ Cập nhật node gần target nhất
        if (current.hCost < bestNode.hCost)
            bestNode = current;

        // 🎯 Đã tới mục tiêu
        if (current.pos == target)
            return BuildPath(current);

        foreach (var dir in directions)
        {
            Vector2Int nextPos = current.pos + dir;

            if (!IsValid(nextPos, grid, width, height))
                continue;

            if (closedSet.Contains(nextPos))
                continue;

            int newG = current.gCost + 1;

            if (!allNodes.TryGetValue(nextPos, out Node neighbor))
            {
                neighbor = new Node(nextPos);
                allNodes[nextPos] = neighbor;
            }
            else if (newG >= neighbor.gCost && openSet.Contains(neighbor))
            {
                continue;
            }

            neighbor.gCost = newG;
            neighbor.hCost = Heuristic(nextPos, target);
            neighbor.parent = current;

            if (!openSet.Contains(neighbor))
                openSet.Add(neighbor);
        }
    }

    // 🚑 Không tới được target → đi tới node gần nhất
    return BuildPath(bestNode);
}

    static bool IsValid(Vector2Int pos, int[,] grid, int w, int h)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= w || pos.y >= h)
            return false;

        return grid[pos.y, pos.x] == 0;
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    static List<Vector2Int> BuildPath(Node endNode)
    {
        List<Vector2Int> path = new();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.pos);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
}
public class Node
{
    public Vector2Int pos;
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
    public Node parent;

    public Node(Vector2Int pos)
    {
        this.pos = pos;
    }
}
