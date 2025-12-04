using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextAsset lv;
    [SerializeField] private List<Transform> points, fruitGreen, fruitOrange;
    [SerializeField] private Transform holeGreen, holeOrange;
    [SerializeField] private float radius;
    
    [Button]
    private void RunFruit()
    {
        var pointPos = points.Select(p => p.position).ToList();
        var fruitGreenPos = fruitGreen.Select(p => p.position).ToList();
        var fruitOrangePos = fruitOrange.Select(p => p.position).ToList();
        var targetGreen = RandomPointInsideCircle(holeGreen.position, radius);
        var targetOrange = RandomPointInsideCircle(holeOrange.position, radius);
        FindAndMove(fruitGreen, targetGreen);
    }
    [Button]
    private void RunFruitGreen()
    {
        var f = fruitGreen.First();
        var targetGreen = RandomPointInsideCircle(holeGreen.position, radius);
        var pointPos = points.Select(p => (Vector2)p.position).ToList();
        float step = Vector2.Distance(fruitGreen.First().position, fruitGreen[1].position);
        List<Vector2> path = FindPathGreen(f.position, targetGreen, pointPos, step, 0.1f);
        Debug.Log("Path: " + string.Join(",", path));
        // thực hiện di chuyển theo path
        StartCoroutine(MoveAlongPath(f, path));
    }

    private List<Vector2> FindPathGreen(Vector2 start, Vector2 goal, List<Vector2> pos, float step, float tolerance)
    {
        List<Vector2> path = new List<Vector2>();
        path.Add(start);
        for (int i = 0; i < pos.Count; i++)
        {
            Debug.Log("start: " + path.Last());
            var neighbors = GetNeighborsGreen(path.Last(), pos, step, tolerance);
            Debug.Log("GetNeighborsGreen: " + string.Join(",", neighbors));
            var p = neighbors.OrderBy(n => Vector2.Distance(n, goal)).First();
            path.Add(p);
            Debug.Log($"distance: {p}<<>>" + Vector2.Distance(path.Last(), goal));
            if (Vector2.Distance(path.Last(), goal) < radius * 2.1f) break;
        }
        
        return path;
    }
    
    List<Vector2> GetNeighborsGreen(Vector2 startPos, List<Vector2> pos, float step, float tolerance)
    {
        return pos.Where(p => Vector2.Distance(p, startPos) < step + tolerance).ToList();
    }
    private void FindAndMove(List<Transform> fruit, Vector2 targetPos)
    {
        if (fruit.Count < 1) return;
        float step = Vector2.Distance(fruit.First().position, fruit[1].position); // khoảng cách giữa các pointPos
        var pointPos = points.Select(p => (Vector2)p.position).ToList();
        foreach (var f in fruit)
        {
            List<Vector2> path = FindPath(f.position, targetPos, step, pointPos, 0.1f);
            Debug.Log("Path: " + string.Join(",", path));
            // thực hiện di chuyển theo path
            StartCoroutine(MoveAlongPath(f, path));
        }
    }
    List<Vector2> FindPath(Vector2 start, Vector2 goal, float step, List<Vector2> po, float tolerance)
    {
        Queue<Vector2> queue = new Queue<Vector2>();
        queue.Enqueue(start);

        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();

            if (current == goal)
                break;

            foreach (var next in GetNeighbors(current, step, po, tolerance))
            {
                if (!cameFrom.ContainsKey(next))
                {
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }
        }

        // reconstruct path
        List<Vector2> path = new List<Vector2>();

        if (!cameFrom.ContainsKey(goal))
            return path;

        Vector2 cur = goal;
        while (cur != start)
        {
            path.Add(cur);
            cur = cameFrom[cur];
        }

        path.Reverse();
        return path;
    }

    List<Vector2> GetNeighbors(Vector2 pos, float step, List<Vector2> po, float tolerance)
    {
        List<Vector2> result = new List<Vector2>();

        Vector2[] dirs =
        {
            new Vector2(step, 0),    // phải
            new Vector2(-step, 0),   // trái
            new Vector2(0, step),    // lên
            new Vector2(0, -step),   // xuống
        };

        foreach (var d in dirs)
        {
            Vector2 ideal = pos + d;
            Vector2 closest = FindClosestGridPoint(ideal, po, tolerance);

            // nếu lưới có điểm hợp lệ gần vị trí ideal
            if (Vector2.Distance(ideal, closest) <= tolerance)
                result.Add(closest);
        }

        return result;
    }
    Vector2 FindClosestGridPoint(Vector2 target, List<Vector2> po, float tolerance)
    {
        float minDist = float.MaxValue;
        Vector2 closest = target;

        foreach (var p in po)
        {
            float d = Vector2.Distance(p, target);
            if (d < minDist && d <= tolerance)
            {
                minDist = d;
                closest = p;
            }
        }

        return closest;
    }

    IEnumerator MoveAlongPath(Transform obj, List<Vector2> path)
    {
        foreach (var p in path)
        {
            while (Vector2.Distance(obj.position, p) > 0.01f)
            {
                obj.position = Vector2.MoveTowards(obj.position, p, Time.deltaTime * 3f);
                yield return null;
            }
        }
    }
    
    public Vector2 RandomPointInsideCircle(Vector2 center, float r)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Mathf.Sqrt(Random.Range(0f, 1f)) * r;
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }
}