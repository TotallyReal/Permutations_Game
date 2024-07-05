using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private int number = 0;

    private void Awake()
    {
        SetNumber(number);
    }

    private void OnValidate()
    {
        SetNumber(number);
    }

    public int GetNumber()
    {
        return number;
    }

    public void SetNumber(int number)
    {
        this.number = number;
        numberText.text = "" + number;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


}
