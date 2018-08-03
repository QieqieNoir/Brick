using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameSingleton : NetworkBehaviour {

    public static GameSingleton instance;

    [SyncVar]
    public bool allowSnap;
    [SyncVar]
    public int pieceNum;
    [SyncVar]
    public Vector3 anchor;

    public string formatedTime;

    public int totalScore = 0;

    public bool isPieceAbsorbed;
    [SyncVar]
    public GameObject targetGrid;

    public List<GameObject> wallGrids = new List<GameObject>();
    public List<GameObject> spawnedPieces = new List<GameObject>();

    public bool testIsSnapped;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != null)
        {
            Debug.Log("more than one singleton.");
        }
    }

	void Start()
	{
		if (spawnedPieces == null)
		{
			spawnedPieces.Clear();
		}

        if (wallGrids == null)
        {
            wallGrids.Clear();
        }

        targetGrid = null;
	}

    public void AllowSnap(bool _allowSnap)
    {
        allowSnap = _allowSnap;
    }

	public void PieceNum(int _pieceNum)
	{
		pieceNum = _pieceNum;
	}

    public void Anchor(Vector3 _anchor)
    {
        anchor = _anchor;
    }

    public void AddScore()
    {
        totalScore++;
    }

    public void SetIsPieceAbsorbed(bool _isPieceAbsorbed)
    {
        isPieceAbsorbed = _isPieceAbsorbed;
    }

    public string PrintScore()
    {
        return totalScore.ToString();
    }
}
