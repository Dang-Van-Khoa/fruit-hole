using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Fruit
{
    public Transform fruit;
    public AnimRunFruit animFruit;
    public Image image;
    public TypeFruit typeFruit;
    [HideInInspector] public Vector2 startPos;
    public Coroutine move;
    public Sequence jump;
    
    public void Reset()
    {
        jump?.Kill();
        move = null;
        fruit.position = startPos;
        fruit.gameObject.SetActive(true);
        Color c = image.color;
        c.a = 1;
        image.color = c;
    }
}

public enum TypeFruit
{
    Green, Orange
}