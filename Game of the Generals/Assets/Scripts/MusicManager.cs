using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;

    [SerializeField] private AudioSource Move;
    [SerializeField] private AudioSource Take;
    [SerializeField] private AudioSource BGM;
    public static MusicManager GetInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }
        else
        {
            return null;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        playBGM();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMove()
    {
        Move.Play();
    }

    public void PlayTake()
    {
        Take.Play();
    }

    private void playBGM()
    {

        BGM.Play();
    }
}
