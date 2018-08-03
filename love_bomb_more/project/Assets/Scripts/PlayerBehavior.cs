using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    GameObject[] pieces;

    [SerializeField]
    float pieceHoverThreshold;

    [SerializeField]
    float wallDisOffset;
    float wallDis;

    int snappedPiece;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip snapSound;

    [SerializeField]
    AudioClip nonInteractableSound;

    [SerializeField]
    AudioClip releaseSound;

    [SerializeField]
    LineRenderer lineRenderer;

    Vector3[] points;
    [SerializeField]
    int pointsNum;

    [SerializeField]
    float shakeDetectionThreshold;
    float filter;
    Vector3 lowPassValue;

    [SerializeField]
    Net net;

    [SerializeField]
    GameObject netCol;

    [SerializeField]
    GameObject container;

    [SerializeField]
    float lerpSpeed = 0.5f;

    bool insideNet;
    // release pieces by tabbing
    bool isTabbed;

    Vector3 releasePos;
    [SerializeField]
    float bounceRange = 2f;

    Coroutine releasePiece;
    Coroutine bounceBack;

    [SerializeField]
    bool _debug;

    void OnEnable()
    {
        Debug.Log(GameSingleton.instance.spawnedPieces.Count);
        if (points == null)
        {
            points = new Vector3[pointsNum];
        }
    }

    void Start()
    {
        lowPassValue = Input.acceleration;
        isTabbed = false;
        insideNet = net.GetInsideNet();
    }

    void Update()
    {
        Vector3 acceleration = Input.acceleration;
        filter = Time.deltaTime * shakeDetectionThreshold;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, filter);
        Vector3 deltaAcceleration = acceleration - lowPassValue;
        //Debug.Log("Input acceleration: " + Input.acceleration);
        //Debug.Log("delta acceleration: " + deltaAcceleration.sqrMagnitude);
        //Debug.Log("shake detection threshold: " + shakeDetectionThreshold);

        insideNet = net.GetInsideNet();
        Debug.Log("is snapped: " + GameSingleton.instance.isSnapped);
        Debug.Log("net.GetInsideNet: " + net.GetInsideNet());
        for (int i = 0; i < GameSingleton.instance.spawnedPieces.Count; i++)
        {
            if (insideNet)
            {
                if (isTabbed)
                {
                    isTabbed = false;
                }
                // check the tag
                if (!GameSingleton.instance.isSnapped)
                {
                    if (net.pieceInNet.tag == "player1")
                    {
                        Snap();
                        GameSingleton.instance.SnappedPiece(i);

                    }
                    else if (net.pieceInNet.tag == "player2")
                    {
                        NonInteractable();
                    }
                }
            }
            else if (net.pieceInNet != null && net.pieceInNet.transform.parent != null)
            {
                GameSingleton.instance.IsSnapped(true);
            }
            else
            {
                GameSingleton.instance.IsSnapped(false);
            }

            // tab to release
            if (Input.touchCount >= 1 && !isTabbed)
            {
                Debug.Log("tab");
                Release();
                isTabbed = true;
                Debug.Log("touch and release");
            }

            if (_debug)
            {
                Release();
                _debug = false;
            }

            //// shake to release
            //if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
            //{
            //    Debug.Log("shake?");
            //    Release();
            //}

            if (GameSingleton.instance.isSnapped)
            {
               // DrawLine();
            }
        }
    }

    void Snap()
    {
        // parenting
        snappedPiece = GameSingleton.instance.snappedPiece;
        // net.pieceInNet.transform.rotation = Quaternion.identity;
        net.pieceInNet.transform.parent = container.transform;
        GameSingleton.instance.IsSnapped(true);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(snapSound);
        }
        Debug.Log("snaped piece: " + snappedPiece);
    }

    void NonInteractable()
    {
        //add behavior here
        bounceBack = StartCoroutine(BounceBack());
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(nonInteractableSound);
            Debug.Log("nope");
        }
    }

    void Release()
    {
        net.pieceInNet.transform.parent = null;
        net.SetInsideNet(false);
        lineRenderer.enabled = false;
        releasePiece = StartCoroutine(ReleasePiece());
        GameSingleton.instance.IsSnapped(false);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(releaseSound);
            Debug.Log("release sound");
        }
    }

    void DrawLine()
    {
        //float distance = Vector3.Distance(transform.position, GameSingleton.instance.spawnedPieces[snappedPiece].transform.position);
        //float addDis = distance / pointsNum;
        //for (int i = 0; i < pointsNum; i++)
        //{
        //    points[i] = new Vector3(transform.position.x + (addDis * i), transform.position.y + (addDis * i), transform.position.z + (addDis * i));
        //    Debug.Log(points[i]);
        //}
        lineRenderer.enabled = true;
        points[0] = transform.position;
        points[1] = GameSingleton.instance.spawnedPieces[snappedPiece].transform.position;
        lineRenderer.SetPositions(points);
        Debug.Log("draw line");
    }

    IEnumerator ReleasePiece()
    {
        // disable the net
        float lerpTime = 0f;
        releasePos = new Vector3
            (net.pieceInNet.transform.position.x + bounceRange,
            net.pieceInNet.transform.position.y, net.pieceInNet.transform.position.z + bounceRange);
        netCol.SetActive(false);

        while (true)
        {
            if (lerpTime >= 1f)
            {
                netCol.SetActive(true);
                net.pieceInNet.transform.parent = null;
                //StopCoroutine(releasePiece);
                yield break;
            }
            else if (net.pieceInNet != null)
            {
                // bounce back
                lerpTime += Time.deltaTime * lerpSpeed;
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position,
                    transform.InverseTransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }

    IEnumerator BounceBack()
    {
        float lerpTime = 0f;
        releasePos = new Vector3
    (net.pieceInNet.transform.position.x + bounceRange,
    net.pieceInNet.transform.position.y, net.pieceInNet.transform.position.z + bounceRange);

        while (true)
        {
            if (lerpTime >= 1f)
            {
                yield break;
            }
            else
            {
                // bounce back
                lerpTime += Time.deltaTime * lerpSpeed;
                net.pieceInNet.transform.position = Vector3.Lerp(net.pieceInNet.transform.position,
                    transform.TransformDirection(releasePos), lerpTime);
            }
            yield return null;
        }
    }
}
