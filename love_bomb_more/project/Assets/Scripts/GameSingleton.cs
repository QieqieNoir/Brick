using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleton : MonoBehaviour {

    public static GameSingleton instance;

    public bool allowSnap;
	public int pieceNum;
    public int snappedPiece;
    public bool isSnapped;
    public Vector3 anchor;
    public float playTime = 0f;
    public int score = 0;
    public int matchedPiece;

	public List<GameObject> spawnedPieces = new List<GameObject> ();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
	}

    public void AllowSnap(bool _allowSnap)
    {
        allowSnap = _allowSnap;
    }

	public void PieceNum(int _pieceNum)
	{
		pieceNum = _pieceNum;
	}

    public void SnappedPiece(int _snappedPiece)
    {
        snappedPiece = _snappedPiece;
    }

    public void IsSnapped(bool _isSnapped)
    {
        isSnapped = _isSnapped;
    }

    public void Anchor(Vector3 _anchor)
    {
        anchor = _anchor;
    }

    public void MatchedPiece(int _matchedPiece)
    {
        matchedPiece = _matchedPiece;
    }

    public void AddScore()
    {
        score++;
    }

    public string PrintScore()
    {
        return score.ToString();
    }

    public void CountTime()
    {
        playTime += Time.deltaTime;
    }

    public string PrintTime()
    {
        return playTime.ToString();
    }
}
