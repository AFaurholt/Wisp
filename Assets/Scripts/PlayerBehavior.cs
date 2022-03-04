using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    //cam
    [SerializeField] private Camera playerCam;
    [SerializeField] private Vector3 playerCamOffset;
    //light color
    [SerializeField] private Light playerLight;
    [SerializeField] private Color colorSafe;
    [SerializeField] private Color colorDanger;
    [SerializeField] private float currentSafetyRange = 0f;

    void Start()
    {
        //set offset relative to player
        playerCam.transform.position = transform.position + playerCamOffset;
    }

    void Update()
    {
        playerLight.color = Color.Lerp(colorSafe, colorDanger, currentSafetyRange);
    }

    void FixedUpdate()
    {
        
    }
}
