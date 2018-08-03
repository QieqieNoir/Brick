using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class combinedH : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		if (!isServer)
        {
            Debug.Log("NotServer");
            Destroy(this.gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
