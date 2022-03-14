using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UiTextGarbage : MonoBehaviour
{
  public TextMeshProUGUI numberTest;
  private float characterMagnitude;
  [SerializeField] private CharacterController playerController;
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    numberTest.text = "Magnitude " + characterMagnitude;

  }

  void FixedUpdate()
  {
    characterMagnitude = playerController.velocity.magnitude / 5;
  }
}
