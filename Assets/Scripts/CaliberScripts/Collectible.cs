using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
    
{
    private CollectibleTracker collectibleTracker;
    public int pointValue;
    [SerializeField]
    private ParticleSystem flash;
    void Start()
    {
        collectibleTracker = GameObject.Find("GameManager").GetComponent<CollectibleTracker>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            Instantiate(flash, transform.position, flash.transform.rotation);
            collectibleTracker.pointScore += pointValue;
            Destroy(gameObject);
        }
    }
}
