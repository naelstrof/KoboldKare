using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFluidOutput {
    void SetVolumePerSecond(float newVPS);
    bool isFiring { get; }
    void Fire(GenericReagentContainer b);
    void StopFiring();
}
