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
}
