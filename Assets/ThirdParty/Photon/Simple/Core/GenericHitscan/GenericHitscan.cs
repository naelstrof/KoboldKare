// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Simple.Pooling;

#if GHOST_WORLD
using Photon.Pun.Simple.GhostWorlds;
#endif

namespace Photon.Pun.Simple
{

    public enum HitscanType { Raycast, SphereCast, CapsuleCast, BoxCast, OverlapSphere, OverlapCapsule, OverlapBox }

    /// <summary>
    /// Extension of the HitscanType Enum for testing the type of cast/overlap being used.
    /// </summary>
    public static class HitscanTypeExt
    {
        public static bool IsCast(this HitscanType hitscanType)
        {
            return ((int)hitscanType < 4);
        }
        public static bool IsOverlap(this HitscanType hitscanType)
        {
            return ((int)hitscanType > 3);
        }
        public static bool UsesRadius(this HitscanType hitscanType)
        {
            return hitscanType == HitscanType.SphereCast || hitscanType == HitscanType.CapsuleCast || hitscanType == HitscanType.OverlapSphere || hitscanType == HitscanType.OverlapCapsule;
        }
        public static bool IsBox(this HitscanType hitscanType)
        {
            return (hitscanType == HitscanType.BoxCast) || (hitscanType == HitscanType.OverlapBox);
        }
        public static bool IsCapsule(this HitscanType hitscanType)
        {
            return (hitscanType == HitscanType.CapsuleCast) || (hitscanType == HitscanType.OverlapCapsule);
        }
    }

    /// <summary>
    /// Utility class that contains methods to create a unified argument string for calling all of the Raycast/Overlap cast calls. Used by the Rewind Engine.
    /// </summary>
    public static class GenericHitscanExt
    {
        public static Collider[] reusableColliderArray = new Collider[64];
        public static RaycastHit[] reusableRayHitArray = new RaycastHit[64];
        public static List<NetworkHit> reusableHitscanHitList = new List<NetworkHit>();
        public static List<NetObject> reusableNetObjectsList = new List<NetObject>();

        #region Visualizers

        //#if (UNITY_EDITOR || DEVELOPMENT_BUILD)

        private static GameObject DebugSpherePrefab;
        private static GameObject DebugCylinderPrefab;
        private static GameObject DebugCubePrefab;

        private static GameObject SetUpDebugPrimitive(this GameObject go, string name, bool createCylinderChild = false)
        {
            go.name = name;
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            Object.DontDestroyOnLoad(go);

            Collider collider;

            if (createCylinderChild)
            {
                var child = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                child.GetComponent<Renderer>().material.color = Color.yellow;
                child.transform.parent = go.transform;
                child.transform.eulerAngles = new Vector3(90, 0, 0);
                collider = child.GetComponent<Collider>();
            }
            else
            {
                collider = go.GetComponent<Collider>();
            }
            Object.DestroyImmediate(collider);

            var rend = go.GetComponent<Renderer>();
            if (rend)
                rend.material.color = Color.yellow;

            return go;
        }

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateDebugPrimitives()
        {
            DebugSpherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere).SetUpDebugPrimitive("DebugSpherePrefab");
            Pool.AddPrefabToPool(DebugSpherePrefab.gameObject, 2, 2, null, true);

            DebugCubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube).SetUpDebugPrimitive("DebugCubePrefab");
            Pool.AddPrefabToPool(DebugCubePrefab.gameObject, 2, 2, null, true);

