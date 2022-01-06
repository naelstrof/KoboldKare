using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHolder
{
    public Transform GetAttachPoint();
    public Transform GetUsedAttachPoint();
}
