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
    [SerializeField] private float _aimTimer = 5f;
    private float _currentAimTimer = 0f;
    private float _currentSearchTimer = 0f;
    [SerializeField] private string _playerTag = "Player";
    private Transform _playerTransform;
    private Vector3 _lastSeen;
    private Quaternion _idleRotBody;
    private Quaternion _idleRotTurret;
    private Rigidbody _rb;
    private int _newIdx = 0;
    private Transform _root;
    private Transform _body;
    private Transform _turret;
    private bool _isInc = true;
    private bool _playerIsZipped = false;
    private Quaternion _targetRot;
    private RaycastHit[] scanHits;
    private int scanMask;
    private ParticleSystem _ps;
    private bool _shouldFire = false;

    void Start()
    {
      _playerTransform = GameObject.FindWithTag(_playerTag).transform;
      _rb = GetComponent<Rigidbody>();
      _root = transform.Find(_rootName);
      _body = transform.Find(_bodyName);
      _turret = transform.Find(_turretName);
      _root.position = _line.GetPosition(_currentIdx);
      _idleRotBody = _body.rotation;
      _idleRotTurret = _turret.rotation;
      _targetRot = _idleRotBody;
      _ps = transform.Find($"{_turretName}/VFX_LaserBeam").GetComponent<ParticleSystem>();
      PlayerManager.SubscribeZip(PlayerIsZipped);
      scanHits = new RaycastHit[10];
      scanMask = ~(0 << PlayerManager.PlayerLayer << PlayerManager.PlayerPickupLayer);
      PlayerManager.SubscribeRespawn(SetPatrolIfNotDead);
    }

    void OnTriggerStay(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer)
      {
        if (CanSeePlayer())
        {
          _turretMode = TurretMode.HasTarget;
          _lastSeen = _playerTransform.position;
        }
        else if (_turretMode == TurretMode.HasTarget)
        {
          _turretMode = TurretMode.LostTarget;
        }
      }
    }

    void OnTriggerExit(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer)
      {
        if (_turretMode == TurretMode.HasTarget)
        {
          _turretMode = TurretMode.LostTarget;
        }
      }
    }

    void FixedUpdate()
    {

      float speed = 0f;
      switch (_turretMode)
      {
        case TurretMode.LostTarget:
          {
            _currentAimTimer = 0f;
            _currentSearchTimer += Time.fixedDeltaTime;
            if (_currentSearchTimer >= _searchTimer)
            {
              _turretMode = TurretMode.Patrol;
            }
            else
            {
              speed = _huntSpeed;
              int closestIdx = 0;
              float lastDist = Vector3.Distance(_lastSeen, _line.GetPosition(closestIdx));
              for (int i = 1; i < _line.positionCount; i++)
              {
                var tmpDist = Vector3.Distance(_lastSeen, _line.GetPosition(i));
                if (tmpDist < lastDist)
                {
                  lastDist = tmpDist;
                  closestIdx = i;
                }
              }
              _newIdx = closestIdx;
            }
            break;
          }
        case TurretMode.HasTarget:
          {
            _currentSearchTimer = 0f;
            AimAtPlayer();
            _currentAimTimer += Time.fixedDeltaTime;
            if (_currentAimTimer >= _aimTimer)
            {
              _ps.Play();
              PlayerManager.CurrentPlayer.Die();
              _turretMode = TurretMode.Offline;
            }
            break;
          }
        case TurretMode.Patrol:
          {
            _turret.rotation = _idleRotTurret;
            _targetRot = _idleRotBody;
            speed = _patrolSpeed;
            if (_line.GetPosition(_newIdx) == _root.position)
            {
              if (_isInc)
              {
                _newIdx++;
                if (!(_maxPatrolIdx > _newIdx))
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
    }

    void PlayerIsZipped(bool b)
    {
      if (b && _turretMode == TurretMode.HasTarget)
      {
        _turretMode = TurretMode.LostTarget;
      }
    }

    bool CanSeePlayer()
    {
      if (!(_turretMode == TurretMode.Offline || _turretMode == TurretMode.Dying))
      {
        //center
        int numHits = Physics.RaycastNonAlloc(
          _playerTransform.position
          , _turret.position - _playerTransform.position
          , scanHits
          , Mathf.Infinity
          , scanMask
          , QueryTriggerInteraction.Ignore);

        Debug.DrawRay(_playerTransform.position, _turret.position - _playerTransform.position, Color.magenta);

        if (numHits > 0)
        {
          int closestIdx = 0;
          for (int i = 1; i < numHits; i++)
          {
            if (scanHits[closestIdx].distance > scanHits[i].distance)
            {
              closestIdx = i;
            }
          }
          if (scanHits[closestIdx].transform == transform)
          {
            return true;
          }
        }

        //right
        var pos = _playerTransform.position + new Vector3(PlayerManager.VisibleRadius, 0, 0);
        numHits = Physics.RaycastNonAlloc(
          pos
          , _turret.position - pos
          , scanHits
          , Mathf.Infinity
          , scanMask
          , QueryTriggerInteraction.Ignore);

        Debug.DrawRay(pos, _turret.position - pos, Color.red);

        if (numHits > 0)
        {
          int closestIdx = 0;
          for (int i = 1; i < numHits; i++)
          {
            if (scanHits[closestIdx].distance > scanHits[i].distance)
            {
              closestIdx = i;
            }
          }
          if (scanHits[closestIdx].transform == transform)
          {
            return true;
          }
        }

        //left
        pos = _playerTransform.position + new Vector3(-PlayerManager.VisibleRadius, 0, 0);
        numHits = Physics.RaycastNonAlloc(
          pos
          , _turret.position - pos
          , scanHits
          , Mathf.Infinity
          , scanMask
          , QueryTriggerInteraction.Ignore);

        Debug.DrawRay(pos, _turret.position - pos, Color.red);

        if (numHits > 0)
        {
          int closestIdx = 0;
          for (int i = 1; i < numHits; i++)
          {
            if (scanHits[closestIdx].distance > scanHits[i].distance)
            {
              closestIdx = i;
            }
          }
          if (scanHits[closestIdx].transform == transform)
          {
            return true;
          }
        }

        //up
        pos = _playerTransform.position + new Vector3(0, PlayerManager.VisibleRadius, 0);
        numHits = Physics.RaycastNonAlloc(
          pos
          , _turret.position - pos
          , scanHits
          , Mathf.Infinity
          , scanMask
          , QueryTriggerInteraction.Ignore);

        Debug.DrawRay(pos, _turret.position - pos, Color.blue);

        if (numHits > 0)
        {
          int closestIdx = 0;
          for (int i = 1; i < numHits; i++)
          {
            if (scanHits[closestIdx].distance > scanHits[i].distance)
            {
              closestIdx = i;
            }
          }
          if (scanHits[closestIdx].transform == transform)
          {
            return true;
          }
        }

        //down
        pos = _playerTransform.position + new Vector3(0, -PlayerManager.VisibleRadius, 0);
        numHits = Physics.RaycastNonAlloc(
          pos
          , _turret.position - pos
          , scanHits
          , Mathf.Infinity
          , scanMask
          , QueryTriggerInteraction.Ignore);

        Debug.DrawRay(pos, _turret.position - pos, Color.blue);

        if (numHits > 0)
        {
          int closestIdx = 0;
          for (int i = 1; i < numHits; i++)
          {
            if (scanHits[closestIdx].distance > scanHits[i].distance)
            {
              closestIdx = i;
            }
          }
          if (scanHits[closestIdx].transform == transform)
          {
            return true;
          }
        }
      }

      return false;
    }

    void AimAtPlayer()
    {
      //TODO make pretty
      _turret.LookAt(_lastSeen, Vector3.left);
      var q = Quaternion.AngleAxis(90, Vector3.down);
      _turret.rotation *= q;

    }

    void LookAround()
    { }

    void SetPatrolIfNotDead()
    {
      if (_turretMode != TurretMode.Dying)
      {
        _turretMode = TurretMode.Patrol;
        _turret.rotation = _idleRotTurret;
        _currentAimTimer = 0f;
        _currentSearchTimer = 0f;
      }
    }
  }
}