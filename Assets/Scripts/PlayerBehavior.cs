using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Game
{
  public class PlayerBehavior : MonoBehaviour
  {
    [SerializeField] private GameObject _modelGo;
    [Header("Camera")]
    [SerializeField] private Camera _playerCam;
    [SerializeField] private Vector3 _playerCamOffset;
    [SerializeField] private Vector3 _playerCamMaxOffset;
    [SerializeField] private float _camTime = 1f;
    [SerializeField] private Vector3 _camVelocity = Vector3.zero;
    private Vector3 _camToPoint;
    [SerializeField] private Vector3 _newCamPos;
    [Header("Player color")]
    [SerializeField] private Light _playerLight;
    [SerializeField] private MeshRenderer _meshRend;
    [SerializeField] private Color _colorSafe;
    [SerializeField] private Color _colorDanger;
    [SerializeField] private Color _colorHopeful;
    [SerializeField] private Material _matSafe;
    [SerializeField] private Material _matDanger;
    [SerializeField] private Material _matHopeful;
    [SerializeField] private float _currentSafetyRange = 0f;
    [SerializeField] private int _pulses = 3;
    [SerializeField] private float _pulseInterval = 0.5f;
    int _currentPulse = 0;
    float _currentInterval = 0f;
    float _currentMood = 0f;
    [Header("Player movement")]
    [SerializeField] private CharacterController _playerCc;
    [SerializeField] private float _terminalVelocity = 20f;
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _baseSpeed = 1f;
    [SerializeField] private float _decelSpeed = 0.5f;
    [SerializeField] private float _sprintExtraSpeed = 10f;
    [SerializeField] private Vector2 _moveDirection = Vector2.zero;
    [SerializeField] private Vector2 _moveVelocity = Vector2.zero;
    private bool _shouldSprint = false;
    [Header("Zip stuff")]
    [SerializeField] private LineRenderer _line;
    private Gradient _currentLineColor;
    [SerializeField] private Gradient _aimColor;
    [SerializeField] private Gradient _legalTargetColor;
    [SerializeField] private float _maxLineLength = 20f;
    [SerializeField] private Vector3[] _linePoints;
    bool _isLineEnabled = false;
    private RaycastHit[] _lineRaycastHits = new RaycastHit[10];
    private Vector3 _pointerWorldPos = Vector3.zero;
    private int _layerMask;
    private bool _shouldTryZip = false;
    private ZipTargetBehavior _zippedTo = null;
    [Header("Key stuff")]
    [SerializeField] private Transform _keyHolder;
    [SerializeField] private float _keySpeed = 10f;
    [Header("Death stuff")]
    [SerializeField] private float _deathWaitTime = 3f;
    float _currentDeathWaitTime = 0f;
    bool _isDead = false;
    bool _isDeathCamMove = false;
    [Header("Turret stuff")]
    [SerializeField] float _visibleRadius = 0.3f;

        //Calibers changes start
        [SerializeField] private GameObject deathSFX;
        [SerializeField] private GameObject zipSFX;
        //Calibers changes end

    void Start()
    {
      //get everything except player stuff
      _layerMask = PlayerManager.PlayerLayer;

      _linePoints = new Vector3[2];
      _line.positionCount = 2;
      _line.enabled = false;
      _linePoints[0] = _playerCc.transform.position;
      _linePoints[1] = _playerCc.transform.position;

      _currentLineColor = _aimColor;

      //set offset relative to player
      _playerCam.transform.position = _playerCc.transform.position + _playerCamOffset;
      _camToPoint = _playerCam.transform.position;
      _newCamPos = _camToPoint;

      PlayerManager.PlayerKeyHolder = _keyHolder;
      PlayerManager.KeySpeed = _keySpeed;
      PlayerManager.CurrentPlayer = this;
      PlayerManager.VisibleRadius = _visibleRadius;
    }

    void Update()
    {
      if (_currentSafetyRange < 0)
      {
        float safety = _currentSafetyRange * -1;
        _playerLight.color = Color.Lerp(_colorSafe, _colorHopeful, safety);
        _meshRend.material.Lerp(_matSafe, _matHopeful, safety);
      }
      else
      {
        _playerLight.color = Color.Lerp(_colorSafe, _colorDanger, _currentSafetyRange);
        _meshRend.material.Lerp(_matSafe, _matDanger, _currentSafetyRange);
      }

      _line.SetPositions(_linePoints);
      _line.colorGradient = _currentLineColor;
      _line.enabled = _isLineEnabled;
    }

    void LateUpdate()
    {
      _playerCam.transform.position = _newCamPos;
    }

    void FixedUpdate()
    {
      var vec3 = new Vector3(Pointer.current.position.ReadValue().x, Pointer.current.position.ReadValue().y, -_playerCam.transform.position.z);
      _pointerWorldPos = _playerCam.ScreenToWorldPoint(vec3);

      //movement
      if (_playerCc.enabled && !_isDead)
      {
        var speed = _baseSpeed;
        if (_shouldSprint)
        {
          speed += _sprintExtraSpeed;
        }
        Vector2 move = Vector2.Scale(_moveDirection, new Vector2(speed, speed));
        _moveVelocity += move;
        _moveVelocity = Vector2.ClampMagnitude(_moveVelocity, _maxSpeed);

        if (Mathf.Epsilon > _moveDirection.sqrMagnitude)
        {
          _moveVelocity = Vector2.MoveTowards(_moveVelocity, Vector2.zero, _decelSpeed);
        }

        _playerCc.Move(_moveVelocity * 0.01f); //I'm assuming one unit is 1 meter, so we wanna make that smaller
        _playerCc.transform.position = new Vector3(_playerCc.transform.position.x, _playerCc.transform.position.y, 0);
      }

      //line stuff
      _linePoints[0] = _playerCc.transform.position;

      var lineDir = _pointerWorldPos - _playerCc.transform.position;
      var lineLen = Mathf.Min(_maxLineLength, lineDir.magnitude);
      _linePoints[1] = (lineDir.normalized * lineLen) + _playerCc.transform.position;

      _currentLineColor = _aimColor;
      if (lineDir != Vector3.zero && !_isDead)
      {
        int hits = Physics.RaycastNonAlloc(_playerCc.transform.position, lineDir, _lineRaycastHits, lineLen, _layerMask, QueryTriggerInteraction.Ignore);
        if (hits > 0)
        {
          //find the hit with shortest distance to origin
          RaycastHit hit = _lineRaycastHits[0];
          for (int i = 1; i < hits; i++)
          {
            if (_lineRaycastHits[i].distance < hit.distance)
            {
              hit = _lineRaycastHits[i];
            }
          }
          lineLen = hit.distance; //how long the line actually is

          if (hit.transform.parent)
          {
            var maybeZip = hit.transform.parent.GetComponent<ZipTargetBehavior>(); //is the hit a zip point
            if (maybeZip)
            {
              _currentLineColor = _legalTargetColor;
              if (_shouldTryZip) // try to zip
              {
                if (_zippedTo)
                {
                  _zippedTo.UndoZip();
                }
                _moveVelocity = Vector3.zero; //dont retain velocity, it's weird
                _zippedTo = maybeZip;
                _zippedTo.DoZip();
                                //Calibers Changes START
                                Instantiate(zipSFX, transform.position, zipSFX.transform.rotation);
                                //Calibers Changes END
                _playerCc.enabled = false; //disable collision
                _playerCc.transform.position = hit.transform.position;
                _shouldTryZip = false; //we zipped good, don't zip more
                _modelGo.SetActive(false);
                PlayerManager.ZipHandler?.Invoke(true);
              }
            }
          }

          _linePoints[1] = hit.point;
        }

        //set color?
        lineDir = _linePoints[1].normalized;
        hits = Physics.SphereCastNonAlloc(_linePoints[1], _playerCc.radius, lineDir, _lineRaycastHits, 0.1f, _layerMask, QueryTriggerInteraction.Ignore);
        if (hits > 0)
        {
          for (int i = 0; i < hits; i++)
          {
            if (_lineRaycastHits[i].transform.parent)
            {
              var maybeZip = _lineRaycastHits[i].transform.parent.GetComponent<ZipTargetBehavior>(); //is the hit a zip point
              if (maybeZip)
              {
                _currentLineColor = _legalTargetColor;
                if (_shouldTryZip) // try to zip
                {
                  if (_zippedTo)
                  {
                    _zippedTo.UndoZip();
                  }
                  _zippedTo = maybeZip;
                  _zippedTo.DoZip();
                  _playerCc.enabled = false; //disable collision
                  _playerCc.transform.position = maybeZip.transform.position;
                  _shouldTryZip = false; //we zipped good, don't zip more
                  PlayerManager.ZipHandler?.Invoke(true);

                }
              }
            }
          }
        }
        else if (_zippedTo)
        {
          _currentLineColor = _legalTargetColor;
          if (_shouldTryZip)
          {
                        //Calibers Changes START
                        Instantiate(zipSFX, transform.position, zipSFX.transform.rotation);
                        //Calibers Changes END
            _zippedTo.UndoZip();
            _zippedTo = null;
            _playerCc.transform.position = _linePoints[1];
            _playerCc.enabled = true;
            _modelGo.SetActive(true);
            PlayerManager.ZipHandler?.Invoke(false);

          }
        }
      }

      _shouldTryZip = false;

      //camera smooth
      var from = _playerCam.transform.position;
      var to = _playerCc.transform.position + Vector3.Lerp(_playerCamOffset, _playerCamMaxOffset, PlayerManager.NormalizeF(_playerCc.velocity.magnitude, 0f, _terminalVelocity));
      if (_zippedTo)
      {
        to = _playerCc.transform.position + _playerCamMaxOffset;
      }

      _newCamPos = Vector3.SmoothDamp(from, to, ref _camVelocity, _camTime, _terminalVelocity + 1f, Time.fixedDeltaTime);

      //death stuff
      if (_isDead)
      {
                //Calibers Changes START
                Instantiate(zipSFX, transform.position, deathSFX.transform.rotation);
                //Calibers Changes END
        if (!_isDeathCamMove)
        {
          _currentDeathWaitTime += Time.fixedDeltaTime;
        }
        if (_currentDeathWaitTime >= _deathWaitTime || _isDeathCamMove)
        {
          _isDeathCamMove = true;
          var newFrom = _playerCc.transform.position + _playerCamMaxOffset;
          var newTo = PlayerManager.CurrentRespawn.position + _playerCamMaxOffset;
          _newCamPos = Vector3.Lerp(newTo, newFrom, PlayerManager.NormalizeF(_currentDeathWaitTime, 0f, _deathWaitTime));
          _currentDeathWaitTime -= Time.fixedDeltaTime;

          if (_currentDeathWaitTime <= 0f)
          {
            _camVelocity = Vector3.zero;
            _modelGo.SetActive(true);
            _playerCc.transform.position = PlayerManager.CurrentRespawn.position;
            _isDead = false;
            _playerCc.gameObject.layer = PlayerManager.PlayerLayer;
            _isDeathCamMove = false;
            _newCamPos = Vector3.SmoothDamp(from, _playerCc.transform.position + _playerCamOffset, ref _camVelocity, _camTime, _terminalVelocity + 1f, Time.fixedDeltaTime);
            PlayerManager.RespawnHandler?.Invoke();
          }
        }
      }

      PlayerManager.PlayerVelocity = _moveVelocity;
    }

    public void OnMove(InputAction.CallbackContext cbc)
    {
      _moveDirection = cbc.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext cbc)
    {
      if (cbc.started && !_isDead)
      {
        //display line
        _isLineEnabled = true;
      }

      if (cbc.canceled && !_isDead)
      {
        _isLineEnabled = false;
        _shouldTryZip = true;
      }
    }

    public void OnLook(InputAction.CallbackContext cbc)
    {
      //sad empty method :c
    }

    public void OnSprint(InputAction.CallbackContext cbc)
    {
      if (cbc.started)
      {
        _shouldSprint = true;
      }

      if (cbc.canceled)
      {
        _shouldSprint = false;
      }
    }

    public void Die()
    {
      _currentDeathWaitTime = 0f;
      _isDead = true;
      _modelGo.SetActive(false);
      _playerCc.gameObject.layer = PlayerManager.DeadLayer;
      _isLineEnabled = false;
      _moveVelocity = Vector3.zero;
      if (PlayerManager.CurrentKey)
      {
        PlayerManager.CurrentKey.ResetKey();
      }
    }

    public void OnExit(InputAction.CallbackContext cbt)
    {
      Application.Quit(0);
    }
  }
}
