using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehavior : MonoBehaviour {

    [SerializeField]
    Transform anchor;
    Vector3 relPos;
    Vector3 target;

    [SerializeField]
    float range;

    float lerpTime;
    [SerializeField]
    float lerpSpeed;

	// Use this for initialization
	void Start ()
    {
        relPos = new Vector3(anchor.position.x + range, anchor.position.y + range, anchor.position.z + range);
        target = transform.InverseTransformDirection(relPos);
    }
	
	// Update is called once per frame
	void Update ()
    {
        lerpTime += Time.deltaTime * lerpSpeed;
        transform.position = Vector3.Lerp(anchor.position, target, lerpTime);
	}
}
