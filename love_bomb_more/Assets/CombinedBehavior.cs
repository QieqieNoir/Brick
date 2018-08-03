using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CombinedBehavior : NetworkBehaviour
{
    public GameObject H;
    public GameObject C;

    public bool isTappedH;
    public bool isTappedC;

    public Vector3 offsetH;
    public Vector3 offsetC;

    public Vector3 cameraLast;
    public Vector3 cameraNow;

    private bool isMoving;

    /************************************
        
    ************************************/
    [SerializeField]
    float speed = 2f;

    Vector3 anchor;
    public GameObject matchedGrid;
    public Collider col;
    GameObject player;

    Vector3 stopPos;

    public bool isInteracting;

    bool isAbsorbed;
    public void SetIsAbsorbed(bool _isAbsorbed)
    {
        isAbsorbed = _isAbsorbed;
    }

    public bool GetIsAbsorbed()
    {
        return isAbsorbed;
    }

    bool enableMatch;
    public bool GetEnableMatch()
    {
        return enableMatch;
    }

    AudioSource audioSource;
    [SerializeField]
    AudioClip matchSound;

    bool avoidCol;
    Collider wallCollider;
    /***************************************

     ***************************************/


    // Use this for initialization
    void Start () {
        H = this.transform.Find("H").gameObject;
        C = this.transform.Find("C").gameObject;
		if (isServer)
        {
            Destroy(C);
            C = null;
        }
        else
        {
            Destroy(H);
            H = null;
        }
        offsetC = Vector3.zero;
        offsetH = Vector3.zero;

        /***************************************/
        anchor = GameSingleton.instance.anchor;
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        wallCollider = GameObject.FindGameObjectWithTag("wall").GetComponent<Collider>();

        if (isServer)
        {
            player = GameObject.FindGameObjectWithTag("player1");
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("player2");
        }

        avoidCol = true;
        StartCoroutine(DetectColInBeginning());
        /***************************************/
    }

    void FixedUpdate()
    {
        if (!player)
        {
            if (isServer)
            {
                player = GameObject.FindGameObjectWithTag("player1");
            }
            else
            {
                player = GameObject.FindGameObjectWithTag("player2");
            }
        }
/*
        if (isTappedC && isTappedH)
        {
            transform.position = transform.position + (offsetH + offsetC) / 2;
            Debug.Log("MMMMMMMMMMMMOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOVing!!!!!!!!!!!!!!!!!!!!!!!!!!11");
        }
*/
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "wall" && avoidCol)
        {
            float ranVal = Random.Range(0.3f, 0.4f);
            transform.position += new Vector3(ranVal, 0f, ranVal);
            Debug.Log("collide with the wall and move position");
        }

        if (player.GetComponent<PlayerBehaviorNetworking>().GetIsSnapped())
        {
            if ((this.gameObject.tag == "piece1" && other.gameObject.tag == "grid1")
            || (this.gameObject.tag == "piece2" && other.gameObject.tag == "grid2")
            || (this.gameObject.tag == "piece3" && other.gameObject.tag == "grid3")
            || (this.gameObject.tag == "piece4" && other.gameObject.tag == "grid4"))
            {
                // disable the previous grid
                if (matchedGrid && matchedGrid != other.gameObject)
                {
                    matchedGrid.GetComponent<WallGrid>().isHovering = false;
                    matchedGrid.GetComponent<WallGrid>().triggerHover = false;
                    matchedGrid = null;
                }

                // get new closest grid and make it glow
                matchedGrid = other.gameObject;
                enableMatch = true;
                matchedGrid.GetComponent<WallGrid>().isHovering = false;
                matchedGrid.GetComponent<WallGrid>().triggerHover = true;
                Debug.Log("can be matched with " + matchedGrid);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "wall" && avoidCol)
        {
            float ranVal = Random.Range(0.3f, 0.4f);
            transform.position += new Vector3(ranVal, 0f, ranVal);
            Debug.Log("collide with the wall and move position");
        }

        if (matchedGrid && !enableMatch)
        {
            enableMatch = true;
        }
        if (matchedGrid && !matchedGrid.GetComponent<WallGrid>().triggerHover)
        {
            matchedGrid.GetComponent<WallGrid>().isHovering = false;
            matchedGrid.GetComponent<WallGrid>().triggerHover = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (matchedGrid && matchedGrid.GetComponent<WallGrid>().triggerHover)
        {
            enableMatch = false;
            matchedGrid.GetComponent<WallGrid>().isHovering = false;
            matchedGrid.GetComponent<WallGrid>().triggerHover = false;
        }
    }

    public void Match()
    {
        StartCoroutine(Absorb());
        matchedGrid.GetComponent<WallGrid>().triggerHover = false;
        //this.GetComponent<PieceHover>().Final();
    }

    IEnumerator Absorb()
    {
        transform.parent = null;
        isAbsorbed = true;
        matchedGrid.GetComponent<WallGrid>().hasPiece = true;

        speed = 0f;
        float lerpTime = 0f;
        float absorbSpeed = 0.5f;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(matchSound);
        }

        while (true)
        {
            lerpTime += Time.deltaTime * absorbSpeed;

            if (lerpTime >= 1f)
            {
                yield break;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, matchedGrid.transform.position, lerpTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, lerpTime);
            }
            yield return null;
        }
    }

    IEnumerator DetectColInBeginning()
    {
        float detectTimer = 3f;
        while (detectTimer >= 0)
        {
            detectTimer -= Time.deltaTime;
            yield return null;

            if (detectTimer < 0)
            {
                avoidCol = false;
                wallCollider.isTrigger = false;
                wallCollider.enabled = false;
                Debug.Log("no more move");
                yield break;
            }
        }
    }
}
