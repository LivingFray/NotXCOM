using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject {
    //Display icon shown on ability bar
    public Sprite icon;
    //Name of the specific ability
    public string abilityName;
    //Brief description of the ability
    public string description;
    //Number of action points ability costs
    public byte cost;
    //Whether the ability uses all remaining action points for unit
    public bool endsTurn;

    //Called when an action for an entity is selected (usually through UI)
    public bool SelectAction(Entity entity) {
        if (entity.actions < cost) {
            return false;
        }
        return SelectActionImpl(entity);
    }

    protected abstract bool SelectActionImpl(Entity entity);

    //Called when the action is confirmed (happens after being selected)
    public virtual void TriggerAction(Entity entity) {
        entity.actions -= cost;
        if(endsTurn) {
            entity.actions = 0;
        }
        entity.team.AbilityClicked();
    }


}
