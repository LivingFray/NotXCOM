using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour {

    [HideInInspector]
    public Team team;
    [HideInInspector]
    public GameController controller;

    public float movementSpeed = 5.0f;

    public int movementPoints = 12;

    public byte actions = 2;

    public Vector3Int GridPos { get; private set; }

    public void SetPosition(Vector3Int position) {
        GridPos = position;
        transform.position = position;
    }

    public void MoveTo(Vector3Int target) {
        StartCoroutine(Move_Coroutine(target));
    }

    IEnumerator Move_Coroutine(Vector3Int target) {
        GridPos = target;
        float dist = (target - transform.position).magnitude;
        float d = 0;
        Vector3 start = transform.position;
        while (d < dist) {
            d += movementSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, d / dist);
            yield return null;
        }
    }

    public void FollowPath(Vector3Int[] path) {
        if(path == null) {
            Debug.LogWarning("No path to follow");
            return;
        }
        if(actions == 0 || path.Length > movementPoints) {
            return;
        }
        StartCoroutine(Path_Coroutine(path));
        actions--;
    }

    IEnumerator Path_Coroutine(Vector3Int[] path) {
        for (int i = 0; i < path.Length; i++) {
            yield return Move_Coroutine(path[i]);
        }
    }

    public void ShootEnemy(EntityController enemy) {
        byte cover;
        bool los = controller.HasLineOfSightDDA(GridPos, enemy.GridPos, out cover);
        if (los) {
            Debug.Log("Took shot at enemy " + cover);
            //Pretty fire animations and such
        } else {
            Debug.Log("Shot blocked");
        }
        actions--;
    }

    private void Awake() {
        GridPos = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }

    private void OnMouseDown() {
        team.EntityClicked(this);
    }
}
