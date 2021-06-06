//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Photon.Pun.Simple
//{
//	/// <summary>
//	/// Variations on GetComponent that are aware of NetObject nesting. If a NetObject is nexted, searches should not leave that nesting
//	/// or else they will start finding components that aren't actually part of the NetObject, which is not desireable in most cases.
//	/// </summary>
//	public static class GetNestedComponentUtils
//	{

//		public static Dictionary<System.Type, ICollection> reusables = new Dictionary<System.Type, ICollection>();
//		public static Dictionary<System.Type, ICollection> tempLists = new Dictionary<System.Type, ICollection>();

//		private static List<Transform> reusableTransformList = new List<Transform>();
//		private static List<Transform> reusableTransformList2 = new List<Transform>();
//		private static List<NetObject> reusableNetObjects = new List<NetObject>();

//		/// <summary>
//		/// Returns all NetObjects found between the child transform and its root. Order in List from child to parent, with the root most being last.
//		/// </summary>
//		/// <param name="t"></param>
//		/// <returns></returns>
//		public static List<NetObject> GetNestedNetObjectsInParents(this Transform t)
//		{
//			reusableNetObjects.Clear();

//			while (t != null)
//			{
//				NetObject no = t.GetComponent<NetObject>();
//				if (no)
//					reusableNetObjects.Add(no);

//				t = t.parent;
//			}
//			return reusableNetObjects;
//		}


//		public static NetObject GetParentNetObject(this Transform t)
//		{
//			NetObject found = t.GetComponent<NetObject>();

//			if (found)
//				return found;

//			var par = t.parent;
//			while (par)
//			{
//				found = par.GetComponent<NetObject>();
//				if (found)
//					return found;
//				par = par.parent;
//			}
//			return null;
//		}

//		public static T GetNestedComponentInParents<T>(this Transform t) where T : class
//		{
//            // First try root
//            var found = t.GetComponent<T>();

//			if (!ReferenceEquals(found, null))
//				return found;

//			/// Get the reverse list of transforms climing for start up to netobject
//			var par = t.parent;

//			while (!ReferenceEquals(par, null))
//			{
				
//				found = par.GetComponent<T>();
//				if (!ReferenceEquals(found, null))
//					return found;

//				/// Stop climbing at the NetObj (this is how we detect nesting
//				if (!ReferenceEquals(par.GetComponent<NetObject>() , null))
//                    return null;

//                par = par.parent;
//			};

//			return null;
//		}


//		//public static List<T> GetNestedComponentsInParents<T>(this Transform t, List<T> list)
//		//{
//		//	list.Clear();
//		//	reusableTransformList.Clear();

//		//	reusableTransformList.Add(t);
//		//	/// Get the reverse list of transforms climing for start up to netobject
//		//	var par = t.parent;

//		//	while (true)
//		//	{
//		//		if (ReferenceEquals(par, null))
//		//			break;

//		//		reusableTransformList.Add(par);

//		//		/// Stop climbing at the NetObj (this is how we detect nesting
//		//		if (par.GetComponent<NetObject>() != null)
//		//			break;

//		//		par = par.parent;
//		//	};

//		//	System.Type type = typeof(T);

//		//	if (!tempLists.ContainsKey(type))
//		//		tempLists.Add(type, new List<T>());

//		//	List<T> temp = tempLists[type] as List<T>;
//		//	temp.Clear();

//		//	/// Reverse iterate the transform list and add components found. This produces a GetComponentInParent that stops at the NetObj
//		//	for (int i = reusableTransformList.Count - 1; i >= 0; --i)
//		//	{
//		//		reusableTransformList[i].AddComponentsFromObject(list, temp);
//		//	}
//		//	return list;

//		//}

//		public static T GetNestedComponentInChildren<T>(this Transform t) where T : Component
//		{
//			/// Look for the most obvious check first on the root.
//			var found = t.GetComponent<T>();
//			if (found)
//				return found;

//			/// No root found, start testing layer by layer
//			reusableTransformList.Clear();

//			int ccnt = t.childCount;
//			for (int c = 0; c < ccnt; ++c)
//			{
//				var child = t.GetChild(c);

//				/// Hit a nested NetObject - don't search this node
//				if (child.GetComponent<NetObject>())
//					continue;

//				found = child.GetComponent<T>();

//				if (found)
//					return found;

//				reusableTransformList.Add(child);
//			}

//			found = RecurseLayer<T>(reusableTransformList, reusableTransformList2);
//			return found;
//		}


//		private static T RecurseLayer<T>(List<Transform> currList, List<Transform> nextList) where T : Component
//		{

//			nextList.Clear();

//			/// Iterate each transform in our current depth list
//			int count = currList.Count;
//			for (int i = 0; i < count; ++i)
//			{
//				var t = currList[i];

//				/// Check each child for end of nesting, seeked component, and if not found add to next layer list
//				int ccnt = t.childCount;
//				for (int c = 0; c < ccnt; ++c)
//				{
//					var child = t.GetChild(c);

//					if (child.GetComponent<NetObject>())
//						continue;

//					var foundchild = child.GetComponent<T>();

//					/// If we found the component, we are done
//					if (foundchild)
//						return foundchild;

