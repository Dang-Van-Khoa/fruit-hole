using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextAsset lv;
    [Button]
    private void ReadLevelData()
    {
        TextAsset levelData = Resources.Load<TextAsset>("levelsencypted/levelsjson/Level1"); // bỏ .bytes
        if (levelData == null)
        {
            Debug.LogError("Không tìm thấy file trong Resources");
            return;
        }

        var data = levelData.bytes.ToString();

        // In 20 byte đầu tiên
        Debug.Log($"data level: {data}");
    }
}