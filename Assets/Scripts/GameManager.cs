using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextAsset lv;
    
    [Button]
    private void ReadLevelData()
    {
        byte[] bytes = lv.bytes;

        Debug.Log("Length: " + bytes.Length);

        // In 20 byte đầu
        string hex = BitConverter.ToString(bytes, 0, Math.Min(bytes.Length, 64));
        Debug.Log("HEX: " + hex);
    }
}