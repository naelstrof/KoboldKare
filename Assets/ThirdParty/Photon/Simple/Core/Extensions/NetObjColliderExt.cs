// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Photon.Utilities;
using UnityEngine;

namespace Photon.Pun.Simple.Internal
{
    /// <summary>
    /// Utilities for finding/indexing Colliders on a NetObject
    /// </summary>
    public static class NetObjColliderExt
    {
        public readonly static List<Component> reusableComponents = new List<Component>();
        public readonly static List<Collider> reusableColliders = new List<Collider>();
        public readonly static List<Collider2D> reusableColliders2D = new List<Collider2D>();

        /// <summary>
        /// Finds all active and inactive colliders on an object
        /// </summary>
        /// <param name="netObj"></param>
        public static void IndexColliders(this NetObject netObj)
        {
            var indexed = netObj.indexedColliders;
            var lookup = netObj.colliderLookup;

            lookup.Clear();
            indexed.Clear();

#if PUN_2_OR_NEWER
            /// Find ALL components, we will loop through this.
            netObj.transform.GetNestedComponentsInChildren<Component, NetObject>(reusableComponents);
#endif
            /// Check each component to see if it is a Collider/Collider2D
            int compCnt = reusableComponents.Count;
            for (int c = 0; c < compCnt; ++c)
            {
                Component comp = reusableComponents[c];
                Collider collider = comp as Collider;
                if (collider)
                {
                    indexed.Add(comp);
                }
                else
                {
                    Collider2D collider2D = comp as Collider2D;
                    if (collider2D)
                        indexed.Add(comp);
                }
            }

            /// Populate the lookup with the found colliders
            for (int i = 0, cnt = indexed.Count; i < cnt; ++i)
                lookup.Add(indexed[i], i);

            netObj.bitsForColliderIndex = (indexed.Count - 1).GetBitsForMaxValue();
        }

        /// <summary>
        /// Finds the first child collider or collider 2D. Retuns the count of colliders found.
        /// </summary>
        public static int GetFirstChildCollider(this Transform transform, ref Component firstFoundCollider, bool countTriggers, bool countNonTriggers)
        {
            if (!countTriggers && !countNonTriggers)
            {
                Debug.LogError("Counting Colliders, but args indicate to ignore everything. Set one to true.");
                firstFoundCollider = null;
                return 0;
            }

            /// First try 3d
            transform.GetComponentsInChildren(true, reusableColliders);
            int cnt = reusableColliders.Count;
            if (cnt > 0)
            {
                if (countTriggers && countNonTriggers)
                {
                    firstFoundCollider = reusableColliders[0];
                    return cnt;
                }

                int foundCount = 0;
                firstFoundCollider = null;

                for (int i = 0; i < cnt; ++i)
                {
                    Collider col = reusableColliders[i];
                    if (countTriggers ? col.isTrigger : !col.isTrigger)
                    {
                        if (firstFoundCollider == null)
                            firstFoundCollider = col;

                        foundCount++;
                    }
                }

                return foundCount;
            }

            /// None found, try 2D
            transform.GetComponentsInChildren(true, reusableColliders2D);
            int cnt2D = reusableColliders2D.Count;
            if (cnt2D > 0)
            {
                if (countTriggers && countNonTriggers)
                {
                    firstFoundCollider = reusableColliders[0];
                    return cnt;
                }

                int foundCount = 0;
                firstFoundCollider = null;

                for (int i = 0; i < cnt2D; ++i)
                {
                    Collider2D col = reusableColliders2D[i];
                    if (countTriggers ? col.isTrigger : !col.isTrigger)
                    {
                        if (firstFoundCollider == null)
                            firstFoundCollider = col;

                        foundCount++;
                    }
                }
                return foundCount;
            }

            /// None found, return null and 0
            firstFoundCollider = null;
            return 0;


        }

        /// <summary>
        /// Finds the first child collider or collider 2D. Retuns the count of colliders found.
        /// </summary>
        public static int CountChildCollider(this Transform transform, bool countTriggers, bool countNonTriggers)
        {
            if (!countTriggers && !countNonTriggers)
            {
                Debug.LogError("Counting Colliders, but args indicate to ignore everything. Set one to true.");
                return 0;
            }

            /// First try 3d
            transform.GetComponentsInChildren(true, reusableColliders);
            int cnt = reusableColliders.Count;
            if (cnt > 0)
            {
                if (countTriggers && countNonTriggers)
                {
                    return cnt;
                }

                int foundCount = 0;

                for (int i = 0; i < cnt; ++i)
                {
                    Collider col = reusableColliders[i];
                    if (countTriggers ? col.isTrigger : !col.isTrigger)
                    {
                        foundCount++;
                    }
                }

                return foundCount;
            }

            /// None found, try 2D
            transform.GetComponentsInChildren(true, reusableColliders2D);
            int cnt2D = reusableColliders2D.Count;
            if (cnt2D > 0)
            {
                if (countTriggers && countNonTriggers)
                {
                    return cnt;
                }

                int foundCount = 0;

                for (int i = 0; i < cnt2D; ++i)
                {
                    Collider2D col = reusableColliders2D[i];
                    if (countTriggers ? col.isTrigger : !col.isTrigger)
                    {
                        foundCount++;
                    }
                }
                return foundCount;
            }

            /// None found, return 0
            return 0;


        }
    }
}

