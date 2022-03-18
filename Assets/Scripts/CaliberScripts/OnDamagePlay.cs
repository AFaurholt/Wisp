using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDamagePlay : MonoBehaviour
{
    public GameObject soundEffect;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Damage")) 
        {
            Instantiate(soundEffect,transform.position,soundEffect.transform.rotation);
        }
    }


}
