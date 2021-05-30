using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public BoardManager BM = null;

    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private Text debugButtonText = null;

    [SerializeField] private GameObject resultsScreen = null;
    [SerializeField] private Text resultsText = null;

    private Piece selectedPiece = null;

    private Camera mainCam = null;

    public bool isSettingUp = false;
    private bool onDebugMode = false;

    [SerializeField] private GameEventsSO onFinishSetup = null;
    [SerializeField] private GameEventsSO onDebugModeChanged = null;

    public bool playerTurn = true;
    public bool gameOver = false;

    private void Awake()
    {
        mainCam = Camera.main;

        isSettingUp = true;
    }

    private void Update()
    {
        if (playerTurn || isSettingUp)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Sends raycast towards mouse position to see if piece is under cursor
                RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100.0f, LayerMask.GetMask("Piece"));

                if (hit.collider != null)
                {
                    Piece pieceHit = hit.collider.GetComponent<Piece>();

                    if (pieceHit.playerPiece && !pieceHit.isDead)
                        selectedPiece = pieceHit;
                }
            }

            if (selectedPiece != null)
            {
                Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                selectedPiece.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);

                if (Input.GetMouseButtonUp(0))
                {
                    RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100.0f, LayerMask.GetMask("Board"));

                    if (hit.collider != null) BM.PlacePiece(mousePos, selectedPiece); //board.PlacePiece(mousePos, selectedPiece);

                    if (!selectedPiece.isDead) selectedPiece.transform.position = selectedPiece.lastPosition;

                    selectedPiece = null;
                }
            }
        }
    }

    public void StartGame()
    {
        isSettingUp = false;

        onFinishSetup.Raise();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;

        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;

        pauseMenu.SetActive(false);
    }

    public void ReplayGame()
    {
        Time.timeScale = 1;

        gameOver = true;

        SceneManager.LoadScene("GameScene");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;

        gameOver = true;

        SceneManager.LoadScene("MainMenuScene");
    }

    public void ClickedDebug()
    {
        if (!isSettingUp)
        {
            onDebugMode = !onDebugMode;

            if (onDebugMode) debugButtonText.text = "DISABLE DEBUG";
            else debugButtonText.text = "ENABLE DEBUG";

            onDebugModeChanged.Raise();
        }
    }

    public void ShowResults()
    {
        gameOver = true;

        resultsScreen.SetActive(true);

        if (playerTurn) resultsText.text = "HUMAN WINS";
        else resultsText.text = "COMPUTER WINS";
    }

    public void SwitchTurns()
    {
        playerTurn = !playerTurn;
    }
}