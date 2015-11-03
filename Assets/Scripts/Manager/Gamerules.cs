﻿using UnityEngine;
using System.Collections;

public class Gamerules : MonoBehaviour {

    public static Gamerules _instance;

    #region Gamerules Variable
    [Header("Game Rules")]
    [Range (0,4)]
    public int playerAmmount = 0;
    [Tooltip("Defines the maximal use of an ability")]
    public uint abilityLimit;
    public float itemSpawnrate;
    [Tooltip("Time between player death and spawn")]
    public float timeDeathSpawn;
    [Range(0,100)]
    public float damageModifier;
    public Gamemode[] gameModeList = new Gamemode[2];

    //Player/Controller Selection
    [Header("Player Selection")]
    public GameObject[] playerPrefab = new GameObject[4];
    private GameObject[] playerSpawn = new GameObject[4];
    [Tooltip("Spawnpoints must have this given tag")]
    public string spawnTag;
    #endregion

    void Awake()
    {
        //Dont destroy the object on load of a new level
        DontDestroyOnLoad(gameObject);

        if (_instance == null) { _instance = this; }
    }
    
    //Create All Player when the level loads
    void OnLevelWasLoaded()
    {
        //Get all objecticts with the given tag
        playerSpawn = GameObject.FindGameObjectsWithTag(spawnTag);

        if (playerAmmount > 0)
        {
            for (int i = 0; i < playerAmmount; i++)
            {
                GameObject go = Instantiate(playerPrefab[i], playerSpawn[Random.Range(0, playerSpawn.Length)].transform.position, Quaternion.identity) as GameObject;

                if (PlayerSelection._instance.controller[i] == "KB") 
                {
                    go.GetComponent<Player>().playerAxis = "KB";
                    go.name = "KB_Player"; 
                } 
                if(PlayerSelection._instance.controller[i] == "P1")
                {
                    go.GetComponent<Player>().playerAxis = "P1";
                    go.name = "P1_Player";
                }

                if(PlayerSelection._instance.controller[i] == "P2")
                {
                    go.GetComponent<Player>().playerAxis = "P2";
                    go.name = "P2_Player";
                }

                if(PlayerSelection._instance.controller[i] == "P3")
                {
                    go.GetComponent<Player>().playerAxis = "P3";
                    go.name = "P3_Player";
                }

                if(PlayerSelection._instance.controller[i] == "P4")
                {
                    go.GetComponent<Player>().playerAxis = "P4";
                    go.name = "P4_Player";
                }
            }
        }
    }

    [System.Serializable]
    public class Gamemode
    {
        public string name;
        public int lifeLimit;
        [Range(0,100)]
        public float timeLimit;
    }
}
