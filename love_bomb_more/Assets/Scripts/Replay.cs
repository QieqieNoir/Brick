using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class Replay : MonoBehaviour {

    [SerializeField]
    GameObject playAgain;

    AudioSource audioSource;

    [SerializeField]
    AudioClip buttonClick;

    [SerializeField]
    TextMeshProUGUI score;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (GameSingleton.instance != null)
        {
            score.SetText("YOUR SCORE: " + GameSingleton.instance.PrintScore());
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("Lobby");
        //playAgain.SetActive(false);
    }

    public void ButtonSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(buttonClick);
        }
    }
}
