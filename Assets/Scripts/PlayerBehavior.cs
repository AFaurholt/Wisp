using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
  [Header("Camera")]
  [SerializeField] private Camera _playerCam;
  [SerializeField] private Vector3 _playerCamOffset;
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
  [SerializeField] private float _dashSpeed = 50f;
  [SerializeField] private float _dashLengthS = 100f;
  [SerializeField] private float _currentDashLengthS = 0f;
  [SerializeField] private Vector2 _moveDirection = Vector2.zero;
  [SerializeField] private Vector2 _moveVelocity = Vector2.zero;
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

  void Start()
  {
    //get everything except player stuff
    _layerMask = ~0;

    _linePoints = new Vector3[2];
    _line.positionCount = 2;
    _line.enabled = false;
    _linePoints[0] = _playerCc.transform.position;

    _currentLineColor = _aimColor;

    //set offset relative to player
    _playerCam.transform.position = _playerCc.transform.position + _playerCamOffset;
  }

  void Update()
  {
    _playerLight.color = Color.Lerp(_colorSafe, _colorDanger, _currentSafetyRange);

    _line.SetPositions(_linePoints);
    _line.colorGradient = _currentLineColor;
    _line.enabled = _isLineEnabled;
  }

  void FixedUpdate()
  {
    //movement
    if (_playerCc.enabled)
    {
      Vector2 move = Vector2.Scale(_moveDirection, new Vector2(_baseSpeed, _baseSpeed));
      _moveVelocity += move;
      _moveVelocity = Vector2.ClampMagnitude(_moveVelocity, _maxSpeed);

      if (Mathf.Epsilon > _moveDirection.sqrMagnitude)
      {
        _moveVelocity = Vector2.MoveTowards(_moveVelocity, Vector2.zero, _decelSpeed);
      }

      _playerCc.Move(_moveVelocity * Time.deltaTime);
      _playerCc.transform.position = new Vector3(_playerCc.transform.position.x, _playerCc.transform.position.y, 0);
    }

    //line stuff
    _linePoints[0] = _playerCc.transform.position;

    var lineDir = _pointerWorldPos - _playerCc.transform.position;
    if(lineDir == Vector3.zero) //usually happens right after a zip
    {
      lineDir.x += 0.00001f;
    }
    var lineLen = Mathf.Min(_maxLineLength, lineDir.magnitude);
    _linePoints[1] = (lineDir.normalized * lineLen) + _playerCc.transform.position;

    Debug.DrawRay(_playerCc.transform.position, lineDir, Color.red);
    Debug.DrawRay(_playerCc.transform.position, lineDir.normalized * lineLen, Color.cyan);

    _currentLineColor = _aimColor;
    int hits = Physics.RaycastNonAlloc(_playerCc.transform.position, lineDir, _lineRaycastHits, lineLen, _layerMask);
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
      //is the hit a zip point
      var maybeZip = hit.transform.parent.GetComponent<ZipTargetBehavior>();
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
          _playerCc.transform.position = hit.transform.position;
          _shouldTryZip = false; //we zipped good, don't zip more
        }
      }
      _linePoints[1] = hit.point;
    }

    if (_zippedTo && _shouldTryZip) //we're inside a zippy thing and trying to zip
    {
      _playerCc.enabled = true;
      float offset = 0;
      if(hits > 0) // we're colliding with something
      {
        offset = _playerCc.radius * 1.4f; //give a little clearance
      }
      _playerCc.transform.position = (_linePoints[1] - _playerCc.transform.position).normalized * (lineLen - offset) + _playerCc.transform.position;
      _zippedTo.UndoZip();
      _zippedTo = null;
    }

    _shouldTryZip = false;
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
    //update line
    var vec3 = new Vector3(Pointer.current.position.ReadValue().x, Pointer.current.position.ReadValue().y, -_playerCam.transform.position.z);
    _pointerWorldPos = _playerCam.ScreenToWorldPoint(vec3);
  }

  private float NormalizeF(float val, float min, float max)
  {
    return (val - min) / (max - min);
  }
}
