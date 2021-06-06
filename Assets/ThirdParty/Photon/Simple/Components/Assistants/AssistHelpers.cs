// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Photon.Pun.Simple.Assists
{
	public enum Dynamics { Static, Variable, Dynamic }
	public enum Space_XD { SPACE_3D, SPACE_2D }
	public enum SystemPresence { Absent, Incomplete, Partial, Nested, Complete }
    public enum Preset
    {
        Defaults = 0,
        HealthPickup,
        RechargeZone,
        DamageZone,
        WeaponMelee,
        WeaponCannon,
        WeaponScan,
        ContactScan,
        VitalsScan,
        InventorysScan,
        Poke = 1 << 29,
        Grab = 1 << 30,
        Damaging = 1 << 31,
    }

    public enum AssistColor { Black, Red, Green, Blue, Cyan, Yellow, Magenta, Orange, LiteGray, DarkGray }

    public delegate SystemPresence GetSystemPresenceDelegate(GameObject selection, params MonoBehaviour[] dependencies);
	public delegate void SystemAddDelegate(GameObject selection, bool add, params MonoBehaviour[] dependencies);

	public static class AssistHelpers
	{
        public const int PRIORITY = -100;
        public const int CONVERTTO_PRIORITY = -50;
        public const int TUTORIAL_PRIORITY = 100;
        public const string SIMPLE_MENU_ROOT = "GameObject/Simple/";
		public const string TUTORIAL_FOLDER = SIMPLE_MENU_ROOT + "Tutorial/";
		public const string CONVERT_TO_FOLDER = SIMPLE_MENU_ROOT + "Convert To/";
		public const string ADD_TO_OBJ_FOLDER = SIMPLE_MENU_ROOT + "Add To Object/";
		public const string ADD_TO_SCENE_TXT = SIMPLE_MENU_ROOT + "Add To Scene/";

        // Constructor
        static AssistHelpers()
        {
            PopulateObjStateArrays();
        }

		public static GameObject GetSelectedGameObject()
		{
			GameObject selection = Selection.activeGameObject;
			if (!selection)
				Debug.LogWarning("No gameobject selected, cannot Assist aborted.");

			return selection;
		}

		public static GameObject CreateEmptyChildGameObject(this Transform t, string name)
		{
			var go = new GameObject(name);
			go.transform.parent = t;
			go.transform.localPosition = new Vector3(0, 0, 0);
			go.transform.localRotation = new Quaternion(0, 0, 0, 1);
			go.transform.localScale = new Vector3(1, 1, 1);
			return go;
		}

		public static T EnsureComponentExists<T>(this GameObject go, bool checkParents = false) where T : MonoBehaviour
		{
			if (go == null)
				return null;


			T found = checkParents ? go.GetComponentInParent<T>() : go.GetComponent<T>();
			if (found)
				return found;

			return go.AddComponent<T>();
		}

		public static void EnsureComponentOnNestedChildren<T>(this GameObject go, bool allowMultiples, bool recurse = false) where T : Component
		{
			for (int i = 0; i < go.transform.childCount; ++i)
			{
				var child = go.transform.GetChild(i);

				/// Don't touch nests
				if (child.GetComponent<NetObject>())
					continue;

				/// Recurse if applicable
				if (recurse & child.childCount > 0)
					EnsureComponentOnNestedChildren<T>(child.gameObject, allowMultiples, recurse);

				if (!allowMultiples)
					if (child.GetComponent<T>())
						continue;

				child.gameObject.AddComponent<T>();
			}
		}

		public static List<Component> reusableComponents = new List<Component>();
		public static void DestroyComponentOnNestedChildren<T>(this GameObject go, bool recurse = false) where T : Component
		{
			for (int i = 0; i < go.transform.childCount; ++i)
			{
				var child = go.transform.GetChild(i);

				if (child.GetComponent<NetObject>())
					continue;

				/// Recurse if applicable
				if (recurse & child.childCount > 0)
					EnsureComponentOnNestedChildren<T>(child.gameObject, recurse);

				child.GetComponents(reusableComponents);
				for (int c = reusableComponents.Count - 1; c >= 0; --c)
					if (reusableComponents[c] is T)
						Object.DestroyImmediate(reusableComponents[c]);
			}
		}

		public static Component AddRigidbody(this GameObject go, Space_XD space)
		{
			if (space == Space_XD.SPACE_3D)
			{
				var rb = go.GetComponent<Rigidbody>();
				if (!rb)
					rb = go.AddComponent<Rigidbody>();

				rb.isKinematic = true;
				rb.interpolation = RigidbodyInterpolation.Interpolate;
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

				return rb;
			}
			else
			{
				var rb = go.GetComponent<Rigidbody2D>();
				if (!rb)
					rb = go.AddComponent<Rigidbody2D>();

				rb.isKinematic = true;
				rb.interpolation = RigidbodyInterpolation2D.Interpolate;
				rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				return rb;
			}
		}
		
		public enum ColliderType { None, Trigger, Full };

		public static GameObject CreateNewPrimitiveAsChild(this GameObject go, PrimitiveType primitiveType, ColliderType coltype, 
            string name, float scale = 1, AssistColor color = AssistColor.Magenta, params System.Type[] components)
		{
			var prim = GameObject.CreatePrimitive(primitiveType);

			if (primitiveType == PrimitiveType.Sphere)
				prim.transform.localScale = new Vector3(.5f, .5f, .5f) * scale;
			else if (primitiveType == PrimitiveType.Cube)
				prim.transform.localScale = new Vector3(.35f, .35f, .35f) * scale;
			else if (primitiveType == PrimitiveType.Capsule)
				prim.transform.localScale = new Vector3(.3f, .3f, .3f) * scale;
			else if (primitiveType == PrimitiveType.Cylinder)
				prim.transform.localScale = new Vector3(.51f, .1f, .51f) * scale;

			prim.transform.parent = go.transform;
			prim.transform.localPosition = new Vector3(0, 0, 0);
			prim.transform.localRotation = new Quaternion(0, 0, 0, 1);
			prim.name = name;

			switch (coltype)
			{
				case ColliderType.None:
					{
						Object.DestroyImmediate(prim.GetComponent<Collider>());
						break;
					}

				case ColliderType.Trigger:
					{
						prim.GetComponent<Collider>().isTrigger = true;
						break;
					}
				case ColliderType.Full:
					{
						break;
					}
			}

			foreach(var c in components)
			{
				prim.AddComponent(c);
			}

            prim.GetComponent<Renderer>().material = GetMaterial(color);

			return prim;
		}

		public static List<string> itemStateEnumNames = new List<string>();
		public static List<ObjStateEditor> itemStateEnumValues = new List<ObjStateEditor>();

        private static void PopulateObjStateArrays()
        {
            var enumnames = System.Enum.GetNames(typeof(ObjStateEditor));
            var enumvals = (ObjStateEditor[])System.Enum.GetValues(typeof(ObjStateEditor));
            itemStateEnumNames.AddRange(enumnames);
            itemStateEnumValues.AddRange(enumvals);

            itemStateEnumNames.Add("Unmounted");
            itemStateEnumValues.Add((ObjStateEditor)(-1));
        }

  //      public static void CreateChildStatePlaceholders(this GameObject go, Space_XD space, Dynamics dynamics, float scale = 1)
		//{
		//	/// Create a placeholder for each State enum value
		//	for (int i = 0; i < itemStateEnumValues.Count; ++i)
		//	{
		//		go.AddStatePlaceholder(i, space, dynamics, scale);
		//	}
		//}

        public static void CreateChildStatePlaceholders(this GameObject go, Space_XD space, Dynamics dynamics, float scale = 1)
        {
            
            go.AddStatePlaceholder("Despawned", MaskLogic.Operator.EQUALS, 0, 0, ColliderType.None, PrimitiveType.Capsule, AssistColor.Black, space, dynamics, scale);
            go.AddStatePlaceholder("Visible", MaskLogic.Operator.AND, ObjState.Visible, 0, ColliderType.None, PrimitiveType.Sphere, AssistColor.Cyan, space, dynamics, scale);
            go.AddStatePlaceholder("Mounted", MaskLogic.Operator.AND, ObjState.Visible | ObjState.Mounted, 0, ColliderType.None, PrimitiveType.Cube, AssistColor.Magenta, space, dynamics, scale);
            go.AddStatePlaceholder("Unmounted", MaskLogic.Operator.AND, ObjState.Visible | ObjState.Anchored, ObjState.Anchored, ColliderType.Trigger, PrimitiveType.Cylinder, AssistColor.Red, space, dynamics, scale);
            go.AddStatePlaceholder("Attached", MaskLogic.Operator.AND, ObjState.Visible | ObjState.Anchored, 0, ColliderType.None, PrimitiveType.Capsule, AssistColor.Green, space, dynamics, scale);
            go.AddStatePlaceholder("Transit", MaskLogic.Operator.AND, ObjState.Visible | ObjState.Transit, 0, ColliderType.Full, PrimitiveType.Cube, AssistColor.Yellow, space, dynamics, scale)
                .transform.rotation = Quaternion.Euler(45, 45, 45);
        }

        public static Material blackMat, cyanMat, magentaMat, greenMat, redMat, yellowMat, blueMat, orangeMat, liteGrayMat, darkGrayMat;

        
        private static Material GetMaterial(AssistColor color)
        {
            const string path = "Assets/Photon/Simple/Example/Materials/Simple";

            switch (color)
            {
               
                case AssistColor.Red:
                    if (redMat == null)
                        redMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Red.mat") as Material;
                    return redMat;

                case AssistColor.Green:
                    if (greenMat == null)
                        greenMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Green.mat") as Material;
                    return greenMat;

                case AssistColor.Blue:
                    if (blueMat == null)
                        blueMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Blue.mat") as Material;
                    return greenMat;

                case AssistColor.Cyan:
                    if (cyanMat == null)
                        cyanMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Cyan.mat") as Material;
                    return cyanMat;

                case AssistColor.Yellow:
                    if (yellowMat)
                        yellowMat= AssetDatabase.LoadAssetAtPath<Material>(path + "Yellow.mat") as Material;
                    return yellowMat;

                case AssistColor.Magenta:
                    if (magentaMat == null)
                        magentaMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Magenta.mat") as Material;
                    return magentaMat;

                case AssistColor.Black:
                    if (blackMat == null)
                        blackMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Black.mat") as Material;
                    return blackMat;

                case AssistColor.Orange:
                    if (orangeMat == null)
                        orangeMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Orange.mat") as Material;
                    return orangeMat;

                case AssistColor.LiteGray:
                    if (liteGrayMat == null)
                        liteGrayMat = AssetDatabase.LoadAssetAtPath<Material>(path + "LiteGray.mat") as Material;
                    return liteGrayMat;

                case AssistColor.DarkGray:
                    if (darkGrayMat == null)
                        darkGrayMat = AssetDatabase.LoadAssetAtPath<Material>(path + "DarkGray.mat") as Material;
                    return darkGrayMat;

                default:
                    if (magentaMat == null)
                        magentaMat = AssetDatabase.LoadAssetAtPath<Material>(path + "Magenta.mat") as Material;
                    return magentaMat;
            }
        }


        private static GameObject AddStatePlaceholder(this GameObject go, string name, 
            ObjStateLogic.Operator op, ObjState mask, ObjState notmask, ColliderType colliderType, PrimitiveType primitiveType,
            AssistColor color, Space_XD space, Dynamics dynamics, float scale = 1)
        {
            string label = name + " Model";

            var visObj = new GameObject(label);
            visObj.transform.parent = go.transform;
            var toggle = visObj.AddComponent<OnStateChangeToggle>();
           
            toggle.stateLogic.notMask = (int)notmask;
            toggle.stateLogic.operation = op;
            toggle.stateLogic.stateMask = (int)mask;

            visObj.CreateNewPrimitiveAsChild(primitiveType, colliderType, label + " Placeholder", scale, color);
            return visObj;
        }

	}

}

#endif