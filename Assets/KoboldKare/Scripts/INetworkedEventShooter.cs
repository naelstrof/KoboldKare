using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkedEventShooter {
    Transform transform { get; }
    void FireEvent();
}
