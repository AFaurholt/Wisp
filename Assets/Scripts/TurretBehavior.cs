using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

  public class TurretBehavior : MonoBehaviour
  {

    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPatrolIdx;
    [SerializeField] private string _rootName = "robot_Turret/Root";
    [SerializeField] private string _bodyName = "robot_Turret/Root/Main_Body";
    [SerializeField] private string _turretName = "robot_Turret/Root/Main_Body/Turret";
    [SerializeField] private int _currentIdx = 0;
    [SerializeField] private TurretMode _turretMode = TurretMode.Patrol;
    [SerializeField] private float _patrolSpeed = 0.01f;
    [SerializeField] private float _huntSpeed = 0.2f;
    [SerializeField] private float _rotSpeed = 0.5f;
    [SerializeField] private float _searchTimer = 5f;
    private float _currentSearchTimer = 0f;
    [SerializeField] private string _playerTag = "Player";
    private Transform _playerTransform;
    private Quaternion _idleRot;
    private Rigidbody _rb;
    private int _newIdx = 0;
    private Transform _root;
    private Transform _body;
    private Transform _turret;
    private bool _isInc = true;
    private bool _playerIsZipped = false;
    private Quaternion _targetRot;

    void Start()
    {
      _playerTransform = GameObject.FindWithTag(_playerTag).transform;
      _rb = GetComponent<Rigidbody>();
      _root = transform.Find(_rootName);
      _body = transform.Find(_bodyName);
      _turret = transform.Find(_turretName);
      _root.position = _line.GetPosition(_currentIdx);
      _idleRot = _body.rotation;
      _targetRot = _idleRot;
      PlayerManager.SubscribeZip(PlayerIsZipped);
    }

    void OnTriggerStay(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer)
      {
        Debug.Log("hello");
      }
    }

    void FixedUpdate()
    {
      float speed = 0f;
      switch (_turretMode)
      {
        case TurretMode.LostTarget:
          {
            _currentSearchTimer += Time.fixedDeltaTime;
            if (_currentSearchTimer >= _searchTimer)
            {
              _turretMode = TurretMode.Patrol;
            }
            break;
          }
        case TurretMode.HasTarget:
          {
            _currentSearchTimer = 0f;
            speed = _huntSpeed;
            int closestIdx = 0;
            float lastDist = Vector3.Distance(_playerTransform.position, _line.GetPosition(closestIdx));
            for (int i = 1; i < _line.positionCount; i++)
            {
              var tmpDist = Vector3.Distance(_playerTransform.position, _line.GetPosition(i));
              if (tmpDist < lastDist)
              {
                lastDist = tmpDist;
                closestIdx = i;
              }
            }
            Debug.Log(closestIdx);
            _newIdx = closestIdx;
            break;
          }
        case TurretMode.Patrol:
          {
            _targetRot = _idleRot;
            speed = _patrolSpeed;
            if (_line.GetPosition(_newIdx) == _root.position)
            {
              if (_isInc)
              {
                _newIdx++;
                if (!(_line.positionCount > _newIdx))
                {
                  _isInc = false;
                  _newIdx -= 2;
                }
              }
              else
              {
                _newIdx--;
                if (_newIdx < 0)
                {
                  _isInc = true;
                  _newIdx += 2;
                }
              }
            }
            break;
          }
        default:
          break;
      }


      if (_line.GetPosition(_currentIdx) == _root.position)
      {
        if (_newIdx < _currentIdx)
        {
          _currentIdx--;
        }
        else if (_newIdx > _currentIdx)
        {
          _currentIdx++;
        }
      }
      _root.position = Vector3.MoveTowards(_root.position, _line.GetPosition(_currentIdx), speed);
      _body.rotation = Quaternion.RotateTowards(_body.rotation, _targetRot, _rotSpeed);
    }

    void PlayerIsZipped(bool b)
    {
      _playerIsZipped = b;
    }
  }
}