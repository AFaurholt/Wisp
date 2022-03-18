using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
  public class CursorBehavior : MonoBehaviour
  {
    RectTransform _rect;
    void Start()
    {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.None;
      _rect = GetComponent<RectTransform>();
    }

    void Update()
    {
      _rect.transform.position = new Vector3(Pointer.current.position.ReadValue().x, Pointer.current.position.ReadValue().y, 0);
    }
  }
}
