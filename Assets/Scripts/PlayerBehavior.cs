using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
  //cam
  [SerializeField] private Camera _playerCam;
  [SerializeField] private Vector3 _playerCamOffset;
  //light color
  [SerializeField] private Light _playerLight;
  [SerializeField] private Color _colorSafe;
  [SerializeField] private Color _colorDanger;
  [SerializeField] private float _currentSafetyRange = 0f;
  //movement
  [SerializeField] private CharacterController _playerCc;
  [SerializeField] private float _terminalVelocity = 20f;
  [SerializeField] private float _maxSpeed = 10f;
  [SerializeField] private float _baseSpeed = 1f;
  [SerializeField] private float _decelSpeed = 0.5f;
  [SerializeField] private float _dashSpeed = 50f;
  [SerializeField] private float _dashLengthS = 100f;
  [SerializeField] private float _currentDashLengthS = 0f;
  [SerializeField] private Vector2 _moveDirection = Vector2.zero;
  [SerializeField] private Vector2 _moveDirectionLast = Vector2.zero;
  [SerializeField] private Vector2 _moveVelocity = Vector2.zero;

  void Start()
  {
    //set offset relative to player
    _playerCam.transform.position = transform.position + _playerCamOffset;
  }

  void Update()
  {
    _playerLight.color = Color.Lerp(_colorSafe, _colorDanger, _currentSafetyRange);
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


    //update delta move
    _moveDirectionLast = _moveDirection;
  }

  public void OnMove(InputAction.CallbackContext cbc)
  {
    _moveDirection = cbc.ReadValue<Vector2>();
  }

  private float NormalizeF(float val, float min, float max)
  {
    return (val - min) / (max - min);
  }
}