            DebugCylinderPrefab = new GameObject().SetUpDebugPrimitive("DebugCylinderPrefab", true);
            Pool.AddPrefabToPool(DebugCylinderPrefab.gameObject, 4, 4, null, true);
        }

        public static void VisualizeHitscan(this HitscanDefinition hd, Transform origin, float duration = .5f)
        {
            if (DebugSpherePrefab == null)
                CreateDebugPrimitives();

            //Transform tr = hd.sourceObject.transform;

            switch (hd.hitscanType)
            {
                case HitscanType.Raycast:
                    {
                        Vector3 pos = hd.useOffset ?
                             origin.position + origin.TransformDirection(hd.offset1) + (origin.forward * hd.distance * .5f) :
                             origin.position + (origin.forward * hd.distance * .5f);

                        Pool.Spawn(DebugCylinderPrefab, pos, origin.rotation, new Vector3(.1f, .1f, hd.distance * .5f), duration);
                    }
                    break;

                case HitscanType.BoxCast:
                    {
                        Vector3 pos = (hd.useOffset) ?
                            origin.position + origin.TransformDirection(hd.offset1) :
                            origin.position;

                        Vector3 pos2 = hd.useOffset ?
                            origin.position + origin.TransformDirection(hd.offset1) + (origin.forward * hd.distance) :
                            origin.position + (origin.forward * hd.distance);

                        Vector3 midpos = pos + (pos2 - pos) * .5f;

                        var rot = Quaternion.Euler(origin.eulerAngles + hd.orientation);
                        Vector3 scl = hd.halfExtents * 2;

                        Pool.Spawn(DebugCubePrefab, pos2, rot, scl, duration);
                        //DebugCylinder.Set(midpos, Quaternion.LookRotation(Vector3.up, tr.forward), new Vector3(.1f, cd.distance * .5f, .1f), duration);
                        Pool.Spawn(DebugCylinderPrefab, midpos, Quaternion.LookRotation(pos2 - pos, Vector3.up), new Vector3(.1f, .1f, hd.distance * .5f), duration);

                    }
                    break;

                case HitscanType.SphereCast:
                    {
                        Vector3 spos = hd.useOffset ?
                            origin.position + origin.TransformDirection(hd.offset1) :
                            origin.position;

                        Vector3 mpos = hd.useOffset ?
                            origin.position + origin.TransformDirection(hd.offset1) + (origin.forward * hd.distance * .5f) :
                            origin.position + (origin.forward * hd.distance * .5f);

                        Vector3 epos = hd.useOffset ?
                            origin.position + origin.TransformDirection(hd.offset1) + (origin.forward * hd.distance) :
                            origin.position + (origin.forward * hd.distance);

                        //Vector3 dir = (epos - spos);
                        float scl = hd.radius * 2;
                        Pool.Spawn(DebugSpherePrefab, spos, Quaternion.identity, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugSpherePrefab, epos, Quaternion.identity, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugCylinderPrefab, mpos, origin.rotation, new Vector3(scl, scl, hd.distance * .5f), duration);
                    }
                    break;

                case HitscanType.CapsuleCast:
                    {
                        Vector3 spos1 = origin.position + origin.TransformDirection(hd.offset1);
                        Vector3 spos2 = origin.position + origin.TransformDirection(hd.offset2);

                        var originFwd = origin.forward * hd.distance;
                        var halfOriginFwd = originFwd * .5f;

                        Vector3 mpos1 = spos1 + halfOriginFwd;
                        Vector3 mpos2 = spos2 + halfOriginFwd;


                        Vector3 epos1 = spos1 + originFwd;
                        Vector3 epos2 = spos2 + originFwd;

                        Vector3 mposStr = spos1 + (spos2 - spos1) * .5f;
                        Vector3 mposEnd = epos1 + (epos2 - epos1) * .5f;

                        float scl = hd.radius * 2;
                        Pool.Spawn(DebugSpherePrefab, spos1, Quaternion.identity, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugSpherePrefab, epos1, Quaternion.identity, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugSpherePrefab, spos2, Quaternion.identity, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugSpherePrefab, epos2, Quaternion.identity, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugCylinderPrefab, mpos1, origin.rotation, new Vector3(scl, scl, hd.distance * .5f), duration);
                        Pool.Spawn(DebugCylinderPrefab, mpos2, origin.rotation, new Vector3(scl, scl, hd.distance * .5f), duration);
                        float mag = Vector3.Magnitude(spos2 - spos1) * .5f;
                        Pool.Spawn(DebugCylinderPrefab, mposStr, Quaternion.LookRotation(spos2 - spos1, Vector3.up), new Vector3(scl, scl, mag), duration);
                        Pool.Spawn(DebugCylinderPrefab, mposEnd, Quaternion.LookRotation(epos2 - epos1, Vector3.up), new Vector3(scl, scl, mag), duration);

                    }
                    break;

                case HitscanType.OverlapSphere:
                    {
                        Vector3 pos = (hd.useOffset) ? origin.position + hd.offset1 : origin.position;
                        Quaternion rot = origin.rotation;
                        float scl = hd.radius * 2;
                        Pool.Spawn(DebugSpherePrefab, pos, rot, new Vector3(scl, scl, scl), duration);
                    }
                    break;

                case HitscanType.OverlapBox:
                    {
                        Vector3 pos = (hd.useOffset) ? origin.position + hd.offset1 : origin.position;
                        var rot = Quaternion.Euler(origin.eulerAngles + hd.orientation);
                        Vector3 scl = hd.halfExtents * 2;

                        Pool.Spawn(DebugCubePrefab, pos, rot, scl, duration);
                    }
                    break;

                case HitscanType.OverlapCapsule:
                    {
                        Quaternion rot = origin.rotation;
                        float scl = hd.radius * 2;
                        Vector3 pos1 = origin.TransformPoint(hd.offset1);
                        Vector3 pos2 = origin.TransformPoint(hd.offset2);
                        Vector3 dir = pos2 - pos1;
                        Vector3 mid = pos1 + dir * .5f;
                        Pool.Spawn(DebugSpherePrefab, pos1, rot, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugSpherePrefab, pos2, rot, new Vector3(scl, scl, scl), duration);
                        Pool.Spawn(DebugCylinderPrefab, mid, Quaternion.LookRotation(dir, Vector3.up), new Vector3(scl, scl, dir.magnitude * .5f), duration);
                    }
                    break;
            }
        }
        //#endif
        #endregion

        private static readonly Dictionary<int, int> reusableGameObjIntDict = new Dictionary<int, int>();

        public static int GenericHitscanNonAlloc(this HitscanDefinition hd, Transform origin, out RaycastHit[] rayhits, out Collider[] hits, ref int nearestIndex, bool showDebugWidgets = false, bool useSecondaryRealm = false)
        {
            hits = reusableColliderArray;
            rayhits = reusableRayHitArray;
            return GenericHitscanNonAlloc(hd, origin, ref reusableColliderArray, ref reusableRayHitArray, ref nearestIndex, showDebugWidgets, useSecondaryRealm);
        }

        public static int GenericHitscanNonAlloc(this HitscanDefinition hd, Transform origin, NetObject ownerNetObj, ref List<NetworkHit> hitscanHits, ref int nearestIndex, bool showDebugWidgets = false, bool useSecondaryRealm = false)
        {
            if (hitscanHits == null)
                hitscanHits = reusableHitscanHitList;

            int nearestRayHit = -1;
            nearestIndex = -1;
            int count = GenericHitscanNonAlloc(hd, origin, ref reusableColliderArray, ref reusableRayHitArray, ref nearestRayHit, showDebugWidgets, useSecondaryRealm);

            reusableGameObjIntDict.Clear();
            hitscanHits.Clear();

            if (count > 0)
            {
                /// Check each collider hit for its contactGroup and its rootgameobject/viewID - add to our outgoing List<HitscanHit>
                for (int i = 0; i < count; ++i)
                {
                    var hit = reusableColliderArray[i];
                    var netObjs = reusableNetObjectsList;

#if PUN_2_OR_NEWER

                    /// Collect all nested NetObjects - child to parent in order
                    hit.transform.GetNestedComponentsInParents(netObjs);
#endif

                    /// We are only interested in objects with IDs
                    int noCount = netObjs.Count;
                    if (noCount == 0)
                        continue;

                    /// Determine if any of the nested NetObjs is the owner.
                    bool selfHit = false;
                    for (int c = 0; c < noCount; ++c)
                    {
                        if (ReferenceEquals(netObjs[c], ownerNetObj))
                        {
                            selfHit = true;
                            break;
                        }
                    }


                    if (selfHit)
                        continue;

                    var netObj = netObjs[0];
                    var viewID = netObj.ViewID;

                    var contactGroupassign = hit.GetComponent<IContactGroupsAssign>();
                    var mask = (ReferenceEquals(contactGroupassign, null)) ? 0 : contactGroupassign.Mask;

                    int existingIndex;
                    bool exists = reusableGameObjIntDict.TryGetValue(viewID, out existingIndex);

                    int colliderId = netObj.colliderLookup[hit];

                    if (exists)
                    {
                        var hitscanHit = hitscanHits[existingIndex];
                        //Debug.Log("EXIST Hitscan hit on " + origin.name + "/" + hit.name + " m: " + mask + "->" + (hitscanHit.hitMask | mask));
                        hitscanHits[existingIndex] = new NetworkHit(hitscanHit.netObjId, hitscanHit.hitMask | mask, colliderId);

                        /// If we are adding the nearest raycast, log it.
                        if (i == nearestRayHit)
                            nearestIndex = existingIndex;
                    }
                    else
                    {
                        //Debug.Log("NEW Hitscan hit on " + rootGO.name + "/" + hit.name + " " + mask);

                        /// If we are adding the nearest raycast, log it.
                        if (i == nearestRayHit)
                            nearestIndex = hitscanHits.Count;

                        hitscanHits.Add(new NetworkHit(viewID, mask, colliderId));

                        reusableGameObjIntDict.Add(viewID, hitscanHits.Count - 1);
                    }
                }
            }
            return reusableGameObjIntDict.Count;
        }

        /// <summary>
        /// Trigger the defined Cast/Overlap.
        /// </summary>
        /// <param name="showDebugWidgets">Show 3d visuals for hitscans. Disabled in Release Builds.</param>
        /// <returns></returns>
        public static int GenericHitscanNonAlloc(this HitscanDefinition hd, Transform origin, ref Collider[] hits, ref RaycastHit[] rayhits, ref int nearestIndex, bool showDebugWidgets = false, bool useSecondaryRealm = false)
        {

            //#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            if (showDebugWidgets)
                VisualizeHitscan(hd, origin, 0.5f);
            //#endif

            if (ReferenceEquals(hits, null))
                hits = reusableColliderArray;

            if (ReferenceEquals(rayhits, null))
                rayhits = reusableRayHitArray;

            var hitscanType = hd.hitscanType;

            int hitcount;
            Vector3 srcPos = (hd.useOffset) ? origin.TransformPoint(hd.offset1) : origin.position;

#if !UNITY_2019_1_OR_NEWER

#if GHOST_WORLD
            LayerMask layerMask = (useSecondaryRealm) ?
                (LayerMask)(GhostWorld.GHOST_WORLD_LAYERMASK) :
                (LayerMask)(hd.layerMask & ~GhostWorld.GHOST_WORLD_LAYERMASK);
#else
            LayerMask layerMask = hd.layerMask;
#endif
            switch (hitscanType)
            {
                case HitscanType.Raycast:
                    hitcount = Physics.RaycastNonAlloc(/*srcPos*/ origin.position, origin.forward, rayhits, hd.distance, layerMask);
                    break;

                case HitscanType.SphereCast:
                    hitcount = Physics.SphereCastNonAlloc(new Ray(srcPos, origin.forward), hd.radius, rayhits, hd.distance, layerMask);
                    break;

                case HitscanType.BoxCast:
                    hitcount = Physics.BoxCastNonAlloc(srcPos, hd.halfExtents, origin.forward, rayhits, Quaternion.Euler(origin.eulerAngles + hd.orientation), hd.distance, layerMask);
                    break;

                case HitscanType.CapsuleCast:
                    hitcount = Physics.CapsuleCastNonAlloc(origin.TransformPoint(hd.offset1), origin.TransformPoint(hd.offset2), hd.radius, origin.forward, rayhits, hd.distance, layerMask);
                    break;

                case HitscanType.OverlapSphere:
                    hitcount = Physics.OverlapSphereNonAlloc(srcPos, hd.radius, hits, layerMask);
                    break;

                case HitscanType.OverlapBox:
                    hitcount = Physics.OverlapBoxNonAlloc(srcPos, hd.halfExtents, hits, Quaternion.Euler(origin.eulerAngles + hd.orientation), layerMask);
                    break;

                case HitscanType.OverlapCapsule:
                    hitcount = Physics.OverlapCapsuleNonAlloc(origin.TransformPoint(hd.offset1), origin.TransformPoint(hd.offset2), hd.radius, hits, layerMask);
                    break;

                default:
                    hitcount = 0;
                    break;
            }
#else
    
#if GHOST_WORLD
            var phys = (realm == Realm.Primary) ? Physics.defaultPhysicsScene : GhostWorld.ghostPhysics;
#else
            var phys = Physics.defaultPhysicsScene;
#endif
			LayerMask layerMask = hd.layerMask;

			switch (hitscanType)
			{
				case HitscanType.Raycast:
					hitcount = phys.Raycast(srcPos, origin.forward, rayhits, hd.distance, layerMask);
					break;

				case HitscanType.SphereCast:
					hitcount = phys.SphereCast(srcPos, hd.radius, origin.forward, rayhits, hd.distance, layerMask);
					break;

				case HitscanType.BoxCast:
					hitcount = phys.BoxCast(srcPos, hd.halfExtents, origin.forward, rayhits, Quaternion.Euler(origin.eulerAngles + hd.orientation), hd.distance, layerMask);
					break;

				case HitscanType.CapsuleCast:
					hitcount = phys.CapsuleCast(origin.TransformPoint(hd.offset1), origin.TransformPoint(hd.offset2), hd.radius, origin.forward, rayhits, hd.distance, layerMask);
					break;

				case HitscanType.OverlapSphere:
					hitcount = phys.OverlapSphere(srcPos, hd.radius, hits, layerMask, QueryTriggerInteraction.UseGlobal);
					break;

				case HitscanType.OverlapBox:
					hitcount = phys.OverlapBox(srcPos, hd.halfExtents, hits, Quaternion.Euler(origin.eulerAngles + hd.orientation), layerMask);
					break;

				case HitscanType.OverlapCapsule:
					hitcount = phys.OverlapCapsule(origin.TransformPoint(hd.offset1), origin.TransformPoint(hd.offset2), hd.radius, hits, layerMask);
					break;

				default:
					hitcount = 0;
					break;
			}
#endif
            nearestIndex = -1;

            if (hitcount == 0)
                return hitcount;

            bool nearestOnly = hd.nearestOnly;
            float distToNearest = float.PositiveInfinity;


            /// Overlaps have no rayhit info, so we are done.
            if (hitscanType.IsOverlap())
            {
                nearestIndex = -1;
                return hitcount;
            }
            ///Cast Nearest Only
            if (nearestOnly)
            {
                for (int i = 0; i < hitcount; i++)
                {
                    RaycastHit rayhit = rayhits[i];

                    /// Get Nearest
                    float dist = rayhit.distance;

                    if (dist < distToNearest)
                    {
                        distToNearest = dist;
                        nearestIndex = i;
                    }
                }

                hits[0] = rayhits[nearestIndex].collider;
                return 1;
            }
            /// Cast All
            // Convert the raycasthits to colliders[] if this was a cast and not an overlap
            else /*if (hitscanType.IsCast())*/
            {
                for (int i = 0; i < hitcount; i++)
                {
                    RaycastHit rayhit = rayhits[i];

                    /// Get Nearest
                    float dist = rayhit.distance;

                    if (dist < distToNearest)
                    {
                        distToNearest = dist;
                        nearestIndex = i;
                    }

                    if (!nearestOnly)
                        hits[i] = rayhits[i].collider;
                }
                if (nearestOnly)
                {
                    hits[0] = rayhits[nearestIndex].collider;
                    return 1;
                }
                return hitcount;
            }

        }

        [System.Obsolete("Haven't reworked this for new physx yet.")]
        public static int GenericCastNonAlloc(this Transform srcT, Collider[] hits, RaycastHit[] rayhits, float distance, float radius, int mask, Quaternion orientation, bool useOffset, Vector3 offset1, Vector3 offset2, HitscanType hitscanType)
        {
            int hitcount;
            Vector3 srcPos = (useOffset) ? (srcT.position + srcT.TransformDirection(offset1)) : srcT.position;

            switch (hitscanType)
            {
                case HitscanType.Raycast:
                    hitcount = Physics.RaycastNonAlloc(new Ray(srcPos, srcT.forward), rayhits, distance, mask);
                    break;

                case HitscanType.SphereCast:
                    hitcount = Physics.SphereCastNonAlloc(new Ray(srcPos, srcT.forward), radius, rayhits, distance, mask);
                    break;

                case HitscanType.BoxCast:
                    hitcount = Physics.BoxCastNonAlloc(srcPos, offset2, srcT.forward, rayhits, orientation, distance, mask);
                    break;

                case HitscanType.CapsuleCast:
                    hitcount = Physics.CapsuleCastNonAlloc(srcT.TransformPoint(offset1), srcT.TransformPoint(offset2), radius, srcT.forward, rayhits, distance, mask);
                    break;

                case HitscanType.OverlapSphere:
                    hitcount = Physics.OverlapSphereNonAlloc(srcPos, radius, hits, mask);
                    break;

                case HitscanType.OverlapBox:
                    hitcount = Physics.OverlapBoxNonAlloc(srcPos, offset2, hits, orientation, mask);
                    break;

                case HitscanType.OverlapCapsule:
                    hitcount = Physics.OverlapCapsuleNonAlloc(srcT.TransformPoint(offset1), srcT.TransformPoint(offset2), radius, hits, mask);
                    break;

                default:
                    hitcount = 0;
                    break;
            }

            // Convert the raycasthits to colliders[] if this was a cast and not an overlap
            if (hitscanType.IsCast())
                for (int i = 0; i < hitcount; i++)
                    hits[i] = rayhits[i].collider;

            return hitcount;
        }
    }
}




