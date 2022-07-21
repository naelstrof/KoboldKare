using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable {
    public float GetHealth();
    public void Damage(float amount);
    public void Heal(float amount);
}
