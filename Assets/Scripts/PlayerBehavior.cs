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
    [SerializeField] private Color _colorSafe;
    [SerializeField] private Color _colorDanger;
    [SerializeField] private float _currentSafetyRange = 0f;
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

    void Start()
    {
      //get everything except player stuff
      _layerMask = ~0;

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
    }

    void Update()
    {
      _playerLight.color = Color.Lerp(_colorSafe, _colorDanger, _currentSafetyRange);

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
      if (_playerCc.enabled)
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
      if (lineDir != Vector3.zero)
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
                _playerCc.enabled = false; //disable collision
                _playerCc.transform.position = hit.transform.position;
                _shouldTryZip = false; //we zipped good, don't zip more
                _modelGo.SetActive(false);
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
                  _moveVelocity = Vector3.zero; //dont retain velocity, it's weird
                  _zippedTo = maybeZip;
                  _zippedTo.DoZip();
                  _playerCc.enabled = false; //disable collision
                  _playerCc.transform.position = maybeZip.transform.position;
                  _shouldTryZip = false; //we zipped good, don't zip more
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
            _zippedTo.UndoZip();
            _zippedTo = null;
            _playerCc.transform.position = _linePoints[1];
            _playerCc.enabled = true;
            _modelGo.SetActive(true);
          }
        }
      }


      _shouldTryZip = false;

      //camera smooth
      var from = _playerCam.transform.position;
      var to = _playerCc.transform.position + Vector3.Lerp(_playerCamOffset, _playerCamMaxOffset, PlayerManager.NormalizeF(_playerCc.velocity.magnitude, 0f, _terminalVelocity));
      _newCamPos = Vector3.SmoothDamp(from, to, ref _camVelocity, _camTime, _terminalVelocity + 1f, Time.fixedDeltaTime);

      PlayerManager.PlayerVelocity = _moveVelocity;
    }

    public void OnMove(InputAction.CallbackContext cbc)
    {
      _moveDirection = cbc.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext cbc)
    {
      if (cbc.started)
      {
        //display line
        _isLineEnabled = true;
      }

      if (cbc.canceled)
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
  }
}
