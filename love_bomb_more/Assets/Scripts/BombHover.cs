using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombHover : NetworkBehaviour {

    [SerializeField]
    float lerpSpeed;

    // shiver
    [SerializeField]
    float shiverSpeed;
    [HideInInspector]
    public bool isShivering;

    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip hoverSound;
    [SerializeField]
    AudioClip countdownSound;
    [SerializeField]
    AudioClip explodeSound;


    // Use this for initialization
    void Start () {
		
	}
	
    public void Active()
    {
        isShivering = true;

        StartCoroutine(Shiver());

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(countdownSound);
        }
    }

    public void NotShiver()
    {
        isShivering = false;
        this.transform.localPosition = Vector3.zero;
    }
    IEnumerator Shiver()
    {
        float lerpTime = 0f;
        float randomRange = Random.Range(-0.04f, 0.04f);
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + new Vector3(randomRange, randomRange, randomRange);

        while (true)
        {
            lerpTime += Time.deltaTime * shiverSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, lerpTime);
            Debug.Log("shiver");

            if (lerpTime >= 1f)
            {
                Vector3 temp = startPos;
                startPos = targetPos;
                targetPos = temp;
                randomRange = Random.Range(-0.04f, 0.04f);

                lerpTime = 0f;
            }

            if (!isShivering)
            {
                Debug.Log("shivering break");
                yield break;
            }

            yield return null;
        }
    }

}
