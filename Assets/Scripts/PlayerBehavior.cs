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
  [SerializeField] private Gradient _aimColor;
  [SerializeField] private Gradient _legalTargetColor;
  [SerializeField] private float _maxLineLength = 20f;
  [SerializeField] private Vector3[] _linePoints;
  bool _isLineEnabled = false;
  private RaycastHit[] _lineRaycastHits = new RaycastHit[10];
  private Vector3 _pointerWorldPos = Vector3.zero;
  private int _layerMask;

  void Start()
  {
    //get everything except player stuff
    _layerMask = ~0;

    _linePoints = new Vector3[2];
    _line.positionCount = 2;
    _line.enabled = false;
    _linePoints[0] = transform.position;

    //set offset relative to player
    _playerCam.transform.position = transform.position + _playerCamOffset;
  }

  void Update()
  {
    _playerLight.color = Color.Lerp(_colorSafe, _colorDanger, _currentSafetyRange);

    _line.SetPositions(_linePoints);

    _line.enabled = _isLineEnabled;
  }

  void FixedUpdate()
  {
    Vector2 move = Vector2.Scale(_moveDirection, new Vector2(_baseSpeed, _baseSpeed));
    _moveVelocity += move;
    _moveVelocity = Vector2.ClampMagnitude(_moveVelocity, _maxSpeed);

    if(Mathf.Epsilon > _moveDirection.sqrMagnitude)
    {
      _moveVelocity = Vector2.MoveTowards(_moveVelocity, Vector2.zero, _decelSpeed);
    }

    _playerCc.Move(_moveVelocity * Time.deltaTime);
    transform.position = new Vector3(transform.position.x, transform.position.y, 0);

    _linePoints[0] = transform.position;
    
    var lineDir = _pointerWorldPos - transform.position;
    var lineLen = Mathf.Min(_maxLineLength, lineDir.magnitude);
    _linePoints[1] = (lineDir.normalized * lineLen) + transform.position;

    Debug.DrawRay(transform.position, lineDir, Color.red);
    Debug.DrawRay(transform.position, lineDir.normalized * lineLen, Color.cyan);
    if(Physics.RaycastNonAlloc(transform.position, lineDir, _lineRaycastHits, lineLen, _layerMask) != 0)
    {
      _linePoints[1] = _lineRaycastHits[0].point;
    }
  }

  public void OnMove(InputAction.CallbackContext cbc)
  {
    _moveDirection = cbc.ReadValue<Vector2>();
  }

  public void OnFire(InputAction.CallbackContext cbc)
  {
    if(cbc.started)
    {
      //display line
      _isLineEnabled = true;
    }

    if (cbc.canceled)
    {
      _isLineEnabled = false;
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
