using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DcClass;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    public delegate void AllLevelsComplete();
    public event AllLevelsComplete OnAllLevelsComplete;

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
    public void OpenScene(int scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void NextLevel()
    {
        if (SceneManager.GetSceneAt(SceneManager.GetActiveScene().buildIndex + 1).IsValid())
            OpenScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
        {
            if(OnAllLevelsComplete != null)
                OnAllLevelsComplete();
        }
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
