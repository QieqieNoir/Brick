using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombBehavior : NetworkBehaviour
{
    GameObject player;

    public enum state {ACTIVE, EXPLODING, USED, INTERACTING, REMOVED, NONE };

    public state bombState = state.NONE;

    public bool isExploding;
    public bool isUsed;

    bool startCount;

    [SerializeField]
    public float totalTime;
    public float leaveTime;

    [SerializeField]
    public GameObject[] cols;





    // Use this for initialization
    void Start () {

        if(isServer)
        {
            player = GameObject.FindGameObjectWithTag("player2");

        }
        else
        {
            player = GameObject.FindGameObjectWithTag("player2");
        }

        bombState = state.ACTIVE;
        leaveTime = totalTime;
        StartCoroutine(BombCountDown());
        this.GetComponent<BombHover>().Active();
    }
	
	// Update is called once per frame
	void Update () {
		switch(bombState)
        {
            case state.ACTIVE:
                SetActive();
                break;
            case state.EXPLODING:
                break;
            case state.USED:
                break;
            case state.INTERACTING:
                break;
            case state.REMOVED:
                break;
            case state.NONE:
                break;
            default:
                break;
        }

	}

    public void SetActive()
    {
  
    }

    IEnumerator BombCountDown()
    {
        while (leaveTime > 0f)
        {
            
            yield return new WaitForSeconds(1f);
            leaveTime--;

            if (leaveTime <= 0f)
            {
                StartCoroutine(Explose());
               
                Debug.Log("state.EXPLODING");
                yield break;
            }
        }
    }

    IEnumerator Explose()
    {
        bombState = state.EXPLODING;
        isExploding = true;

        this.GetComponent<BombHover>().NotShiver();
        foreach (GameObject obj in cols)
        {
            obj.GetComponent<BombArea>().PushOut();
            obj.GetComponent<BoxCollider>().enabled = false;
            Debug.Log(obj.name);
        }
        float lerpTime = 0f;
        float speed = 0.2f;
        bombState = state.USED;
        Debug.Log("Bomb USED");
        Handheld.Vibrate();

        while (true)
        {
            lerpTime += Time.deltaTime * speed;

            if (transform.localScale.x >= 500)
            {
                transform.localScale = Vector3.zero;
               
                yield break;
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 5, lerpTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, lerpTime);
            }
            yield return null;
        }
        transform.localScale = Vector3.zero;
        
        yield break;
    }



}
