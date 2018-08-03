using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombArea : NetworkBehaviour
{
    bool setLayer;
    bool isExploded;
    public bool hasPiece;
    GameObject insidePiece = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (!setLayer && this.transform.parent.GetComponent<BombBehavior>().isUsed)
        {
            this.gameObject.layer = 1 << 10;
            setLayer = true;
        }
	}

    void OnTriggerStay(Collider other)
    {
        Debug.Log("inininin: " + other.gameObject.name);
        Debug.Log("isAbsorbed(): " + other.GetComponent<PieceBehavior>().GetIsAbsorbed());
        if (other.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            insidePiece = other.gameObject;
            hasPiece = true;
            Debug.Log("inside Piece: " + insidePiece.name);
        }
    }

    public void PushOut()
    {
        if(hasPiece)
        {
            insidePiece.GetComponent<PieceBehavior>().BeExploded();
            isExploded = true;
            Debug.Log("PushOut");
        }
    }

}
