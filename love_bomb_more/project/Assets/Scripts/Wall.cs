using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    [SerializeField]
    AudioClip finishSound;

    [SerializeField]
    AudioSource audioSource;

    bool isFinal;

	void Start ()
    {
		
	}
	
	void Update ()
    {
        if (GameSingleton.instance.score == 10 && !isFinal)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(finishSound);
            }
            isFinal = true;
        }
    }
}
