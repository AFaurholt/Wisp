using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
  [SerializeField] private CharacterController playerController;
  private float characterMagnitude;
  public AudioSource wispLoop;

  // Update is called once per frame
  void Update()
  {
    wispLoop.volume = playerController.velocity.magnitude;
  }
}
