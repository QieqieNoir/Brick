using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.HelloAR;
using UnityEngine.Rendering;
using TMPro;

public class Controller : MonoBehaviour
{
    bool m_IsQuitting = false;

    [SerializeField]
    GameObject SearchingForPlaneUI;

    [SerializeField]
    GameObject TrackedPlanePrefab;

    [SerializeField]
    GameObject[] piecesPrefab;

    GameObject pieces;

    [SerializeField]
    int piecesNum;

    [SerializeField]
    GameObject wall;

    [SerializeField]
    GameObject canvas;

    [SerializeField]
    TextMeshProUGUI time;

    [SerializeField]
    TextMeshProUGUI score;

    private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();
    private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

    bool didSpawn = false;

    [SerializeField]
    float height = 1.2f;

    void Start()
    {
        wall.SetActive(false);

        time.SetText("Time: " + GameSingleton.instance.PrintTime());
        score.SetText("Score: " + GameSingleton.instance.PrintScore() + " /10");
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        _QuitOnConnectionErrors();

        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            if (!m_IsQuitting && Session.Status.IsValid())
            {
                SearchingForPlaneUI.SetActive(true);
            }

            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);

        // if found one plane don't add new plane
        if (!didSpawn)
        {
            for (int i = 0; i < m_NewPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
            }
        }

        // Hide snackbar when currently tracking at least one plane.
        Session.GetTrackables<TrackedPlane>(m_AllPlanes);
        bool showSearchingUI = true;

        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                showSearchingUI = false;
                break;
            }
        }
        bool spawn = !showSearchingUI;

        SearchingForPlaneUI.SetActive(showSearchingUI);

        if (spawn && !didSpawn)
        {
            SpawnPieces();
            wall.SetActive(true);
            didSpawn = true;
        }

        if (GameSingleton.instance.allowSnap)
        {
            time.SetText("Time: " + GameSingleton.instance.PrintTime());
            score.SetText("Score: " + GameSingleton.instance.PrintScore() + " /10");
        }
    }

    private void _QuitOnConnectionErrors()
    {
        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    private void _DoQuit()
    {
        Application.Quit();
    }

    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }

    void SpawnPieces()
    {
        // spawn pieces
        TrackedPlane trackedPlane = null;
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                trackedPlane = m_AllPlanes[i];
                break;
            }
        }

        GameSingleton.instance.Anchor(trackedPlane.CenterPose.position);
			
        for (int i = 0; i < piecesNum; i++)
        {
			float yRange = Random.Range(0, 3f);
			float xRange = Random.Range(-3f, 3f);
			float zRange = Random.Range(-3f, 3f);
            int index = i % piecesPrefab.Length;
            pieces = Instantiate(piecesPrefab[index], trackedPlane.CenterPose.position + new Vector3(xRange, yRange, zRange), Random.rotation);
			// store piece list
			GameSingleton.instance.spawnedPieces.Add(pieces);

            // locate wall
            wall.transform.position = trackedPlane.CenterPose.position + new Vector3(0f, height, 0f);
        }
        Debug.Log("spawn");
		// store piece num info
		GameSingleton.instance.PieceNum (piecesNum);
		// allow snapping interaction
        GameSingleton.instance.AllowSnap(true);
    }

    void OnDisable()
    {
        time.SetText("Time: " + GameSingleton.instance.PrintTime());
        score.SetText("Score: " + GameSingleton.instance.PrintScore() + " /10");
    }
}
