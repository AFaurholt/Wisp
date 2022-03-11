using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
 
public class ZipTargetBehavior : MonoBehaviour
{
  [SerializeField] private Color _activeColor;
  [SerializeField] private Color _inactiveColor;
  [SerializeField] private Color _zippedColor;
  [SerializeField] MeshRenderer _meshRend;
  Color _currentColor;


    // Start is called before the first frame update
    void Start()
    {
        _currentColor = _inactiveColor;
    }

    // Update is called once per frame
    void Update()
    {
        _meshRend.material.color = _currentColor;
    }

    public void DoZip()
    {
      _currentColor = _zippedColor;
    }

    public void UndoZip()
    {
      _currentColor = _inactiveColor;
    }
}
 
}