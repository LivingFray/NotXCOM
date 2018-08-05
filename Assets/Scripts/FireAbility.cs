using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Abilities/Fire")]
public class FireAbility : Ability {

    protected override bool SelectActionImpl(Entity entity) {
        if(entity.ammo == 0) {
            Debug.Log("Not enough ammo");
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
        if(enemy == null) {
            return;
        }
        float hitChance = entity.GetHitChance(enemy, cover);
        entity.ammo--;
        if (Random.Range(0.0f, 1.0f) < hitChance) {
            int damage = Random.Range(entity.gun.minDamage, entity.gun.maxDamage + 1);
            enemy.Damage(damage);
            entity.ShowHitIndicator(hitChance, damage, enemy);
        } else {
            entity.ShowHitIndicator(hitChance, 0, enemy);
        }
        //Pretty fire animations and such

        base.TriggerAction(entity);
    }
}
