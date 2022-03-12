using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UiTextGarbage : MonoBehaviour
{
    public TextMeshProUGUI numberTest;
    private float characterMagnitude;
    private CharacterController playerController;
    void Start()
    {
        playerController = GameObject.Find("GameManager").GetComponent<CharacterController>();
        characterMagnitude = playerController.velocity.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        numberTest.text = "Magnitude " + characterMagnitude;

    }
}
