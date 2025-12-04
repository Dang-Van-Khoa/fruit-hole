using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CupPouring : MonoBehaviour
{
    [SerializeField] private RectTransform rtCup;
    [SerializeField] private Image imgCup;
    [SerializeField] private Vector3 rotate;
    [SerializeField] private Vector2 posInit;
    public bool isDoFade = true;

    private Sequence _sq;
    private void OnValidate()
    {
        rtCup = GetComponent<RectTransform>();
        imgCup = GetComponent<Image>();
        posInit = rtCup.anchoredPosition;
    }

    public void SetAnim(bool isFade)
    {
        isDoFade = isFade;
        rtCup.rotation = Quaternion.Euler(Vector3.zero);
        rtCup.anchoredPosition = posInit;
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        _sq?.Kill();
        _sq = DOTween.Sequence()
            .Append(imgCup.DOFade(1, 0.3f).From(isDoFade ? 0 : 1))
            .Join(rtCup.DOLocalRotate(rotate, 0.3f))
            .Join(rtCup.DOAnchorPos(new Vector2(posInit.x > 0 ? posInit.x - 50 : posInit.x + 50, posInit.y), 0.3f))
            .AppendInterval(0.3f)
            .Append(imgCup.DOFade(0, 0.3f).From(1))
            .AppendCallback(() =>
            {
                rtCup.anchoredPosition = posInit;
                gameObject.SetActive(false);
            });
    }
}