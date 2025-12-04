using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextAsset lv;
    [SerializeField] private List<Fruit> fruitGreen, fruitOrange;
    [SerializeField] private List<Transform> points, cupGreen, cupOrange;
    [SerializeField] private List<CupPouring> cupPouringGreen, cupPouringOrange;
    [SerializeField] private Transform holeGreen, holeOrange, blenderGreen, blenderOrange, parentFruitGreen,
        parentFruitOrange, bigCupGreen, bigCupOrange;

    [SerializeField] private RectTransform fillGreen, fillOrange;
    [SerializeField] private float radius;
    [SerializeField] private Button btnBlenderGreen, btnBlenderOrange;
    [SerializeField] private List<Sprite> sprFruitGreen, sprFruitOrange;
    
    private Tween _rotateBlenderGreen, _rotateBlenderOrange, _twBigCupGreen, _twBigCupOrange;
    private int cupFillGreen, cupFillOrange;
    private List<Vector2> posInitCupGreen;
    private Vector2 posInitBigCupGreen, posInitBigCupOrange;
    private bool isRunning;

    private void OnValidate()
    {
        foreach (var f in fruitGreen)
        {
            f.animFruit = f.fruit.GetComponent<AnimRunFruit>();
        }
        foreach (var f in fruitOrange)
        {
            f.animFruit = f.fruit.GetComponent<AnimRunFruit>();
        }
    }

    private void Start()
    {
        btnBlenderGreen.onClick.AddListener(RunFruitGreen);
        btnBlenderOrange.onClick.AddListener(RunFruitOrange);
        posInitCupGreen = cupGreen.Select(c => (Vector2)c.transform.position).ToList();
        posInitBigCupGreen = bigCupGreen.transform.position;
        posInitBigCupOrange = bigCupOrange.transform.position;
        foreach (var fruit in fruitGreen)
        {
            fruit.startPos = fruit.fruit.position;
        }
        foreach (var fruit in fruitOrange)
        {
            fruit.startPos = fruit.fruit.position;
        }
        ReloadLevel();
    }
    [Button]
    private void ReloadLevel()
    {
        fillGreen.gameObject.SetActive(false);
        fillOrange.gameObject.SetActive(false);
        bigCupGreen.transform.position = posInitBigCupGreen;
        bigCupOrange.transform.position= posInitBigCupOrange;
        for (int i = 0; i < posInitCupGreen.Count; i++)
        {
            cupGreen[i].position = posInitCupGreen[i];
        }
        cupFillGreen = 16;
        cupFillOrange = 16;
        cupGreen.ForEach(c => c.gameObject.SetActive(false));
        StopAllCoroutines();
        fruitGreen.ForEach(f => f.Reset());
        fruitOrange.ForEach(f => f.Reset());
    }
    [Button]
    private void SetData()
    {
        if (cupGreen.All(c => c.gameObject.activeSelf)) return;
        var cupShow = cupGreen.First(c => !c.gameObject.activeSelf);
        cupShow.gameObject.SetActive(true);
        var posMove = cupShow.transform.position;
        cupShow.transform.position = new Vector2(posMove.x - 0.1f, posMove.y + 0.2f);
        DOTween.Sequence().Append(cupShow.DOMove(posMove,  0.2f))
            .Join(cupShow.GetComponent<Image>().DOFade(1, 0.2f).From(0));
    }
    
    private void RunFruitGreen()
    {
        if (isRunning) return;
        isRunning = true;
        FindAndMove(fruitGreen, holeGreen.position);
    }
    private void RunFruitOrange()
    {
        if (isRunning) return;
        isRunning = true;
        FindAndMove(fruitOrange, holeOrange.position);
    }
    private void FindAndMove(List<Fruit> fruit, Vector2 holePos)
    {
        if (fruit.Count < 1) return;
        float step = Vector2.Distance(fruit.First().fruit.position, fruit[1].fruit.position); // khoảng cách giữa các pointPos
        var pointPos = points.Select(p => (Vector2)p.position).ToList();
        foreach (var f in fruit)
        {
            var targetPos = RandomPointInsideCircle(holePos, radius);
            List<Vector2> path = FindPath(f.fruit.position, targetPos, step, pointPos, 0.1f);
            //Debug.Log("Path: " + string.Join(",", path));
            // thực hiện di chuyển theo path
            if (f.move != null) StopCoroutine(f.move);
            f.move = StartCoroutine(MoveAlongPath(f, path, targetPos));
        }
    }
    List<Vector2> FindPath(Vector2 start, Vector2 goal, float step, List<Vector2> pos, float tolerance)
    {
        List<Vector2> path = new List<Vector2>();
        path.Add(start);
        for (int i = 0; i < pos.Count; i++)
        {
            //Debug.Log("start: " + path.Last());
            var neighbors = GetNeighbors(path.Last(), pos, step, tolerance);
            //Debug.Log("GetNeighborsGreen: " + string.Join(",", neighbors));
            if (neighbors == null) break;
            var p = neighbors.OrderBy(n => Vector2.Distance(n, goal)).First();
            path.Add(p);
            //Debug.Log($"distance: {p}<<>>" + Vector2.Distance(path.Last(), goal));
            if (Vector2.Distance(path.Last(), goal) < radius * 2.1f) break;
        }
        
        return path;
    }

    List<Vector2> GetNeighbors(Vector2 startPos, List<Vector2> pos, float step, float tolerance)
    {
        return pos.Where(p => Vector2.Distance(p, startPos) < step + tolerance).ToList();
    }

    IEnumerator MoveAlongPath(Fruit obj, List<Vector2> path, Vector2 target)
    {
        foreach (var p in path)
        {
            while (Vector2.Distance(obj.fruit.position, p) > 0.01f)
            {
                obj.fruit.position = Vector2.MoveTowards(obj.fruit.position, p, Time.deltaTime * 3f);
                obj.animFruit.SetSpr(obj.typeFruit == TypeFruit.Green ? sprFruitGreen : sprFruitOrange);
                obj.animFruit.isRunning = true;
                if (obj.animFruit.animCo == null)
                {
                    obj.animFruit.animCo = StartCoroutine(PlayFruitAnim(obj));
                }
                yield return null;
            }
        }
        obj.animFruit.isRunning = false;
        obj.animFruit.StopAllCoroutines();
        obj.animFruit.animCo = null;
        obj.jump?.Kill();
        obj.jump = DOTween.Sequence().Append(obj.fruit.DOJump(target, 1f, 1, 0.3f))
            .AppendCallback(() =>
            {
                //Debug.Log($"cupFillOrange: {obj.typeFruit}<>{cupFillOrange}");
                Poring(obj.typeFruit);
                Blender(obj.typeFruit);
            })
            .Append(obj.image.DOFade(0, 0.2f)).AppendInterval(1f).AppendCallback(() =>
            {
                switch (obj.typeFruit)
                {
                    case TypeFruit.Green when cupFillGreen <= 0:
                        _twBigCupGreen?.Kill();
                        _twBigCupGreen = bigCupGreen.DOMove(new Vector2(10, 0), 2f);
                        break;
                    case TypeFruit.Orange when cupFillOrange <= 0:
                        _twBigCupOrange?.Kill();
                        _twBigCupOrange = bigCupOrange.DOMove(new Vector2(10, 0), 2f);
                        break;
                }
                isRunning = false;
            })
            .AppendInterval(1f).AppendCallback(() =>
            {
                if (cupFillGreen <= 0 && cupFillOrange <= 0)
                {
                    _twBigCupGreen?.Kill();
                    _twBigCupOrange?.Kill();
                    ReloadLevel();
                }
            });
    }
    private IEnumerator PlayFruitAnim(Fruit obj)
    {
        while (obj.animFruit.isRunning)
        {
            obj.animFruit.SetFruit(obj.animFruit.fruit[obj.animFruit.index]);
            obj.animFruit.index = (obj.animFruit.index + 1) % obj.animFruit.fruit.Count;
            yield return new WaitForSeconds(0.025f);   // speed
        }
    }
    private void Poring(TypeFruit typeFruit)
    {
        if (typeFruit != TypeFruit.Green)
        {
            cupFillOrange--;
            if (cupFillOrange == 10)
            {
                fillOrange.gameObject.SetActive(true);
                fillOrange.DOAnchorPosY(0, 1f).From(new Vector2(0, -40f));
            }
            if (cupPouringOrange.Count(c => !c.gameObject.activeSelf) > 0)
                cupPouringOrange.First(c => !c.gameObject.activeSelf).SetAnim(true);
            
            if (cupFillOrange <= 0)
            {
                var cupGreenActivate = cupGreen.Where(c => c.gameObject.activeSelf).ToList();
                
                if (cupGreenActivate.Count > 0)
                {
                    cupFillGreen -= cupGreenActivate.Count;
                    for (int i = 0; i < cupGreenActivate.Count; i++)
                    {
                        var i1 = i;
                        var iTarget = i >= cupPouringGreen.Count ? Random.Range(0, cupPouringGreen.Count) : i;
                        var target = cupPouringGreen[iTarget];
                        cupGreenActivate[i1].DOMove(target.transform.position, 0.5f)
                            .OnComplete(() =>
                            {
                                cupPouringGreen[iTarget].SetAnim(false);
                                cupGreenActivate[i1].gameObject.SetActive(false);
                                if (i1 == 6)
                                {
                                    fillGreen.gameObject.SetActive(true);
                                    fillGreen.DOAnchorPosY(0, 1f).From(new Vector2(0, -40f));
                                }
                            });
                    }
                }
            }
            
        }
        else
        {
            if (cupFillOrange > 0)
            {
                CupWait();
            }
            else
            {
                cupFillGreen--;
                if (cupFillGreen == 10)
                {
                    fillGreen.gameObject.SetActive(true);
                    fillGreen.DOAnchorPosY(0, 1f).From(new Vector2(0, -40f));
                }
                cupPouringGreen.First(c => !c.gameObject.activeSelf).SetAnim(true);
            }
            
        }
    }

    private void Blender(TypeFruit typeFruit)
    {
        if (typeFruit != TypeFruit.Green)
        {
            blenderOrange.rotation = Quaternion.Euler(0f, 0f, 0f);
            _rotateBlenderOrange?.Kill();
            _rotateBlenderOrange = blenderOrange
                .DOLocalRotate(new Vector3(0, 0, 360), 0.4f, RotateMode.LocalAxisAdd)
                .SetLoops(2)
                .SetEase(Ease.Linear);
        }
        else
        {
            blenderGreen.rotation = Quaternion.Euler(0f, 0f, 0f);
            _rotateBlenderGreen?.Kill();
            _rotateBlenderGreen = blenderGreen
                .DOLocalRotate(new Vector3(0, 0, 360), 0.4f, RotateMode.LocalAxisAdd)
                .SetLoops(2)
                .SetEase(Ease.Linear);
        }
    }
    private void CupWait()
    {
        if (cupGreen.All(c => c.gameObject.activeSelf)) return;
        var cupShow = cupGreen.First(c => !c.gameObject.activeSelf);
        cupShow.gameObject.SetActive(true);
        var posMove = cupShow.transform.position;
        cupShow.transform.position = new Vector2(posMove.x - 0.1f, posMove.y + 0.2f);
        DOTween.Sequence().Append(cupShow.DOMove(posMove,  0.2f))
            .Join(cupShow.GetComponent<Image>().DOFade(1, 0.2f).From(0));
    }
    public Vector2 RandomPointInsideCircle(Vector2 center, float r)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Mathf.Sqrt(Random.Range(0f, 1f)) * r;
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }
}