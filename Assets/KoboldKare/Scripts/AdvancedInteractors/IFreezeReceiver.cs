using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFreezeReciever {
    void OnFreeze(Kobold k);
    void OnEndFreeze();
}
