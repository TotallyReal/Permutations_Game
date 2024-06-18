using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Cog : MonoBehaviour
{

    [SerializeField] private Transform cogVisual;
    [SerializeField] private Transform teethParent;
    [SerializeField] private Tooth toothPrefab;
    [SerializeField] private int numberOfTeeth = 3;
    [SerializeField] private TextMeshProUGUI text;

    private List<Tooth> teeth;

    // Start is called before the first frame update
    void Start()
    {
        text.SetText("" + numberOfTeeth);
    }


    // Set Angle in degrees (in global space)
    public void SetAngle(float angle)
    {
        cogVisual.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public float GetAngle()
    {
        return cogVisual.transform.rotation.eulerAngles.z;
    }


    public void SetCogRadius(float radius)
    {
        if (radius < 0)
            return;
        transform.localScale = new Vector3(2 * radius, 2 * radius, 1);
    }

    [ContextMenu("SetNumberOfTeeth")]
    public void SetNumberOfTeeth()
    {
        SetNumberOfTeeth(numberOfTeeth);
    }

    public void SetNumberOfTeeth(int n)
    {
        if (n < 1)
            return;
        numberOfTeeth = n;

        if (Application.isPlaying)
        {
            foreach (Transform tooth in teethParent.transform)
            {
                Destroy(tooth.gameObject);                
            }
        } else
        {
            List<Transform> teethToDel = new List<Transform>();
            foreach (Transform tooth in teethParent.transform)
            {
                teethToDel.Add(tooth);
            }
            foreach (Transform tooth in teethToDel)
            {
                DestroyImmediate(tooth.gameObject);
            }
        }

        teeth = new List<Tooth>();
        for (int i = 0; i < n; i++)
        {
            Tooth tooth = Instantiate(toothPrefab, teethParent);
            teeth.Add(tooth);
            tooth.gameObject.name = $"Tooth {i}";
            tooth.transform.Rotate(0, 0, 360 * i / n);
            tooth.transform.Translate(new Vector3(0, 0.5f, 0), Space.Self);
        }
        text.SetText("" + n);

        teeth.First().SetColor(Color.yellow);
    }
    


    // Update is called once per frame
    void Update()
    {
        
    }

    internal int GetNumerOfTeeth()
    {
        return numberOfTeeth;
    }
}
