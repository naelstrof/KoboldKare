using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reiikz.UnityUtils {

    public class BruteForce
    {
        
        public static GameObject AggressiveTreeFind(Transform t, string name) {
            foreach(Transform tt in t){
                if(tt.gameObject.name.Equals(name)) return tt.gameObject;
                if(tt.childCount > 0){
                    GameObject g = AggressiveTreeFind(tt, name);
                    if(g != null) return g;
                }
            }
            return null;
        }

    }

}
