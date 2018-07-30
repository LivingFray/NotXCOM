using System.Collections.Generic;

public abstract class Team {

    //The game controller
    public GameController Controller { get; set; }

    //The game board
    public Board GameBoard { get; set; }

    protected List<EntityController> entities = new List<EntityController>();

    //Called when a team's turn begins
    public abstract void OnTurnStart();
    
    //Called when a team's turn ends
    public abstract void OnTurnEnd();

    //Called when an entity is clicked on
    public abstract void EntityClicked(EntityController entity);
    
    //Called when an enemy entity is clicked on
    public abstract void EnemyClicked(EntityController entity);

    //Called when a tile is clicked on
    public abstract void TileClicked(Tile tile);

    //Adds the entities belonging to the team to the game
    public abstract void PopulateEntities();

    //Called every frame
    public abstract void Update();

    //Called when an entity dies
    public abstract void EntityDied(EntityController entity);

}
