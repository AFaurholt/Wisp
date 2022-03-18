using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{

  public class Collectible : MonoBehaviour
  {
    public int pointValue;
    [SerializeField]
    private ParticleSystem flash;
    private AudioSource pointAudio;
    public AudioClip pointSound;
    public GameObject pointSFX;
    void Start()
    {
      pointAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer)
      {
        Instantiate(pointSFX, transform.position, pointSFX.transform.rotation);
        pointAudio.PlayOneShot(pointSound);
        Instantiate(flash, transform.position, flash.transform.rotation);
        PlayerManager.CollectibleHandler?.Invoke(pointValue);
        Destroy(gameObject);
      }
    }
  }
}