using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour {

    [SerializeField]
    PlayerBehavior playerBehavior;

    [SerializeField]
    LineRenderer lineRenderer;

    bool isDone;

    void Start()
    {
        playerBehavior.enabled = false;
        lineRenderer.enabled = false;
    }

	void Update()
    {
        // start counting
        if (!isDone && playerBehavior.enabled)
        {
            GameSingleton.instance.CountTime();
        }

        if (GameSingleton.instance.allowSnap)
        {
            playerBehavior.enabled = true;
        }

        if (GameSingleton.instance.score >= 8)
        {
            isDone = true;
        }
    }
}
