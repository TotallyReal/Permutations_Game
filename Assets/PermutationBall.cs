using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PermutationBall : MonoBehaviour
{

    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private TextMeshProUGUI textNumber;
    [SerializeField] private int number;

    public void SetNumber(int n)
    {
        number = n;
        textNumber.text = "" + n;
    }

    public void SetColor(Color color)
    {
        textNumber.color = color;
    }
}
