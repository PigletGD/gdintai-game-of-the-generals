using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private GameManager GM = null;

    [SerializeField] private Text stateText = null;
    [SerializeField] private GameObject setupButton = null;
    [SerializeField] private Text buttonText = null;

    public void ChangeButtonText() => buttonText.text = "Finish Setup";

    public void SetupGameUI()
    {
        setupButton.SetActive(false);

        stateText.text = "HUMAN'S TURN";
    }
}