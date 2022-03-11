using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
  public class KeyHoleBehavior : MonoBehaviour
  {
    [SerializeField] Transform _moveTo;
    [SerializeField] float _time;
    [SerializeField] MovePair[] _movePairs;
    Vector3[] _originalFrom;
    bool _shouldMove = false;
    float _currentTime = 0f;
    Vector3 _from;

    void Start()
    {
      _from = transform.position;
      _originalFrom = new Vector3[_movePairs.Length];
      for (int i = 0; i < _movePairs.Length; i++)
      {
        _originalFrom[i] = _movePairs[i].from.position;
      }
    }

    void FixedUpdate()
    {
      if (_shouldMove)
      {
        _currentTime += Time.fixedDeltaTime;
        int timeDone = 0; //add one if not done, should be zero at the end
        transform.position = Vector3.Lerp(_from, _moveTo.position, PlayerManager.NormalizeF(_currentTime, 0f, _time));
        if (_currentTime < _time)
        {
          timeDone++;
        }
        for (int i = 0; i < _movePairs.Length; i++)
        {
          _movePairs[i].from.position = Vector3.Lerp(_originalFrom[i], _movePairs[i].to.position, PlayerManager.NormalizeF(_currentTime, 0f, _movePairs[i].time));
          if (_currentTime < _movePairs[i].time)
          {
            timeDone++;
          }
        }

        if(timeDone == 0) //all done
        {
          enabled = false; //go sloop z_z
        }
      }
    }

    public void StartMoving()
    {
      _shouldMove = true;
    }
  }
}