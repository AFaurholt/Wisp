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
    Vector3 _startPos;

    void Start()
    {
      _startPos = _rb.transform.position;
    }
    void OnTriggerEnter(Collider other)
    {
      if (!_shouldFollow)
      {
        if (other.gameObject.layer == PlayerManager.PlayerLayer)
        {
          _rb.gameObject.layer = PlayerManager.PlayerPickupLayer;
          _keyHolder = PlayerManager.PlayerKeyHolder;
          _shouldFollow = true;
          PlayerManager.CurrentKey = this;
        }
      }
      else if (other.gameObject == _keyHole && !_shouldDie)
      {
        _rb.detectCollisions = false;
        _keyHolder = other.transform;
        _shouldDie = true;
        PlayerManager.CurrentKey = null;
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

      if (_shouldDie && Vector3.Distance(_rb.transform.position, _keyHolder.transform.position) <= 1f)
      {
        _keyHolder.GetComponent<KeyHoleBehavior>().StartMoving();
        Destroy(_rb.gameObject);
      }
    }

    public void ResetKey()
    {
      _shouldFollow = false;
      _rb.transform.position = _startPos;
      _rb.gameObject.layer = PlayerManager.DefaultLayer;
      PlayerManager.CurrentKey = null;
      _rb.velocity = Vector3.zero;
    }
  }
}