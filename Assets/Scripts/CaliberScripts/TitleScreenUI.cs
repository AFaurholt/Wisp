using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    public Button start;
    public TextMeshProUGUI Wisp;
    public string NextLevel = "";


    public void StartGame()
    {
       SceneManager.LoadScene(NextLevel);
    }
}
