using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using GoogleARCore;
using TMPro;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class PlayerBehaviorNetworking : NetworkBehaviour
{
    GameObject piece;
    GameObject matchedGrid;
    GameObject player;

    // raycast
    Transform camera;
    int layerMask;
    [SerializeField]
    float hoverDis;

    bool isInteractable;
    public bool GetIsInteractable()
    {
        return isInteractable;
    }
    bool isHovering;

    bool isSnapped;
    public bool GetIsSnapped()
    {
        return isSnapped;
    }

    bool isTapped;
    bool isFalling;
    bool hasFall;
    bool isSnapping;

    int bombLayer = 1 << 9;

    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip snapSound;
    [SerializeField]
    AudioClip fallSound;
    [SerializeField]
    AudioClip finalSound;

    TextMeshProUGUI time;

    bool hasCanvas;
    bool startCount;
    bool isplayed;
    bool isFinal;

    [SerializeField]
    float playTime = 180f;

    string minutes;
    string seconds;

    float threshold = 0.5f;
    [SerializeField]
    float lerpRate = 5;

    [SyncVar]
    Vector3 syncPos;
    Vector3 lastPos;
    [SyncVar]
    Vector3 player1;
    [SyncVar]
    Vector3 player1last;
    [SyncVar]
    Vector3 player2;
    [SyncVar]
    Vector3 player2last;

    void Start()
    {
        camera = this.GetComponent<Camera>().transform;
        layerMask = LayerMask.GetMask("Piece");
        syncPos = GetComponent<Transform>().position;

        if (isServer)
        {
            this.gameObject.tag = "player2";
            player2 = camera.position;
            player2last = camera.position;
        }
        else
        {
            this.gameObject.tag = "player2";
            player2 = camera.position;
            player2last = camera.position;
        }
    }

    // manually update player's transform information
    // if this is not updated, the host player cannot see the client progress.
    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
    }

    void Update()
    {
        // ui
        if (GameObject.Find("ScoreBoard") && !time && !hasCanvas)
        {
            time = GameObject.Find("time").GetComponent<TextMeshProUGUI>();

            if (isServer)
            {
 //               GameObject.Find("colorC").SetActive(false);
                GameObject.Find("colorH").SetActive(false);
            }
            else
            {
                GameObject.Find("colorH").SetActive(false);
            }

            hasCanvas = true;
        }

        // count time
        if (hasCanvas && GameSingleton.instance.allowSnap && !startCount)
        {
            StartCoroutine(CountDown(playTime));
        }

        // raycast
        Ray ray = new Ray(camera.position, camera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hoverDis, layerMask) && !isFalling)
        {
            // store hit info
            piece = hit.collider.gameObject;
            if (piece.tag == "combined")
            {
                piece = piece.transform.parent.gameObject;
            }
            Debug.Log("Is hitting gameObject:" + piece);

            // check if the piece is snapped, if it is interactive, and if it is interacting
            if (!isSnapped && !isInteractable && !isHovering)
            {
                // if host
                if (isServer)
                {
                    if (piece.tag == "piece3" || piece.tag == "piece4")
                    {
                        Interactable(piece);
                    }
                    else
                    {
                        isInteractable = false;
                    }
                }
                // if client
                else
                {
                    if (piece.tag == "piece3" || piece.tag == "piece4")
                    {
                        Interactable(piece);
                    }
                    else
                    {
                        isInteractable = false;
                    }
                }
            }

            // if not tapping, check if it is close to grid otherwise release the piece
            if (!isTapped)
            {
                if (piece && isSnapped && piece.tag != "combined")
                {
                    // match
                    if (piece.GetComponent<PieceBehavior>().GetEnableMatch())
                    {
                        piece.GetComponent<PieceBehavior>().Match();
                        piece.GetComponent<PieceHover>().NotHover();
                        isSnapped = false;
                        isSnapping = false;
                        piece = null;
                        Debug.Log("go to the grid");
                    }
                    // release
                    else
                    {
                        if (isServer)
                        {
                            Release();
                            RpcRelease(piece);
                            isSnapping = false;
                        }
                        else
                        {
                            Release();
                            CmdRelease(piece);
                            isSnapping = false;
                        }
                    }
                }
                if (piece && isSnapped && piece.tag == "combined")
                {
                    
                    if (isServer)
                    {
                        Release();
                        RpcRelease(piece);
                        isSnapping = false;
                    }
                    else
                    {
                        Release();
                        CmdRelease(piece);
                        isSnapping = false;
                    }
                    Debug.Log("Release Combined");
                }
            }

        }

        // if there is no hit
        else
        {
            isInteractable = false;

            if (piece)
            {
                isHovering = false;
                isTapped = false;
                isSnapped = false;
                piece.GetComponent<PieceHover>().NotHover();
                if (piece.transform.parent)
                {
                    piece.transform.parent = null;
                }
                piece = null;
            }
        }

        // detect tapping and determine snapping
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
                {
                    isTapped = true;

                    // for single piece
                    if (piece && piece.transform.parent == null && piece.tag != "combined" && !isSnapping)
                    {
                        // if host
                        if (isServer)
                        {
                            if (piece.tag == "piece3" || piece.tag == "piece4")
                            {
                                //Snap();
                                CmdSnap(piece);
                                isSnapping = true;
                            }
                        }
                        // if client
                        else
                        {
                            if (piece.tag == "piece3" || piece.tag == "piece4")
                            {
                                Snap();
                                CmdSnap(piece);
                                isSnapping = true;
                            }
                        }
                    }

                    // for combined piece
                    if (piece && piece.tag == "combined" && !isSnapping)
                    {
                        if (isServer)
                        {
                            Vector3 _offsetH = camera.position - player1last;
                            SnapCombinedH(piece, _offsetH);
                            RpcSnapCombinedH(piece, _offsetH);          
                            Debug.Log("RpcSnapCombinedHHHHHHHHHH");                           
                        }
                        else
                        {
                            Vector3 _offsetC = camera.position - player2last;
                            SnapCombinedC(piece, _offsetC);
                            CmdSnapCombinedC(piece, _offsetC);
                            Debug.Log("CmdSnapCombinedCCCCCCCCCCCC");
                        }
                    }
                
                    Debug.Log("tapping");
                }
                else
                {
                    isTapped = false;
                    Debug.Log("not tapping");
                }
            }
        }

        // when a piece get absorbed into the grid, destory its collider not to get detected by raycasting
        if (piece && piece.GetComponent<PieceBehavior>().GetIsAbsorbed())
        {
            Destroy();
            CmdDestoryCollider(piece);

            if (isServer)
            {
                GameSingleton.instance.AddScore();
            }
            else
            {
                GameSingleton.instance.AddScore();
                CmdAddScore();
            }

            Debug.Log("total score: " + GameSingleton.instance.totalScore);
        }
        if (isServer)
        {
            player1last = camera.position;
        }
        else
        {
            player2last = camera.position;
        }
    }

    // show ray in the editor
    void DrawGizmos()
    {
        if (camera)
        {
            Gizmos.DrawRay(camera.position, camera.forward * 1);
        }
    }

    void OnDrawGizmos()
    {
        DrawGizmos();
    }

    void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }
    
    void Interactable(GameObject piece)
    {
        Debug.Log("piece: " + piece + " is interactable");
        isInteractable = true;
        isHovering = true;
        piece.GetComponent<PieceHover>().Hover();
    }

    void Snap()
    {
        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        piece.transform.parent = camera;
        piece.transform.rotation = Quaternion.identity;
        isSnapped = true;
        isSnapping = true;
        hasFall = false;
        Debug.Log("snap");
        if (!audioSource.isPlaying && !isplayed)
        {
            audioSource.PlayOneShot(snapSound);
            isplayed = true;
        }
    }

    [Command]
    void CmdSnap(GameObject gameObject)
    {
        NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        piece.transform.parent = camera;
        piece.transform.rotation = Quaternion.identity;
        isSnapped = true;
        isSnapping = true;
        hasFall = false;

        if (!audioSource.isPlaying && !isplayed)
        {
            audioSource.PlayOneShot(snapSound);
            isplayed = true;
        }
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        Debug.Log("cmd snap");
    }

    void SnapCombinedH(GameObject gameObject, Vector3 offset)
    {
        if (piece.GetComponent<PieceHover>().isShivering)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = true;
        hasFall = false;

        gameObject.GetComponent<CombinedBehavior>().isTappedH = true;
        if (gameObject.GetComponent<CombinedBehavior>().isTappedC == true)
            gameObject.transform.position = gameObject.transform.position + offset / 2;
        Debug.Log("SnapCombinedH" + gameObject.GetComponent<CombinedBehavior>().offsetH);
    }

    [ClientRpc]
    void RpcSnapCombinedH(GameObject gameObject, Vector3 offset)
    {
       if (piece.GetComponent<PieceHover>().isShivering)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = true;
        hasFall = false;

        gameObject.GetComponent<CombinedBehavior>().isTappedH = true;
        if (gameObject.GetComponent<CombinedBehavior>().isTappedC == true)
            gameObject.transform.position = gameObject.transform.position + offset / 2;
        Debug.Log("RpcSnapCombinedH: " + gameObject.GetComponent<CombinedBehavior>().offsetH);
    }



    void SnapCombinedC(GameObject gameObject, Vector3 offset)
    {
        if (piece.GetComponent<PieceHover>().isShivering)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = true;
        hasFall = false;

        gameObject.GetComponent<CombinedBehavior>().isTappedC = true;
        if (gameObject.GetComponent<CombinedBehavior>().isTappedH == true)
            gameObject.transform.position = gameObject.transform.position + offset / 2;
        Debug.Log("SnapCombinedC: " + gameObject.GetComponent<CombinedBehavior>().offsetC);
    }

    [Command]
    void CmdSnapCombinedC(GameObject gameObject, Vector3 offset)
    {
        NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        isSnapped = true;
        hasFall = false;

        gameObject.GetComponent<CombinedBehavior>().isTappedC = true;
        if (gameObject.GetComponent<CombinedBehavior>().isTappedH == true)
            gameObject.transform.position = gameObject.transform.position + offset / 2;
        Debug.Log("CmdSnapCombinedC: " + gameObject.GetComponent<CombinedBehavior>().offsetC);
    }


    void Release()
    {
        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }
        if (piece.tag == "combined")
        {
            if (isServer)
            {
                piece.GetComponent<CombinedBehavior>().isTappedH = false;
                Debug.Log("Release isTappedHHHHHHHHHh");
            }
            else
            {
                piece.GetComponent<CombinedBehavior>().isTappedC = false;
                Debug.Log("Release isTappedCCCCCCCCCCc");
            }
            isSnapped = false;
        }
        else
        {
            isSnapped = false;
            piece.transform.parent = null;
            StartCoroutine(PieceFall(piece));
            hasFall = true;

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(fallSound);
            }

            isplayed = false;
        }

        isSnapping = false;
        Debug.Log("release");
    }

    [Command]
    void CmdRelease(GameObject piece)
    {
        NetworkIdentity pieceId = gameObject.GetComponent<NetworkIdentity>();
        if (!isServer && pieceId.GetComponent<NetworkIdentity>().hasAuthority)
        {
            pieceId.RemoveClientAuthority(connectionToClient);
        }
        pieceId.AssignClientAuthority(connectionToClient);

        if (piece.GetComponent<PieceHover>().isShivering || piece.GetComponent<PieceHover>().isBlinking)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        if (piece.tag == "combined")
        {
            piece.GetComponent<CombinedBehavior>().isTappedC = false;
            Debug.Log("Release isTappedCCCCCCCCCCcccccccccc");

            isSnapped = false;
        }
        else
        {
            isSnapped = false;
            piece.transform.parent = null;
            StartCoroutine(PieceFall(piece));
            hasFall = true;

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(fallSound);
            }

            isplayed = false;
        }
        isSnapping = false;
        Debug.Log("cmd release");
    }

    [ClientRpc]
    void RpcRelease(GameObject piece)
    {
        if (piece.GetComponent<PieceHover>().isShivering)
        {
            piece.GetComponent<PieceHover>().NotHover();
        }

        if (piece.tag == "combined")
        {
            piece.GetComponent<CombinedBehavior>().isTappedH = false;
            Debug.Log("Release isTappedHHHHHHHHHhhhhhhhhhhhh");
            isSnapped = false;
        }
        else
        {
            isSnapped = false;
            piece.transform.parent = null;
            StartCoroutine(PieceFall(piece));
            hasFall = true;

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(fallSound);
            }

            isplayed = false;
        }
        isSnapping = false;
        Debug.Log("rpc release");
    }

    void Destroy()
    {
//        piece.GetComponent<PieceBehavior>().SetIsAbsorbed(false);
        //        piece.GetComponent<PieceBehavior>().col.isTrigger = false;
        //        piece.GetComponent<PieceBehavior>().col.enabled = false;
        piece.layer = bombLayer;
//        piece.GetComponent<PieceBehavior>().enabled = false;
        isSnapped = false;
        hasFall = true;
        isplayed = false;
    }

    // this function is called from the piece
    [Command]
    public void CmdDestoryCollider(GameObject gameObject)
    {
        RpcDestroyCollider(gameObject);
    }

    [ClientRpc]
    public void RpcDestroyCollider(GameObject gameObject)
    {
        //       gameObject.GetComponent<PieceBehavior>().col.isTrigger = false;
        //       gameObject.GetComponent<PieceBehavior>().col.enabled = false;
        piece.layer = bombLayer;
//        gameObject.GetComponent<PieceBehavior>().enabled = false;
        isplayed = false;
        Debug.Log("destroy collider");
    }

    [Command]
    void CmdAddScore()
    {
        RpcAddScore();
    }

    [ClientRpc]
    void RpcAddScore()
    {
        GameSingleton.instance.AddScore();
        Debug.Log("add score");
    }

    [Command]
    public void CmdAssignClientAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    [Command]
    public void CmdRemoveClientAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToClient);
    }

    IEnumerator BouncePiece()
    {
        float lerpTime = 0f;
        float lerpSpeed = 0.5f;
        Vector3 releasePos = piece.transform.position + transform.forward;

        while (true)
        {
            if (lerpTime >= 1f)
            {
                yield break;
            }
            else if (piece != null)
            {
                lerpTime += Time.deltaTime * lerpSpeed;
                piece.transform.position = Vector3.Lerp(piece.transform.position, releasePos, lerpTime);
            }
            yield return null;
        }
    }

    IEnumerator PieceFall(GameObject piece)
    {
        piece.GetComponent<Rigidbody>().isKinematic = false;
        piece.GetComponent<Rigidbody>().useGravity = true;
        isFalling = true;
        yield return new WaitForSeconds(0.5f);
        piece.GetComponent<Rigidbody>().isKinematic = true;
        piece.GetComponent<Rigidbody>().useGravity = false;
        piece = null;
        isFalling = false;
        Debug.Log("fall");
        yield break;
    }

    void LerpPosition()
    {
        if (!hasAuthority)
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvidePositionToServer(Vector3 pos)
    {
        syncPos = pos;
    }

    void TransmitPosition()
    {
        if (hasAuthority && Vector3.Distance(transform.position, lastPos) > threshold)
        {
            CmdProvidePositionToServer(transform.position);
            lastPos = transform.position;
        }
    }

    IEnumerator CountDown(float totalTime)
    {
        startCount = true;
        playTime = totalTime;
        bool _noBomb = true;
        GameObject gameController = GameObject.FindGameObjectWithTag("controller");
        while (playTime > 0f)
        {
            
            playTime--;
            minutes = Mathf.FloorToInt(playTime / 60).ToString("00");
            seconds = Mathf.RoundToInt(playTime % 60).ToString("00");
            time.SetText(minutes + ":" + seconds);
            if(minutes == "02" && seconds == "00" && _noBomb)
            {
                gameController.GetComponent<GameController>().SpawnBomb();
            }

            yield return new WaitForSeconds(1f);

            if (playTime <= 0f)
            {
                StartCoroutine(Final());
                // avoid negative value
                time.SetText("00:00");
                yield break;
            }
        }
    }

    IEnumerator Final()
    {
        isFinal = true;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(finalSound);
        }
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Score");
    }

    
}
