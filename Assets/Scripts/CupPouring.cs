using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CupPouring : MonoBehaviour
{
    [SerializeField] private RectTransform rtCup;
    [SerializeField] private Image imgCup;
    [SerializeField] private Vector3 rotate;

    private Sequence _sq;
    private void OnValidate()
    {
        rtCup = GetComponent<RectTransform>();
        imgCup = GetComponent<Image>();
    }

    private void OnEnable()
    {
        rtCup.rotation = Quaternion.Euler(Vector3.zero);
        _sq?.Kill();
        _sq = DOTween.Sequence().Append(imgCup.DOFade(1, 0.3f).From(0))
            .Join(rtCup.DOLocalRotate(rotate, 0.3f))
            .AppendInterval(0.3f)
            .Append(imgCup.DOFade(0, 0.3f).From(1))
            .AppendCallback(() => gameObject.SetActive(false));
    }
}