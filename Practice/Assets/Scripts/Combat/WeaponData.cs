using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Melee,
    Pistol,
    Rifle,
    Smg,
    Sniper,
    Shotgun,
    Explosive
}

[System.Serializable]
public class RecoilData
{
    public Vector2 kick;          // X = horizontal, Y = vertical
    public float kickBack;        // Z axis pull
    public float returnSpeed;     // How fast it resets
    public float snappiness;      // How fast it kicks
}

public enum FireMode
{
    Semi,
    Auto,

}

[CreateAssetMenu]
public class WeaponData : ScriptableObject
{
    public int weaponId;
    public AudioClip shotClip;
    public AudioClip reloadClip;
    public WeaponType type;
    public ArmType armType;
    public FireMode fireMode;
    public RecoilData recoilData;
    public int damage;
    public float rpm;
    public float range;
    public int magSize;
    public float reloadTime;
    public bool isScoped;
    public int pelletCount;
    public float spread;
    public float splashRadius;
}
