using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PulseTube : MonoBehaviour
{

    [SerializeField] private Transform ballsContainer;
    [SerializeField] private TubeBall ballPrefab;
    [SerializeField] private float ballRadius = 0.5f;
    [SerializeField] private int maxBalls = 6;

    private Queue<TubeBall> ballsInTube;
    private Queue<TubeBall> ballsOutOfTube;
    private PlayerInput input;

    private bool touchFloor = false;

    private void Awake()
    {
        ballsInTube = new Queue<TubeBall>();
        ballsOutOfTube = new Queue<TubeBall>();
        for (int i = 0; i < maxBalls; i++)
        {
            TubeBall ball = Instantiate(ballPrefab, ballsContainer);
            ballsOutOfTube.Enqueue(ball);
            ball.gameObject.SetActive(false);
        }

        input = new PlayerInput();
        input.Player.Enable();
    }

    private void OnEnable()
    {
        input.Player.MainAction.performed += MainAction_performed;
        input.Player.SecondAction.performed += SecondAction_performed;
    }

    private void OnDisable()
    {
        input.Player.MainAction.performed -= MainAction_performed;
        input.Player.SecondAction.performed -= SecondAction_performed;
    }

    private void SecondAction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        RemoveBall();
    }

    private void MainAction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        AddBallToTube();
    }

    public void AddBallToTube()
    {
        if (ballsOutOfTube.Count == 0)
            return;

        TubeBall ball = ballsOutOfTube.Dequeue();
        ball.transform.localPosition = new Vector3(0, maxBalls - 1, 0);
        ballsInTube.Enqueue(ball);
        ball.speed = 0;
        ball.gameObject.SetActive(true);
    }

    public void RemoveBall()
    {
        if (!touchFloor)
            return;
        TubeBall ball = ballsInTube.Dequeue();
        ballsOutOfTube.Enqueue(ball);
        ball.gameObject.SetActive(false);
        touchFloor = false;
    }

    private void FixedUpdate()
    {
        UpdateBalls();
    }

    public void UpdateBalls()
    {
        float formerPosition = -1f;
        foreach (TubeBall ball in ballsInTube)
        {
            Vector3 position = ball.transform.localPosition;
            if (position.y - formerPosition > 2 * ballRadius)
            {
                ball.speed += 0.03f;
                position.y = Mathf.Max(ball.transform.localPosition.y - ball.speed * Time.deltaTime, formerPosition + 2 * ballRadius);
                ball.transform.localPosition = position;
            }
            else
            {
                ball.speed = 0;
            }
            formerPosition = position.y;
        }
        if (ballsInTube.Count > 0)
        {
            if (ballsInTube.First().transform.localPosition.y < 0.001f)
            {
                touchFloor = true;
            }
        }
    }

}
