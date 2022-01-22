using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFluidOutput {
    bool isFiring { get; }
    void Fire(GenericReagentContainer b);
    void StopFiring();
}
