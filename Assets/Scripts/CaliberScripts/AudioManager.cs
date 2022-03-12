using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private CharacterController playerController;
    private float characterVeloctiy;
    public AudioSource wispLoop;
    void Start()
    {
        playerController = GameObject.Find("GameManager").GetComponent<CharacterController>();
        characterVeloctiy = playerController.velocity.magnitude;
        wispLoop.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
