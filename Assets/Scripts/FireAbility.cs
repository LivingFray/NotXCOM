using UnityEngine;


[CreateAssetMenu(menuName = "Abilities/Fire")]
public class FireAbility : Ability {

    protected override bool SelectActionImpl(Entity entity) {
        if(entity.ammo == 0) {
            return false;
        }
        entity.UpdateVisibleEntities();
        //Request UI change in entity
        entity.ShowVisibleEntities();
        return true;
    }

    public override void TriggerAction(Entity entity) {
        //TODO: Get weapon from entity and extract values that way
        byte cover;
        Entity enemy = entity.GetSelectedEntity(out cover);
        if (enemy == null) {
            return;
        }
        entity.gun.Fire(entity, enemy);
        //Pretty fire animations and such

        base.TriggerAction(entity);
    }
}
