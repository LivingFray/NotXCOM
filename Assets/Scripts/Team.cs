using System.Collections.Generic;

public abstract class Team {

    //Sotres whether this team should display a UI to the screen
    public readonly bool HasUI;

    //The game controller
    public GameController Controller { get; set; }

    //The game board
    public Board GameBoard { get; set; }

    protected List<Entity> entities = new List<Entity>();

    //Called when a team's turn begins
    public abstract void OnTurnStart();
    
    //Called when a team's turn ends
    public abstract void OnTurnEnd();

    //Called when an entity is clicked on
    public abstract void EntityClicked(Entity entity);
    
    //Called when an enemy entity is clicked on
    public abstract void EnemyClicked(Entity entity);

    //Called when a tile is clicked on
    public abstract void TileClicked(Tile tile);

    //Adds the entities belonging to the team to the game
    public abstract void PopulateEntities();

    //Called every frame
    public abstract void Update();

    //Called when an entity dies
    public abstract void EntityDied(Entity entity);

    //Called when an ability is performed
    public abstract void AbilityClicked();

}
