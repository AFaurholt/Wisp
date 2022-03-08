using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
  public class PlatformMoveBehavior : MonoBehaviour
  {
    [SerializeField] Rigidbody _rb;
    [SerializeField] MoveToPoint[] _moveToPoints;
    [SerializeField] bool _shouldLoop;
    [SerializeField] bool _shouldRotate;
    [SerializeField] int _currentIdx = 0;
    [SerializeField] float _currentTime = 0f;
    bool _isHighToLow = true;

    void FixedUpdate()
    {
      _currentTime += Time.fixedDeltaTime;

      //get the next move to
      int nextIdx = _currentIdx;
      if (_isHighToLow)
      {
        nextIdx++;
      }
      else
      {
        nextIdx--;
      }

      if (_moveToPoints.Length == nextIdx || nextIdx < 0) //we ran out
      {
        if (_shouldLoop)
        {
          nextIdx = 0; //start over
        }
        else
        {
          _isHighToLow = !_isHighToLow; //go reverse

          if (_isHighToLow)
          {
            nextIdx++;
          }
          else
          {
            nextIdx--;
          }
        }
      }

      // //update move
      var normalTime = _currentTime / _moveToPoints[nextIdx].time;
      _rb.MovePosition(Vector3.Lerp(_moveToPoints[_currentIdx].transform.position, _moveToPoints[nextIdx].transform.position, normalTime));
      if (_shouldRotate)
      {
        _rb.MoveRotation(Quaternion.Lerp(_moveToPoints[_currentIdx].transform.rotation, _moveToPoints[nextIdx].transform.rotation, normalTime));
      }

      if (_currentTime >= _moveToPoints[nextIdx].time) //we should be there
      {
        _currentIdx = nextIdx;
        _currentTime = 0;
      }
    }

    
  }

}
