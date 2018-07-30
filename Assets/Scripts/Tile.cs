using UnityEngine;
/*
 * Class for handling a tile. If a tile's side is blocked but the tile above
 * isn't then it provides half cover. If a tile and the one above are blocked
 * then it provides full cover.
 * If the floor is blocked and ceiling isn't then the tile is walkable
 */
enum CoverType {NONE, HALF, FULL};
enum CoverSides {NEGX, POSX, NEGY, POSY, NEGZ, POSZ};

public class Tile {

    public GameController controller;

    public class Cover {
        public byte negativeX, positiveX, negativeY, positiveY, negativeZ, positiveZ;
        public byte GetCover(byte side) {
            switch (side) {
                case 0:
                    return negativeX;
                case 1:
                    return positiveX;
                case 2:
                    return negativeY;
                case 3:
                    return positiveY;
                case 4:
                    return negativeZ;
                case 5:
                    return positiveZ;
                default:
                    return 0;
            }
        }
        public void SetCover(byte side, byte cover) {
            switch (side) {
                case 0:
                    negativeX = cover;
                    break;
                case 1:
                    positiveX = cover;
                    break;
                case 2:
                    negativeY = cover;
                    break;
                case 3:
                    positiveY = cover;
                    break;
                case 4:
                    negativeZ = cover;
                    break;
                case 5:
                    positiveZ = cover;
                    break;
                default:
                    break;
            }
        }
    }

    public Cover cover;

    [HideInInspector]
    public Vector3Int gridPos;

    GameObject _tile;

    //May or may not exist, refers to the physical representation of the tile
    public GameObject TileObject { get { return _tile; } set {
            _tile = value;
            _tile.GetComponent<TileInput>().tileController = this;
        } }

    public Tile(GameController controller) {
        cover = new Cover();
        this.controller = controller;
    }

    public void TileClicked() {
        controller.TileClicked(gridPos);
    }

}