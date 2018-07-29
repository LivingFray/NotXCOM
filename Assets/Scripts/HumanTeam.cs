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
        if (turnActive && entity.actions > 0) {
            //Make entity visibly selected (update hud, actions, etc)
            currentEntity = entity;
            Controller.entitySelect.transform.parent = currentEntity.transform;
            Controller.entitySelect.transform.localPosition = new Vector3(0, 0, 0);
            Controller.entitySelect.SetActive(true);
        }
        Controller.EntityClicked(entity);
    }

    public override void OnTurnStart() {
        turnActive = true;
        foreach (EntityController ent in entities) {
            ent.actions = 2;
        }
        //Update UI stuff
    }

    public override void PopulateEntities() {
        for (int i = 0; i < spawnPositions.Length; i++) {
            GameObject newEnt = Object.Instantiate(entityPrefab, spawnPositions[i], Quaternion.identity);
            EntityController ent = newEnt.GetComponent<EntityController>();
            Controller.entities.Add(newEnt);
            entities.Add(ent);
            ent.team = this;
            ent.controller = Controller;
        }
    }

    public override void TileClicked(TileController tile) {
        if (turnActive) {
            if (currentEntity != null) {
                currentEntity.FollowPath(Controller.FindPath(currentEntity.GridPos, tile.gridPos));
                if(currentEntity.actions == 0) {
                    currentEntity = null;
                    Controller.entitySelect.SetActive(false);
                }
                CheckActionsLeft();
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

    void CheckActionsLeft() {
        foreach (EntityController ent in entities) {
            if(ent.actions != 0) {
                return;
            }
        }
        Controller.NextTurn();
    }

    public override void EnemyClicked(EntityController entity) {
        if(currentEntity != null) {
            currentEntity.ShootEnemy(entity);
            if (currentEntity.actions == 0) {
                currentEntity = null;
                Controller.entitySelect.SetActive(false);
            }
        }
    }
}
