using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Mozzarella;
using UnityEngine;

public class MozzarellaPool : GenericPool<Mozzarella> {
    public static MozzarellaPool instance;
    private void Start() {
        instance = this;
    }
}
