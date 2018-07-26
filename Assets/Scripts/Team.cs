using System.Collections.Generic;

public abstract class Team {

    //The game controller
    GameController Controller { get; set; }

    protected List<EntityController> entities;

    //Called when a team's turn begins
    public abstract void OnTurnStart();

}
