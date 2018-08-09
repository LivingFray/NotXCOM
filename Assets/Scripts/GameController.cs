using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

public class GameController : MonoBehaviour {
    public Material baseMaterial;
    public Material darkMaterial;
    public Material startMaterial;
    public Material endMaterial;

    public Material checkingMaterial;

    private bool placingStart = true;

    public GameObject line;

    public bool clearLOS = true;

    public int maxClimbHeight = 2;
    public int maxFallHeight = 4;

    //TODO: Make private?
    public List<Entity> entities;
    public Team[] teams;

    public GameObject entityPrefab;

    public GameObject turnText;

    public GameObject entitySelect;

    public GameObject entityUI;

    private int currentTeam = 0;


    Board board;


    // Use this for initialization
    void Start() {
        //Find board in game controller object
        board = GetComponent<Board>();
        if(board == null) {
            Debug.LogError("Could not find board in controller");
        }
        /*
        //Initialise map
        board.CreateBoard(20, 5, 20);
        //Create map
        board.GenerateTestBoard();
        */
        board.LoadBoard(GameObject.Find("Board"));
        //Create teams
        AddTeams();
        //Add entities
        foreach (Team t in teams) {
            t.PopulateEntities();
        }
        //Start game

        //Turn off entity select as nno entity is selected
        entitySelect.SetActive(false);
    }

    public void NextTurn() {
        teams[currentTeam].OnTurnEnd();
        currentTeam = (currentTeam + 1) % teams.Length;
        teams[currentTeam].OnTurnStart();
        turnText.GetComponent<TextMeshProUGUI>().text = "Player " + (currentTeam + 1) + "'s turn";
    }

    void AddTeams() {
        teams = new Team[2];
        //Human team
        teams[0] = new HumanTeam {
            Controller = this,
            GameBoard = board,
            entityPrefab = entityPrefab
        };
        //"""AI""" team (human for now, AI is hard)
        teams[1] = new HumanTeam {
            Controller = this,
            GameBoard = board,
            entityPrefab = entityPrefab
        };
        var spawnList = ((HumanTeam)teams[1]).spawnPositions;
        for (int i = 0; i < spawnList.Length; i++) {
            spawnList[i].x += 4;
        }
        //Set turn to player 1's (or player 0 if you use index position I guess)
        currentTeam = 0;
        teams[0].OnTurnStart();
    }

    //Called when an entity enters a tile
    public void EntityMoved(Entity ent, Vector3Int newTile) {
        //Loop through players in overwatch and test for shot
        //TODO: Track overwatched players separately?
        foreach(Entity e in entities) {
            //Entity died
            if(ent == null) {
                break;
            }
            if(!e.onOverwatch || e.team == ent.team) {
                continue;
            }
            e.TriggerOverwatch(ent);
        }
    }

    private void Update() {
        //Toggle begin/end node placement
        if (Input.GetKeyDown(KeyCode.Space)) {
            placingStart = !placingStart;
        }
        teams[currentTeam].Update();
    }

    public void TileClicked(Vector3Int tile) {
        teams[currentTeam].TileClicked(board.GetTile(tile));
    }

    public void EntityClicked(Entity entity) {
        if(entity.team == teams[currentTeam]) {
            return;
        }
        teams[currentTeam].EnemyClicked(entity);
    }

    public void TeamDied(Team team) {
        Debug.Log("GAME OVER");
    }
}
