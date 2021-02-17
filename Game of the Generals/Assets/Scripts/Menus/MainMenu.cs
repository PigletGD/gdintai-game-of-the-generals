using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() => SceneManager.LoadScene("GameScene");

    public void QuitGame() => Application.Quit();

    public void ViewInstructions() => SceneManager.LoadScene("InstructionsScene");

    public void ReturnCall() => SceneManager.LoadScene("MainMenuScene");
}