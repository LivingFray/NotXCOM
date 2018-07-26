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

    Vector3Int gridPos;

    public void SetPosition(Vector3Int position) {
        gridPos = position;
        transform.position = position;
    }

    public void MoveTo(Vector3Int target) {
        gridPos = target;
        StartCoroutine(Move_Coroutine(target));
    }

    IEnumerator Move_Coroutine(Vector3 target) {
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
        StartCoroutine(Path_Coroutine(path));
    }

    IEnumerator Path_Coroutine(Vector3Int[] path) {
        for (int i = 0; i < path.Length; i++) {
            yield return Move_Coroutine(path[i]);
        }
    }
}
