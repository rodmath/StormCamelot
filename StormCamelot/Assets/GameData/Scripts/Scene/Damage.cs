using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//https://dnd5e.fandom.com/wiki/Damage_Type
public enum DamageType
{
    acid, bludgeoning, cold, fire, force, lightning, necrotic, piercing, poison, psychic, radiant, slashing, thunder
}

public class Damage : MonoBehaviour {

    public DamageType type = DamageType.bludgeoning;
}
