using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CollectibleTracker : MonoBehaviour
{
    public int pointScore;
    public TextMeshProUGUI currentScore;
    
    
    void Start()
    {
        pointScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        currentScore.text = "Score: " + pointScore;
    }

    
}
