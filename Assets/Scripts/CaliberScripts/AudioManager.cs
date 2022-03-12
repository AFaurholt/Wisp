using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private CharacterController playerController;
    private float characterMagnitude;
    public AudioSource wispLoop;
    void Start()
    {
        playerController = GameObject.Find("GameManager").GetComponent<CharacterController>();
        characterMagnitude = playerController.velocity.magnitude;
        
    }

    // Update is called once per frame
    void Update()
    {
        wispLoop.volume = 1;
    }
}
