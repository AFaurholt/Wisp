using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
  public class KillZoneBehavior : MonoBehaviour
  {
    void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer)
      {
        PlayerManager.CurrentPlayer.Die();
      }
    }
  }
}