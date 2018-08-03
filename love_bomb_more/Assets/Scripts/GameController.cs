using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GoogleARCore;
using TMPro;

public class GameController : NetworkBehaviour {

    [SerializeField] PlaneController planeController;

    [SerializeField] GameObject[] piecesPrefab;
    [SerializeField] int piecesNum;

    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject floorPrefab;

    [SerializeField] GameObject bombPrefab;

    [SerializeField] GameObject allGrids;
   [SerializeField] GameObject[] grids;

    // wall height
    [SerializeField] float height = 1.2f;
    bool didSpawn;
    bool ifWallSpawn;

    GameObject wallObject;
    Vector3 hitPos;

    // when add new objects, use unity editor to add prefabs
    [SerializeField] GameObject[] spawnablesPrefab;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip spawnSound;

    void Start ()
    {
        if (!isServer)
        {
            this.enabled = false;
            Debug.Log("not server");
            return;
        }

        Debug.Log("is server");
    }
	
    void Update ()
    {
        if(!ifWallSpawn)
        {
            SpawnWall();
            
        }


        if (ifWallSpawn && planeController.GetHasPlaneFound() && !didSpawn)
        {
            // server spawns pieces and wall
            SpawnPieces();
            SpawnFloor();
            //Spawnables();
            didSpawn = true;
        }
        

    }

    void SpawnPieces()
    {
        // spawn pieces
        for (int i = 0; i < piecesNum; i++)
        {
//            float yRange = Random.Range(0, 1.5f);
            float yRange = Random.Range(0.35f, 1.5f);
            float xRange = Random.Range(-2f, 2f);
            float zRange = Random.Range(-2f, 2f);
            int index = i % piecesPrefab.Length;
            Debug.Log(wallObject.transform.position);
            GameObject pieces = Instantiate(piecesPrefab[index], GameSingleton.instance.anchor + (new Vector3(xRange, yRange, zRange) + hitPos), Random.rotation);
            Debug.Log("piecesPrefab: " + index + piecesPrefab.ToString());
            NetworkServer.Spawn(pieces);
            // store piece list
            GameSingleton.instance.spawnedPieces.Add(pieces);
            Debug.Log("spawn");
        }

        // store piece num info
        GameSingleton.instance.PieceNum(piecesNum);
        // allow snapping interaction
        GameSingleton.instance.AllowSnap(true);
    }

    void SpawnWall()
    {
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            hitPos = hit.Pose.position;
            wallObject = Instantiate(wallPrefab, hit.Pose.position + new Vector3(0f, height, 0f), hit.Pose.rotation);

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
            // world evolves.
            var anchor = hit.Trackable.CreateAnchor(hit.Pose);

            wallObject.transform.parent = anchor.transform;

            NetworkServer.Spawn(wallObject);

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(spawnSound);
            }

            Debug.Log("wall");
            allGrids = wallObject.transform.Find(name:"Grid").gameObject;
            Debug.Log(allGrids.name);
            for(int i = 0; i < 20; i++)
            {
                grids[i] = allGrids.transform.Find(name: i+"").gameObject;
                Debug.Log(grids[i].name);
            }
            Debug.Log("wallGrid got!!");
            ifWallSpawn = true;
        }
   
    }

    void SpawnFloor()
    {
        GameObject floor = Instantiate(floorPrefab, GameSingleton.instance.anchor + new Vector3(0f, 0.2f, 0f), Quaternion.identity);
        NetworkServer.Spawn(floor);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        Debug.Log("floor");
    }

    public void SpawnBomb()
    {

        int rand;
        while(true)
        {
            rand = Random.Range(0, 20);
            if (!grids[rand].GetComponent<WallGrid>().hasPiece)
            {
                break;
            }
        }

        GameObject bomb = Instantiate(bombPrefab, GameSingleton.instance.anchor + grids[rand].transform.position, Quaternion.identity);
        NetworkServer.Spawn(bomb);
        bomb.transform.parent = grids[rand].transform;
        bomb.transform.localPosition = Vector3.zero;
        bomb.transform.parent = null;

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        Debug.Log("bomb");
    }

    // when add new spawn objects, use this function
    void Spawnables()
    {
        for (int i = 0; i < spawnablesPrefab.Length; i++)
        {
            GameObject spawnables = Instantiate(spawnablesPrefab[i]);
            NetworkServer.Spawn(spawnables);
        }
    }

}
