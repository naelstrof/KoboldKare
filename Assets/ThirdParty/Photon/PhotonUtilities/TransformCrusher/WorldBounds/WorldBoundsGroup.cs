// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using System.Text;
using emotitron.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{
	[System.Serializable]
	public class WorldBoundsGroup
	{
		public const string defaultName = "Default";
		public const string newAddName = "Unnamed";

		public string name = defaultName;

		[System.NonSerialized]
		public System.Action OnWorldBoundChanged;

		//[System.NonSerialized]
		public /*readonly*/ ElementCrusher crusher = GetUncompressedCrusher();
		//public int resolution = 100;
		//public bool accurateCenter = false;

		[System.NonSerialized]
		public readonly List<WorldBounds> activeWorldBounds = new List<WorldBounds>();
		//public void Add(WorldBounds wb) { activeMapBoundsObjects.Add(wb);  }
		//public void Remove(WorldBounds wb) { activeMapBoundsObjects.Remove(wb); }

		public int ActiveBoundsObjCount { get { return activeWorldBounds.Count; } }

		[System.NonSerialized]
		public Bounds _combinedWorldBounds;

		public static ElementCrusher GetUncompressedCrusher()
		{
			return new ElementCrusher(TRSType.Position, false)
			{
				enableLocalSelector = false,
				hideFieldName = true,
				XCrusher = new FloatCrusher() { axis = Axis.X, outOfBoundsHandling = OutOfBoundsHandling.Clamp, Resolution = 100, BitsDeterminedBy = BitsDeterminedBy.Uncompressed },
				YCrusher = new FloatCrusher() { axis = Axis.Y, outOfBoundsHandling = OutOfBoundsHandling.Clamp, Resolution = 100, BitsDeterminedBy = BitsDeterminedBy.Uncompressed },
				ZCrusher = new FloatCrusher() { axis = Axis.Z, outOfBoundsHandling = OutOfBoundsHandling.Clamp, Resolution = 100, BitsDeterminedBy = BitsDeterminedBy.Uncompressed }
			};
		}

		public void ResetActiveBounds()
		{
			activeWorldBounds.Clear();
		}

		/// <summary>
		/// Whenever an instance of WorldBoundss gets removed, the combinedWorldBounds needs to be rebuilt with this.
		/// </summary>
		public void RecalculateWorldCombinedBounds()
		{
			var xc = crusher.XCrusher;
			var yc = crusher.YCrusher;
			var zc = crusher.ZCrusher;

			if (activeWorldBounds.Count == 0)
			{
				_combinedWorldBounds = new Bounds();

				// When we have no bounds for a group, default to uncompressed to ensure "always works"
				//xc.BitsDeterminedBy = BitsDeterminedBy.Uncompressed;
				//yc.BitsDeterminedBy = BitsDeterminedBy.Uncompressed;
				//zcs.BitsDeterminedBy = BitsDeterminedBy.Uncompressed;
			}
			else
			{

				/// When we have bounds for a group, switch back to Resolution mode if we are in a mode hostile to trying to set things
				if (xc.BitsDeterminedBy > 0 ||
					xc.BitsDeterminedBy == BitsDeterminedBy.SetBits || xc.BitsDeterminedBy == BitsDeterminedBy.Uncompressed || xc.BitsDeterminedBy == BitsDeterminedBy.HalfFloat)
				{
					xc.Resolution = 100;
					xc.BitsDeterminedBy = BitsDeterminedBy.Resolution;
				}
				if (yc.BitsDeterminedBy > 0 ||
					yc.BitsDeterminedBy == BitsDeterminedBy.SetBits || yc.BitsDeterminedBy == BitsDeterminedBy.Uncompressed || yc.BitsDeterminedBy == BitsDeterminedBy.HalfFloat)
				{
					yc.Resolution = 100;
					yc.BitsDeterminedBy = BitsDeterminedBy.Resolution;
				}
				if (zc.BitsDeterminedBy > 0 ||
					zc.BitsDeterminedBy == BitsDeterminedBy.SetBits || zc.BitsDeterminedBy == BitsDeterminedBy.Uncompressed || zc.BitsDeterminedBy == BitsDeterminedBy.HalfFloat)
				{
					zc.Resolution = 100;
					zc.BitsDeterminedBy = BitsDeterminedBy.Resolution;
				}

				// must have a starting bounds to encapsulate, otherwise it starts encapsulating a 0,0,0 center 
				// which may not be desired if the group doesn't encapsulate 0,0,0
				_combinedWorldBounds = activeWorldBounds[0].myBounds;
				for (int i = 1; i < activeWorldBounds.Count; i++)
				{
					_combinedWorldBounds.Encapsulate(activeWorldBounds[i].myBounds);
				}

				//xc.AccurateCenter = accurateCenter;
				//yc.AccurateCenter = accurateCenter;
				//zc.AccurateCenter = accurateCenter;

				//xc.Resolution = (ulong)resolution;
				//yc.Resolution = (ulong)resolution;
				//zc.Resolution = (ulong)resolution;
				crusher.Bounds = _combinedWorldBounds;
			}

			//if(Application.isPlaying)
			if (OnWorldBoundChanged != null)
			{
				OnWorldBoundChanged();
			}

		}
		//public void UpdateWorldBounds(bool mute = false)
		//{
		//	// No log messages if commanded, if just starting up, or just shutting down.
		//	WorldBoundsSO.SetWorldRanges(0, _combinedWorldBounds, muteMessages || mute);
		//}

#if UNITY_EDITOR

		public StringBuilder strb = new StringBuilder(); // .editorReusable;
		public const int BoundsReportHeight = 3 * 12 + 4;
		public string BoundsReport()
		{
			strb.Length = 0;
			strb.Append("Encapsulates [").Append(ActiveBoundsObjCount).Append("] ").Append(typeof(WorldBounds).Name)
				.Append("\nCombined Center: ").Append(_combinedWorldBounds.center)
				.Append("\nCombined Size: ").Append(_combinedWorldBounds.size);
			//.Append("\nBits: ").Append(crusher.TallyBits())
			//.Append(" (").Append(crusher.XCrusher.GetBits())
			//.Append(", ").Append(crusher.YCrusher.GetBits())
			//.Append(", ").Append(crusher.ZCrusher.GetBits())
			//.Append(")\nActual Res: ")
			//.Append(crusher.XCrusher.GetResAtBits()).Append(", ")
			//.Append(crusher.YCrusher.GetResAtBits()).Append(", ")
			//.Append(crusher.ZCrusher.GetResAtBits())
			//.Append("\nActual Prec: ")
			//.Append(crusher.XCrusher.GetPrecAtBits().ToString("N5")).Append(", ")
			//.Append(crusher.YCrusher.GetPrecAtBits().ToString("N5")).Append(", ")
			//.Append(crusher.ZCrusher.GetPrecAtBits().ToString("N5"));

			return strb.ToString();
		}
#endif
	}


#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(WorldBoundsGroup))]
	public class WorldBoundsSettingsDrawer : PropertyDrawer
	{
		//static GUIContent gngc = new GUIContent("", "(Editor only name), used in selection popup lists to identify each group.");

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool haschanged = false;

			float lw = EditorGUIUtility.labelWidth;

			Rect r = position;
			//r.xMax = lw - 4;
			r.height = 16;
			//r.xMax -= 18;

			var name = property.FindPropertyRelative("name");

			EditorGUI.BeginDisabledGroup(name.stringValue == WorldBoundsGroup.defaultName);
			string n = EditorGUI.DelayedTextField(r, /*gngc,*/ name.stringValue);
			EditorGUI.EndDisabledGroup();

			if (n != name.stringValue)
			{
				Undo.RecordObject(property.serializedObject.targetObject, "Undo World Bounds Group name change.");
				haschanged = true;
				name.stringValue = n;

				property.serializedObject.ApplyModifiedProperties();
				WorldBoundsSettings.EnsureNamesAreUnique();
				property.serializedObject.Update();
			}

			r.y += 18;
			r.xMax += 18;

			if (haschanged)
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
				//AssetDatabase.SaveAssets();
			}

		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label); /*+ 18 + 16 + 1*/;
		}
	}

#endif
}
