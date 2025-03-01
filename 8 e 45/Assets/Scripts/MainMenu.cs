using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play(){
        SceneManager.LoadScene("Xogo");
    }

    public void Quit(){
        Application.Quit();
    }
}
