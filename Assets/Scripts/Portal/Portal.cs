using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Portal : MonoBehaviour
{

    public bool isActive = true;
    private bool isWaiting = false;
    [SerializeField] private Vector3 jump;
    [SerializeField]

    public event EventHandler<Vector3> Portaled;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"Trigger : {collision}");
        if (collision.tag == "Player" && isActive && !isWaiting)
        {
            if (transform.InverseTransformPoint(collision.transform.position).x>0)
            {
                // only portal when entering from the left.
                return;
            }
            isWaiting = true;
            //Debug.Log("Has Player tag");

            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            CinemachineVirtualCamera vCam = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            vCam.ForceCameraPosition(Camera.main.transform.position + jump, vCam.transform.rotation);


            collision.transform.position += jump;
            Portaled?.Invoke(this, jump);

            StartCoroutine(WaitAndReactivate());
        }
    }

    private IEnumerator WaitAndReactivate()
    {
        yield return new WaitForSeconds(0.1f);
        isWaiting = false;
    }

}
