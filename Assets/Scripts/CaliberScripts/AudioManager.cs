using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
  [SerializeField] private CharacterController playerController;
  private float characterMagnitude;
  public AudioSource wispLoop;
    public AudioSource wispIdle;

  // Update is called once per frame
  void Update()
  {
        characterMagnitude = playerController.velocity.magnitude;
    wispLoop.volume = characterMagnitude / 5;
        if (characterMagnitude == 0)
        {
            wispIdle.volume = 0.5f;
        }
        else 
        {
            wispIdle.volume = 0;
        }
  }
}
