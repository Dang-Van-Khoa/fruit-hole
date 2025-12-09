using System;
using System.Collections.Generic;
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

    // ----------------- PriorityQueue (min-heap) -----------------
    public class PriorityQueue<T>
    {
        List<T> data;
        IComparer<T> comparer;

        public PriorityQueue(IComparer<T> comparer)
        {
            this.data = new List<T>();
            this.comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count => data.Count;

        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (comparer.Compare(data[ci], data[pi]) >= 0) break;
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            if (data.Count == 0) throw new InvalidOperationException("Queue empty");
            int li = data.Count - 1;
            T frontItem = data[0];
            data[0] = data[li];
            data.RemoveAt(li);
            --li;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && comparer.Compare(data[rc], data[ci]) < 0) ci = rc;
                if (comparer.Compare(data[pi], data[ci]) <= 0) break;
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
                pi = ci;
            }
            return frontItem;
        }

        public T Peek()
        {
            if (data.Count == 0) throw new InvalidOperationException("Queue empty");
            return data[0];
        }
    }

    // ----------------- Node & Comparer -----------------
    public class Node
    {
        public Vector2Int pos;
        public int gCost = int.MaxValue; // init large
        public int hCost;
        public int fCost => gCost + hCost;
        public Node parent;

        public Node(Vector2Int pos)
        {
            this.pos = pos;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node n) return pos == n.pos;
            if (obj is Vector2Int v) return pos == v;
            return false;
        }

        public override int GetHashCode()
        {
            return pos.x * 73856093 ^ pos.y * 19349663;
        }
    }

    // Compare nodes by fCost then hCost (min-heap)
    class NodeComparer : IComparer<Node>
    {
        public int Compare(Node a, Node b)
        {
            int cmp = a.fCost.CompareTo(b.fCost);
            if (cmp != 0) return cmp;
            return a.hCost.CompareTo(b.hCost);
        }
    }

    // ----------------- Helper functions -----------------
    static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // ----------------- FindPath for int[,] grid -----------------
    public static List<Vector2Int> FindPath(int[,] grid, Vector2Int start, Vector2Int target)
    {
        int width = grid.GetLength(1);
        int height = grid.GetLength(0);

        Dictionary<Vector2Int, Node> allNodes = new();
        Queue<Node> openHeap = new Queue<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> openSet = new HashSet<Vector2Int>(); // membership set

        Node startNode = new Node(start)
        {
            gCost = 0,
            hCost = Heuristic(start, target)
        };
        allNodes[start] = startNode;
        openHeap.Enqueue(startNode);
        openSet.Add(start);

        Node bestNode = startNode;

        while (openHeap.Count > 0)
        {
            Node current = openHeap.Dequeue();

            // if this node was already closed (we may have enqueued duplicates), skip
            if (closedSet.Contains(current.pos))
                continue;

            openSet.Remove(current.pos);
            closedSet.Add(current.pos);

            if (current.hCost < bestNode.hCost)
                bestNode = current;

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

                // if found better path to neighbor
                if (newG < neighbor.gCost)
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = Heuristic(nextPos, target);
                    neighbor.parent = current;

                    // enqueue updated node (we allow duplicates in heap)
                    openHeap.Enqueue(neighbor);
                    openSet.Add(nextPos);
                }
            }
        }

        // couldn't reach target — return path to bestNode
        return BuildPath(bestNode);
    }

    // ----------------- FindPath for List<CellGrid> variant -----------------
    public static List<CellGrid> FindPath(
        List<CellGrid> gridVector2,
        Vector2 start,
        Vector2 target,
        int width,
        int height, float cellSize)
    {
        // map
        Dictionary<Vector2Int, CellGrid> cellMap = new();
        for (int i = 0; i < gridVector2.Count; i++)
        {
            int x = i % width;
            int y = i / width;
            if (y >= height) break;
            Vector2Int key = new Vector2Int(x, y);
            gridVector2[i].posInt = key;
            cellMap[key] = gridVector2[i];
        }

        Vector2 origin = gridVector2[0].pos;
        Vector2Int startI = WorldToGrid(start, origin, cellSize, width, height);
        Vector2Int targetI = WorldToGrid(target, origin, cellSize, width, height);

        Dictionary<Vector2Int, Node> allNodes = new();
        Queue<Node> openHeap = new Queue<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> openSet = new HashSet<Vector2Int>();

        Node startNode = new Node(startI)
        {
            gCost = 0,
            hCost = Heuristic(startI, targetI)
        };
        allNodes[startI] = startNode;
        openHeap.Enqueue(startNode);
        openSet.Add(startI);

        Node bestNode = startNode;

        while (openHeap.Count > 0)
        {
            Node current = openHeap.Dequeue();

            if (closedSet.Contains(current.pos))
                continue;

            openSet.Remove(current.pos);
            closedSet.Add(current.pos);

            if (current.hCost < bestNode.hCost)
                bestNode = current;

            if (current.pos == targetI)
                return BuildPathCells(current, cellMap);

            foreach (var dir in directions)
            {
                Vector2Int next = current.pos + dir;

                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                    continue;

                if (!cellMap.TryGetValue(next, out var cell))
                    continue;

                if (cell.isObstacle) continue;

                if (closedSet.Contains(next)) continue;

                int newG = current.gCost + 1;

                if (!allNodes.TryGetValue(next, out Node neighbor))
                {
                    neighbor = new Node(next);
                    allNodes[next] = neighbor;
                }

                if (newG < neighbor.gCost)
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = Heuristic(next, targetI);
                    neighbor.parent = current;

                    openHeap.Enqueue(neighbor);
                    openSet.Add(next);
                }
            }
        }

        return BuildPathCells(bestNode, cellMap);
    }

    // ----------------- Utilities -----------------
    static bool IsValid(Vector2Int pos, int[,] grid, int w, int h)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= w || pos.y >= h) return false;
        return grid[pos.y, pos.x] == 0;
    }

    static Vector2Int WorldToGrid(Vector2 worldPos, Vector2 origin, float cellSize, int width, int height)
    {
        int x = Mathf.RoundToInt(Mathf.Abs(worldPos.x - origin.x) / cellSize);
        int y = Mathf.RoundToInt(Mathf.Abs(worldPos.y - origin.y) / cellSize);
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);
        return new Vector2Int(x, y);
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

    static List<CellGrid> BuildPathCells(Node node, Dictionary<Vector2Int, CellGrid> cellMap)
    {
        List<CellGrid> path = new();
        while (node != null)
        {
            if (cellMap.TryGetValue(node.pos, out var c))
                path.Add(c);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }
}
