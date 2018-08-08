using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Overwatch")]
public class OverwatchAbility : Ability {
    protected override bool SelectActionImpl(Entity entity) {
        entity.onOverwatch = true;
        return true;
    }
}
