using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TMPro;
using UnityEngine;

public class PuzzlePermutation : MonoBehaviour
{

    private Color[] colors = new Color[] { 
        Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.black, Color.white
    };

    [SerializeField] private int size;
    [SerializeField] private int order;
    [SerializeField] private PermutationLink linkPrefab;
    [SerializeField] private PermutationBall ballPrefab;
    [SerializeField] private SpriteRenderer stepPrefab;
    [SerializeField] private Transform gate;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private TextMeshProUGUI puzzleName;

    private PermutationLink[] links;
    private DragAndDrop[] targets;
    private int[] inverse;
    private int[] ballPosition;
    private PermutationBall[] balls;
    private SpriteRenderer[] steps;

    PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
        input.Player.Enable();

        links = new PermutationLink[size];
        targets = new DragAndDrop[size];
        balls = new PermutationBall[size];
        inverse = new int[size];
        ballPosition = new int[size];
        for (int i = 0; i < size; i++)
        {
            inverse[i] = i;
            links[i] = Instantiate(linkPrefab, transform);
            links[i].name = $"Link {i}";
            links[i].SetIDs(i, i);
            links[i].SetColor(colors[i]);
            targets[i] = links[i].GetTarget();
            targets[i].OnEndDragging += OnEndDragging;

            balls[i] = Instantiate(ballPrefab, transform);
            balls[i].transform.localPosition = new Vector3(0, i, -1);
            balls[i].SetNumber(i);
            balls[i].SetColor(colors[i]);
            balls[i].name = $"ball {i}";
            ballPosition[i] = i;
        }

        steps = new SpriteRenderer[order];
        for (int i = 0; i < order; i++)
        {
            steps[i] = Instantiate(stepPrefab, transform);
            steps[i].name = $"step {i}";
            steps[i].transform.localPosition = new Vector3(i, -1, -1);
        }
    }

    private void OnEnable()
    {
        input.Player.SecondAction.performed += MainAction_performed;
    }

    private void OnDisable()
    {
        input.Player.SecondAction.performed -= MainAction_performed;
    }

    private void MainAction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ballMoveCounter = 0;
        stepIndex = 0;
        MoveBalls();
    }

    private int ballMoveCounter, stepIndex;

    private void FailPuzzle()
    {
        for (int i = 0; i < order; i++)
        {
            steps[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < balls.Length; i++)
        {
            balls[i].transform.localPosition = new Vector3(0, i, -1);
        }
    }

    private void MoveBalls()
    {
        if (stepIndex >= steps.Length)
        {
            FailPuzzle();
            return;
        }
        steps[stepIndex].gameObject.SetActive(false);
        stepIndex++;

        Sequence tweenSequence = DOTween.Sequence();
        for (int i = 0; i < size; i++)
        {
            tweenSequence.Join(balls[i].transform.DOLocalMove(new Vector3(2, links[ballPosition[i]].targetId, -1), 1));
        }
        tweenSequence.AppendInterval(0.5f);
        tweenSequence.AppendInterval(0.5f);
        for (int i = 0; i < size; i++)
        {
            tweenSequence.Join(balls[i].transform.DOLocalMove(new Vector3(0, links[ballPosition[i]].targetId, -1), 1));
        }
        tweenSequence.OnComplete(() => MoveBallsAgain());
    }

    private void MoveBallsAgain()
    {
        ballMoveCounter += 1;
        bool inPlace = true;
        for (int i = 0; i < size; i++)
        {
            ballPosition[i] = links[ballPosition[i]].targetId;
            if (i != ballPosition[i])
                inPlace = false;
        }
        if (!inPlace)
        {
            MoveBalls();
            return;
        }
        if (stepIndex != steps.Length)
        {
            FailPuzzle();
            return;
        }
        gate.transform.DOLocalMoveY(7.5f, 1.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        puzzleName.gameObject.SetActive(true);
        particles.Play();
    }

    private void OnEndDragging(object sender, Vector2 position)
    {
        for (int i = 0; i < size; i++)
        {
            if (targets[i] == sender)
            {
                int targetId = links[i].targetId;
                Vector2 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(position);
                Debug.Log($"End dragging {sender} {i} at position {position}, local position {localPosition}");
                int yInt = Mathf.RoundToInt(localPosition.y);
                if (0 <= yInt && yInt < links.Length && (localPosition - new Vector2(2, yInt)).magnitude < 0.5)
                {
                    int sigma = inverse[yInt];
                    // i     => targetId
                    // sigma => yInt
                    links[i].SetIDs(i, yInt);
                    links[sigma].SetIDs(sigma, targetId);
                    inverse[yInt] = i;
                    inverse[targetId] = sigma;
                } else
                {
                    links[i].SetIDs(i, targetId);
                }
                return;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
