using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class PieceBehavior : MonoBehaviour {

    float range = 5f;
    float speed = 2f;

    Vector3 startPos;
    Vector3 targetPos;
    Vector3 fixPos;
    Vector3 anchor;

    bool isPosFixed;
    bool isMatch;
    bool isSnapped;
    bool startFollowing;

    float lerpSpeed = 0.2f;
    GameObject net;

    Coroutine SnapCoroutine;

    public void SetIsMatch(bool _isMatch)
    {
        isMatch = _isMatch;
    }

    public bool GetIsMatch()
    {
        return isMatch;
    }

    void Start()
    {
        anchor = GameSingleton.instance.anchor;
        startPos = transform.position;
        targetPos = Random.insideUnitSphere * range;
        net = GameObject.Find("Net");
    }

	void Update ()
    {
        if (transform.parent != null)
        {
            Snap();
        }
        else if (isMatch)
        {
            Stop();
        }
        else
        {
            Float();
        }
	}

    void Float()
    {
        transform.RotateAround(anchor,Vector3.up,Time.deltaTime * speed);
    }

    void Snap()
    {
        speed = 0f;
        if (!isSnapped)
        {
            SnapCoroutine = StartCoroutine(SnapToPhone());
            isSnapped = true;
        }
        if (startFollowing)
        {
            //transform.LookAt(net.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, net.transform.position, Time.deltaTime);
        }
    }

    void Stop()
    {
        speed = 0f;
    }

    IEnumerator SnapToPhone()
    {
        float lerpTime = 0f;

        while (true)
        {
            if (lerpTime >= 0.7f)
            {
                //StopCoroutine(SnapCoroutine);
                startFollowing = true;
                yield break;
            }
            else
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                transform.position = Vector3.Lerp(transform.position, net.transform.position, lerpTime);
                Debug.Log("to the net!");
            }
            yield return null;
        }
    }
}
