using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
  public class RespawnZoneBehavior : MonoBehaviour
  {
    [SerializeField] Transform _respawnPoint;

    void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer)
      {
        PlayerManager.CurrentRespawn = _respawnPoint;
      }
    }
  }
}