using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;

public class MainMenu : NetworkBehaviour {

    [SerializeField]
    NetworkLobbyManager lobbyManager;

    [SerializeField]
    GameObject background;
    [SerializeField]
    GameObject backgroundWithoutCubes;

    [SerializeField]
    GameObject enableMatch;
    [SerializeField]
    GameObject createRoom;
    [SerializeField]
    GameObject findRoom;
    [SerializeField]
    GameObject joinRoom;
    [SerializeField]
    GameObject startGame;
    [SerializeField]
    GameObject readyGame;
    [SerializeField]
    InputField typeRoom;
    [SerializeField]
    InputField roomList;

    [SerializeField]
    AudioClip buttonClick;

    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        NetworkServer.Reset();
        NetworkServer.ResetConnectionStats();

        FirstSetting();
    }

    void FirstSetting()
    {
        background.SetActive(true);
        enableMatch.SetActive(true);
        backgroundWithoutCubes.SetActive(false);
        createRoom.SetActive(false);
        startGame.SetActive(false);
        readyGame.SetActive(false);
        findRoom.SetActive(false);
        joinRoom.SetActive(false);
        typeRoom.gameObject.SetActive(false);
        roomList.gameObject.SetActive(false);
    }

    public void OnClickEnableMatchMaker()
    {
        lobbyManager.StartMatchMaker();
        enableMatch.SetActive(false);
    }

    public void GenerateRoom()
    {
        backgroundWithoutCubes.SetActive(true);
        background.SetActive(false);
        typeRoom.gameObject.SetActive(true);
        createRoom.SetActive(true);
        findRoom.SetActive(true);
    }

    public void CreateRoomForMatch()
    {
        lobbyManager.matchMaker.CreateMatch(typeRoom.text, (uint)lobbyManager.maxPlayers, true, "", "", "", 0, 0, OnMatchCreate);
        joinRoom.SetActive(false);
        findRoom.SetActive(false);
        startGame.SetActive(true);
        createRoom.SetActive(false);
        Debug.Log("match name: " + typeRoom.text);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        Debug.Log("Success: " + success);
        Debug.Log(extendedInfo);
        Debug.Log(matchInfo);
        lobbyManager.OnMatchCreate(success, extendedInfo, matchInfo);
    }

    public void ButtonSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(buttonClick);
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartHost());
    }

    public void ReadyGame()
    {
        StartCoroutine(Ready());
    }

    public void JoinMatch()
    {
        lobbyManager.matchMaker.JoinMatch(lobbyManager.matches[lobbyManager.matches.Count - 1].networkId, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
        readyGame.SetActive(true);
        joinRoom.SetActive(false);
    }

    public void FindMatch()
    {
        lobbyManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, lobbyManager.OnMatchList);
        Debug.Log(lobbyManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, lobbyManager.OnMatchList));
        createRoom.SetActive(false);
        typeRoom.gameObject.SetActive(false);
        roomList.gameObject.SetActive(true);
        findRoom.SetActive(false);
        joinRoom.SetActive(true);
        StartCoroutine(FindMatches());
    }

    IEnumerator FindMatches()
    {
        yield return new WaitForSeconds(0.3f);

        foreach (var match in lobbyManager.matches)
        {
            roomList.text = typeRoom.text;
            Debug.Log("match name: " + typeRoom.text);

            lobbyManager.matchName = match.name;
            lobbyManager.matchSize = (uint)match.currentSize;
        }

        roomList.gameObject.SetActive(true);
        roomList.text = typeRoom.text;
    }

    IEnumerator Ready()
    {
        yield return new WaitForSeconds(1f);
        lobbyManager.lobbySlots[1].SendReadyToBeginMessage();
        yield break;
    }

    IEnumerator StartHost()
    {
        yield return new WaitForSeconds(1f);
        lobbyManager.lobbySlots[0].SendReadyToBeginMessage();
        yield break;
    }
}
