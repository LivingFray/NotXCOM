using UnityEngine;

public class HumanTeam : Team {

    bool turnActive = false;
    EntityController currentEntity;

    //Temporarily use the same entity repeatedly
    public GameObject entityPrefab;

    public Vector3Int[] spawnPositions = {
        new Vector3Int(5, 0, 5),
        new Vector3Int(6, 0, 5),
        new Vector3Int(5, 0, 6),
        new Vector3Int(6, 0, 6),
    };

    public override void EntityClicked(EntityController entity) {
        Debug.Log("Ent Click");
        if (turnActive) {
            //Make entity visibly selected (update hud, actions, etc)
            currentEntity = entity;
        }
    }

    public override void OnTurnStart() {
        turnActive = true;
        //Update UI stuff
    }

    public override void PopulateEntities() {
        for (int i = 0; i < spawnPositions.Length; i++) {
            GameObject newEnt = Object.Instantiate(entityPrefab, spawnPositions[i], Quaternion.identity);
            Controller.entities.Add(newEnt);
            newEnt.GetComponent<EntityController>().team = this;
        }
    }

    public override void TileClicked(TileController tile) {
        Debug.Log("Click");
        if (turnActive) {
            if (currentEntity != null) {
                currentEntity.FollowPath(Controller.FindPath(currentEntity.GridPos, tile.gridPos));
            }
        }
    }

    public override void OnTurnEnd() {
        currentEntity = null;
    }

    public override void Update() {
        if(Input.GetKeyDown(KeyCode.End)) {
            Controller.NextTurn();
        }
    }
}
