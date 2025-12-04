using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimRunFruit : MonoBehaviour
{
    public List<Sprite> fruit;
    [SerializeField] private Image imgFruit;

    public bool isRunning;
    public int index = 0;
    public Coroutine animCo;

    public void StopCoroutineAnimCo()
    {
        if (animCo != null)
        {
            StopCoroutine(animCo);
            animCo = null;
        }
    }

    private IEnumerator PlayFruitAnim()
    {
        while (isRunning)
        {
            imgFruit.sprite = fruit[index];
            index = (index + 1) % fruit.Count;
            yield return new WaitForSeconds(0.025f);   // speed
        }
    }

    public void SetFruit(Sprite sprite)
    {
        imgFruit.sprite = sprite;
    }
    public void SetSpr(List<Sprite> f)
    {
        fruit = f;
    }

    private void OnValidate()
    {
        imgFruit = transform.GetComponent<Image>();
    }
}