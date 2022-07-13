using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Reiikz.KoboldKare.Klamp {

    

    public class ModAPI : MonoBehaviour
    {

        public static ModAPI instance;

        public ItemDatabases itemDatabse;

        public static ItemDatabases getItemDatabases { private set {} get { return ModAPI.instance.itemDatabse; } }

        void Start() {
            if(instance != null) throw new Exception("Two ModAPI instances can't exist at the same time");
            else
                instance = this;
        }

    }



}

