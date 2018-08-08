using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Gun")]
public class Gun : ScriptableObject {
    public string gunName;
    public int maxAmmo;
    public int minDamage;
    public int maxDamage;

    //Maybe refactor hit falloff stuff to be different types (short,mid,long)
    public float baseHitChance;
    public float hitChanceFalloff;

    //TODO: Actually give weapons 3D models

    //Attempts to shoot at the target, returns false if can't get a LOS
    public bool Fire(Entity owner, Entity target) {
        byte cover;
        if (!owner.board.HasLineOfSight(owner.GridPos, target.GridPos, out cover)) {
            return false;
        }
        float hitChance = owner.GetHitChance(target, cover);
        owner.ammo--;
        if (Random.Range(0.0f, 1.0f) < hitChance) {
            int damage = Random.Range(owner.gun.minDamage, owner.gun.maxDamage + 1);
            target.Damage(damage);
            owner.ShowHitIndicator(hitChance, damage, target);
        } else {
            owner.ShowHitIndicator(hitChance, 0, target);
        }
        return true;
    }
}
