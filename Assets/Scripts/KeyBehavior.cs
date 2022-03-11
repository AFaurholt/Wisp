using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{

  public class KeyBehavior : MonoBehaviour
  {
    [SerializeField] Rigidbody _rb;
    [SerializeField] Collider _triggerZone;
    [SerializeField] GameObject _keyHole;
    Transform _keyHolder;
    bool _shouldFollow = false;
    bool _shouldDie = false;

    void OnTriggerEnter(Collider other)
    {
      if (!_shouldFollow)
      {
        if (other.gameObject.layer == PlayerManager.PlayerLayer)
        {
          _rb.gameObject.layer = PlayerManager.PlayerLayer;
          _keyHolder = PlayerManager.PlayerKeyHolder;
          _shouldFollow = true;
        }
      }
      else if (other.gameObject == _keyHole)
      {
        _rb.detectCollisions = false;
        _keyHolder = other.transform;
        _shouldDie = true;
      }

    }

    void FixedUpdate()
    {
      if (_shouldFollow)
      {
        var dist = _keyHolder.transform.position - _rb.transform.position;
        var dir = dist.normalized;
        var mag = dist.magnitude;
        _rb.velocity = dir * mag * PlayerManager.KeySpeed;
      }

      if(_shouldDie && _rb.transform.position == _keyHolder.transform.position)
      {
        _keyHolder.GetComponent<KeyHoleBehavior>().StartMoving();
        Destroy(_rb.gameObject);
      }
    }
  }
}