using UnityEngine;

public class MinHeap
{
    Vector2Int[] items;
    int[] priorities;
    int count;

    public int Count => count;

    public MinHeap(int capacity)
    {
        items = new Vector2Int[capacity];
        priorities = new int[capacity];
        count = 0;
    }

    public void Push(Vector2Int item, int priority)
    {
        int i = count++;
        items[i] = item;
        priorities[i] = priority;

        while (i > 0)
        {
            int p = (i - 1) >> 1;
            if (priorities[p] <= priority) break;

            items[i] = items[p];
            priorities[i] = priorities[p];
            i = p;
        }

        items[i] = item;
        priorities[i] = priority;
    }

    public Vector2Int Pop()
    {
        Vector2Int root = items[0];
        int lastPriority = priorities[--count];
        Vector2Int lastItem = items[count];

        int i = 0;
        while (true)
        {
            int left = i * 2 + 1;
            if (left >= count) break;

            int right = left + 1;
            int smallest = (right < count && priorities[right] < priorities[left])
                ? right : left;

            if (priorities[smallest] >= lastPriority) break;

            items[i] = items[smallest];
            priorities[i] = priorities[smallest];
            i = smallest;
        }

        items[i] = lastItem;
        priorities[i] = lastPriority;

        return root;
    }
}