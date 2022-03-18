using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Game
{
  public class LevelEndBehavior : MonoBehaviour
  {
    [SerializeField] private string _text = "foobar";
    [SerializeField] private string _nextLevel = "level name";
    [SerializeField] private GameObject _canvas;
    private bool _isDisplayed = false;


    void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.layer == PlayerManager.PlayerLayer && !_isDisplayed)
      {
        DisplayCanvas();
        _isDisplayed = true;
      }
    }
    void DisplayCanvas()
    {
      _canvas.SetActive(true);
      _canvas.transform.Find("VictoryText").GetComponent<TextMeshProUGUI>().text = _text;
    }

    public void OnNextLevel()
    {
      SceneManager.LoadScene(_nextLevel);
    }
  }
}