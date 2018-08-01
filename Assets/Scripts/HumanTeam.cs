using UnityEngine;

public class HumanTeam : Team {

    bool turnActive = false;
    Entity currentEntity;
    public new readonly bool HasUI = true;

    //Temporarily use the same entity repeatedly
    public GameObject entityPrefab;

    public Vector3Int[] spawnPositions = {
        new Vector3Int(5, 0, 5),
        new Vector3Int(6, 0, 5),
        new Vector3Int(5, 0, 6),
        new Vector3Int(6, 0, 6),
    };

    public override void EntityClicked(Entity entity) {
        if (turnActive && entity.actions > 0) {
            //Make entity visibly selected (update hud, actions, etc)
            if(currentEntity != null) {
                currentEntity.OnDeselected();
            }
            currentEntity = entity;
            Controller.entitySelect.transform.parent = currentEntity.transform;
            Controller.entitySelect.transform.localPosition = new Vector3(0, 0, 0);
            Controller.entitySelect.SetActive(true);
            entity.OnSelected();
        }
        Controller.EntityClicked(entity);
    }

    public override void OnTurnStart() {
        turnActive = true;
        foreach (Entity ent in entities) {
            ent.actions = 2;
        }
        //Update UI stuff
    }

    public override void PopulateEntities() {
        for (int i = 0; i < spawnPositions.Length; i++) {
            GameObject newEnt = Object.Instantiate(entityPrefab, spawnPositions[i], Quaternion.identity);
            Entity ent = newEnt.GetComponent<Entity>();
            Controller.entities.Add(newEnt);
            entities.Add(ent);
            ent.team = this;
            ent.controller = Controller;
            ent.board = GameBoard;
            ent.entityUI = Controller.entityUI;
        }
    }

    public override void TileClicked(Tile tile) {
        if (turnActive) {
            if (currentEntity != null) {
                currentEntity.FollowPath(GameBoard.FindPath(currentEntity.GridPos, tile.gridPos));
                if(currentEntity.actions == 0) {
                    currentEntity.OnDeselected();
                    currentEntity = null;
                    Controller.entitySelect.SetActive(false);
                }
                CheckActionsLeft();
            }
        }
    }

    public override void OnTurnEnd() {
        if (currentEntity != null) {
            currentEntity.OnDeselected();
        }
        currentEntity = null;
        turnActive = false;
    }

    public override void Update() {
        if(Input.GetKeyDown(KeyCode.End)) {
            Controller.NextTurn();
        }
    }

    void CheckActionsLeft() {
        foreach (Entity ent in entities) {
            if(ent.actions != 0) {
                return;
            }
        }
        Controller.NextTurn();
    }

    public override void EnemyClicked(Entity entity) {
        //TODO: Maybe make clicking an enemy the same as selecting them via UI
        /*
        if(currentEntity != null) {
            currentEntity.ShootEnemy(entity);
            if (currentEntity.actions == 0) {
                currentEntity = null;
                Controller.entitySelect.SetActive(false);
                CheckActionsLeft();
            }
        }
        */
    }

    public override void EntityDied(Entity entity) {
        if(currentEntity == entity) {
            currentEntity = null;
        }
        if(!entities.Remove(entity)) {
            Debug.LogWarning("Attempted to remove entity not in list");
        } else {
            Controller.EntityDied(entity);
            if(entities.Count == 0) {
                Controller.TeamDied(this);
            }
        }
    }

    public override void AbilityClicked() {
        if(currentEntity.actions == 0) {
            currentEntity.OnDeselected();
            Controller.entitySelect.SetActive(false);
            currentEntity = null;
        }
    }
}