//					nextList.Add(child);
//				}
//			}

//			/// If nothing was found and there are no more layers left, return null
//			if (nextList.Count == 0)
//				return null;
//			/// Else more layers were found, go the next layer deep.
//			else
//				/// Swap next and curr lists for the next layer
//				return RecurseLayer<T>(nextList, currList);
//		}


//		///// <summary>
//		///// Same as GetComponentsInChildren, but will not recurse into children with their own NetObject. This allows nesting of NetObjects to be respected.
//		///// </summary>
//		///// <typeparam name="T"></typeparam>
//		///// <param name="t"></param>
//		///// <param name="list">Pass null and a reused list will be used. Consume immediately.</param>
//		//public static List<T> GetNestedComponentsInChildren<T>(this Transform t, List<T> list = null)
//		//{
//		//	System.Type type = typeof(T);

//		//	if (ReferenceEquals(list, null))
//		//	{
//		//		if (!reusables.ContainsKey(type))
//		//		{
//		//			list = new List<T>();
//		//			reusables.Add(type, list);
//		//		}
//		//		else
//		//			list = reusables[type] as List<T>;
//		//	}

//		//	List<T> temp;
//		//	if (!tempLists.ContainsKey(type))
//		//	{
//		//		temp = new List<T>();
//		//		tempLists.Add(type, temp);
//		//	}
//		//	else
//		//		temp = tempLists[type] as List<T>;

//		//	temp.Clear();
//		//	list.Clear();

//		//	t.AddComponentsFromObject(list, temp);
//		//	t.RecurseAddChildren(list, temp);

//		//	return list;
//		//}

//		/// <summary>
//		/// Same as GetComponentsInChildren, but will not recurse into children with their own NetObject. This allows nesting of NetObjects to be respected.
//		/// </summary>
//		/// <typeparam name="T"></typeparam>
//		/// <param name="t"></param>
//		/// <param name="list">Pass null and a reused list will be used. Consume immediately.</param>
//		public static List<Component> GetNestedComponentsInChildren(this Transform t, System.Type type, List<Component> list)
//		{
//			System.Type compType = typeof(Component);

//			if (ReferenceEquals(list, null))
//			{
//				if (!reusables.ContainsKey(compType))
//				{
//					list = new List<Component>();
//					reusables.Add(compType, list);
//				}
//				else
//					list = reusables[compType] as List<Component>;
//			}

//			List<Component> temp;

//			if (!tempLists.ContainsKey(type))
//			{
//				temp = new List<Component>();
//				tempLists.Add(type, temp);
//			}
//			else
//				temp = tempLists[type] as List<Component>;

//			temp.Clear();
//			list.Clear();

//			t.AddComponentsFromObject(list, temp, type);
//			t.RecurseAddChildren(list, temp, type);

//			return list;
//		}

//		private static void RecurseAddChildren<T>(this Transform t, List<T> list, List<T> temp)
//		{
//			int count = t.childCount;
//			for (int i = 0; i < count; ++i)
//			{
//				var child = t.GetChild(i);

//				/// Don't dig into branches that have nested NetObject
//				if (child.GetComponent<NetObject>())
//					continue;

//				child.AddComponentsFromObject(list, temp);
//				child.RecurseAddChildren(list, temp);
//			}
//		}

//		private static void RecurseAddChildren(this Transform t, List<Component> list, List<Component> temp, System.Type type)
//		{
//			int count = t.childCount;
//			for (int i = 0; i < count; ++i)
//			{
//				var child = t.GetChild(i);

//				/// Don't dig into branches that have nested NetObject
//				if (child.GetComponent<NetObject>())
//					continue;

//				child.AddComponentsFromObject(list, temp, type);
//				child.RecurseAddChildren(list, temp, type);
//			}
//		}

//		private static void AddComponentsFromObject<T>(this Transform t, List<T> list, List<T> temp)
//		{
//			temp.Clear();

//			t.GetComponents(temp);
//			list.AddRange(temp);
//		}

//		private static void AddComponentsFromObject(this Transform t, List<Component> list, List<Component> temp, System.Type type)
//		{
//			temp.Clear();

//			t.GetComponents(temp);
//			foreach (var comp in temp)
//				if (comp.GetType().IsAssignableFrom(type))
//					list.Add(comp);
//		}

//		/// <summary>
//		/// Finds first occurance of a component climbing toward the transforms root, including disabled objects.
//		public static T GetComponentInParentEvenIfDisabled<T>(this Transform t) where T : Component
//		{
//			T co = t.GetComponent<T>();

//			if (!ReferenceEquals(co, null))
//				return co;

//			co = t.GetComponentInParent<T>();

//			if (!ReferenceEquals(co, null))
//				return co;

//			var exclude = t.GetComponentsInChildren<T>(true);
//			var excludedCount = exclude.Length;

//			Transform par = t.parent;
//			while (par)
//			{
//				/// Recurse upward until a parent finds something new in a children search
//				var found = par.GetComponentsInChildren<T>(true);
//				if (found.Length > exclude.Length)
//					return found[0];

//				par = par.parent;
//			}

//			return null;
//		}

//	}
//}

