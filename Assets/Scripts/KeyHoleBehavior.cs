using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{

  public class KeyHoleBehavior : MonoBehaviour
  {
    [SerializeField] Transform _moveTo;
    [SerializeField] float _time;
    bool _shouldMove = false;
    float _currentTime = 0f;
    Vector3 _from;

    void Start()
    {
      _from = transform.position;
    }

    void FixedUpdate()
    {
      if (_shouldMove)
      {
        _currentTime += Time.fixedDeltaTime;
        transform.position = Vector3.Lerp(_from, _moveTo.position, PlayerManager.NormalizeF(_currentTime, 0f, _time));
      }
    }

    public void StartMoving()
    {
      _shouldMove = true;
    }
  }
}