using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ririri : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(GetExploded());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator GetExploded()
    {
        this.GetComponent<Rigidbody>().isKinematic = false;
        this.GetComponent<Rigidbody>().useGravity = true;
        this.GetComponent<Rigidbody>().AddForce(0f, 0f, 80.0f);

        yield return new WaitForSeconds(0.5f);
        this.GetComponent<Rigidbody>().isKinematic = true;
        this.GetComponent<Rigidbody>().useGravity = false;
        this.GetComponent<Rigidbody>().AddForce(0f, 0f, 0f);


        Debug.Log("is explding");
        yield break;
    }
}
