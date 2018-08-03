using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PieceHover : NetworkBehaviour {

    // blink
    [SerializeField]
    Shader glow;

    Renderer rend;
    [SerializeField]
    float lerpSpeed;
    [HideInInspector]
    public bool isBlinking;

    // shiver
    [SerializeField]
    float shiverSpeed;
    [HideInInspector]
    public bool isShivering;

    AudioSource audioSource;
    [SerializeField]
    AudioClip hoverSound;

    bool isFinal;

    void Start()
    {
        rend = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        rend.material.SetFloat("_MKGlowPower", 0f);
        rend.material.SetFloat("_MKGlowTexStrength", 0f);
    }

    public void Hover()
    {
        isBlinking = true;
        isShivering = true;
        StartCoroutine(Glow());
        StartCoroutine(Shiver());
    }

    public void NotHover()
    {
        isBlinking = false;
        isShivering = false;
        Debug.Log("not hover");
    }

    public void Final()
    {
        StartCoroutine(Glow());
        isFinal = true;
    }

    IEnumerator Glow()
    {
        float lerpTime = 0f;
        float minGlowPower = 0f;
        float maxGlowPower = 1f;
        float curGlowPower = 0f;
        float minGlowStrength = 0f;
        float maxGlowStrength = 1f;
        float curGlowStrength = 0f;

        while (true)
        {
            lerpTime += Time.deltaTime * lerpSpeed;
            curGlowPower = Mathf.Lerp(minGlowPower, maxGlowPower, lerpTime);
            curGlowStrength = Mathf.Lerp(minGlowStrength, maxGlowStrength, lerpTime);
            rend.material.SetFloat("_MKGlowPower", curGlowPower);
            rend.material.SetFloat("_MKGlowTexStrength", curGlowStrength);
            Debug.Log("glow");

            if (lerpTime >= 1f)
            {
                if (isFinal)
                {
                    Debug.Log("final blink ends");
                    yield break;
                }

                float temp = maxGlowPower;
                maxGlowPower = minGlowPower;
                minGlowPower = temp;

                float temp2 = maxGlowStrength;
                maxGlowStrength = minGlowStrength;
                minGlowStrength = temp2;

                lerpTime = 0f;
            }

            if (!isBlinking)
            {
                Debug.Log("glow break");
                rend.material.SetFloat("_MKGlowPower", 0f);
                rend.material.SetFloat("_MKGlowTexStrength", 0f);
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator Shiver()
    {
        float lerpTime = 0f;
        float randomRange = Random.Range(-0.01f, 0.01f);
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
                randomRange = Random.Range(-0.01f, 0.01f);

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
