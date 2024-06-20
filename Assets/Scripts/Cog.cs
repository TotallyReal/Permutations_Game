using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Cog;

public class Cog : MonoBehaviour
{

    [SerializeField] private Transform cogVisual;
    [Header("Teeth")]
    [SerializeField] private Transform teethParent;
    [SerializeField] private Tooth toothPrefab;
    [SerializeField] private int numberOfTeeth = 3;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Rotation")]
    [SerializeField] private bool isRotating = false;
    [Tooltip("Rotations anticlockwise - ignore this for now")]
    //[SerializeField] private float rotationPerSec = 1.0f;
    [SerializeField] Fraction rotationPerSec = new Fraction(1, 1);
    [SerializeField] bool rotateAntiClockwise = true;
    private int _rotateAntiClockwise = 1;
    //[SerializeField] private int nnnn = 5;
    [Tooltip("Starting orientation")]
    [SerializeField] private Phase phase;

    public enum Phase : ushort
    {
        UP = 0,
        LEFT = 90,
        DOWN = 180,
        RIGHT = 270
    }

    private List<Tooth> teeth;

    // Start is called before the first frame update
    void Start()
    {
        SetNumberOfTeeth(numberOfTeeth);
    }

    private void OnValidate()
    {
        _rotateAntiClockwise = rotateAntiClockwise ? 1 : -1;
    }

    #region -------------- Angle --------------

    // Set Angle in degrees (in global space)
    public void SetAngle(float angle)
    {
        cogVisual.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public float GetAngle()
    {
        return cogVisual.transform.rotation.eulerAngles.z;
    }

    /// <summary>
    /// Returns a number in [0, numOfTeeth), indicating which tooth points up;
    /// </summary>
    /// <returns></returns>
    public int GetDiscreteAngle()
    {
        return Mathf.RoundToInt(GetNumerOfTeeth() * GetAngle() / 360);
    }

    #endregion


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
            tooth.transform.Translate(new Vector3(0, 0.5f*cogVisual.transform.localScale.x, 0), Space.Self);
        }
        text.SetText("" + n);

        teeth.First().SetColor(Color.yellow);
    }



    // Update is called once per frame
    // Update is called once per frame
    void Update()
    {
        if (isRotating)        
        {
            //SetAngle(WheelManager.Time() * 360 * rotationPerSec.Value() + (ushort)phase);
            SetAngle(WheelManager.Time() * 360 * _rotateAntiClockwise + (ushort)phase);
            // TODO: only works with global time. Consider adding local time
        }

    }

    internal int GetNumerOfTeeth()
    {
        return numberOfTeeth;
    }

    internal Phase GetPhase()
    {
        return phase;
    }

    internal Fraction GetRotationsPerSec()
    {
        //return rotationPerSec;
        return _rotateAntiClockwise;
    }
}

static class PhaseMethods
{
    public static Fraction GetFraction(this Phase phase)
    {
        return new Fraction((int)phase, 360);
    }

}