using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{

  public class CollectibleTracker : MonoBehaviour
  {
    public int pointScore = 0;
    public TextMeshProUGUI currentScore;

    void Start()
    {
      PlayerManager.SubscribeCollectible(UpdateScore);
      currentScore.text = $"Score: {pointScore}";
    }

    void UpdateScore(int point)
    {
      pointScore += point;
      currentScore.text = $"Score: {pointScore}";
    }
  }
}