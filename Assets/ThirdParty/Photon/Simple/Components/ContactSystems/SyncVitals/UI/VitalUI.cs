// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
#if UNITY_EDITOR
	[CanEditMultipleObjects]
#endif
	public class VitalUI : VitalUIBase
	{
		protected static GameObject vitalBarDefaultPrefab;

		public bool autoOffset = true;
		[Tooltip("Found children elements are nudged (value * vitalIndex). This is to automatically stagger multiple VitalUIs")]
		public Vector3 offset = new Vector3(0, .1f, 0);

		public float widthMultiplier = 1f;

		//[Tooltip("If a Canvas is supplied in the inspector, this will attempt to find the first Text / Image in its children and use that.")]
		//[HideInInspector] public Canvas canvas;
		[Tooltip("Search for UI elements in children of this GameObject.")]
		[HideInInspector] public bool searchChildren = true;
		[HideInInspector] public Text UIText;
		[HideInInspector] public Image UIImage;
		[HideInInspector] public TextMesh textMesh;
		[HideInInspector] public SpriteRenderer barSprite;
		[HideInInspector] public SpriteRenderer backSprite;

		//[HideInInspector] public GameObject defaultPrefabInstance;

		[HideInInspector] public bool billboard = true;

		private const string PLACEHOLDER_CANVAS_NAME = "PLACEHOLDER_VITALS_CANVAS";

		protected override void Reset()
		{
			base.Reset();
			FindUIElements();
		}

#if UNITY_EDITOR

		protected override void OnValidate()
		{
			base.OnValidate();
			AutoAlign();
		}

		public void AddDefaultUIPrefab()
		{
			/// First time we have tried to use the prefab.. need to locate it.
			if (vitalBarDefaultPrefab == null)
			{
				var aid = AssetDatabase.FindAssets("VitalBarPrefab");
				var path = AssetDatabase.GUIDToAssetPath(aid[0]);
				vitalBarDefaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}

			if (vitalBarDefaultPrefab == null)
			{
				Debug.LogWarning("Default VitalBar prefab could not be found.");
				return;
			}

			Instantiate(vitalBarDefaultPrefab, transform);

			Recalculate();
		}
#endif

		protected override void Awake()
		{
			base.Awake();

			FindUIElements();

			enabled = billboard;
		}

		public override void Recalculate()
		{
			AutoAlign();

			if (backSprite)
				backSprite.size = new Vector2(widthMultiplier, backSprite.size.y);

			UpdateGraphics(vital);
		}

		protected virtual void AutoAlign()
		{
			/// Set the auto height offset of default bars when the target vital is changed in the editor
			if (autoOffset && transform.parent)
				transform.localPosition = offset * vitalIndex;
		}

		private static List<SpriteRenderer> resuableFindSpriteRend = new List<SpriteRenderer>();
		/// <summary>
		/// Finds UI Elements.
		/// </summary>
		/// <returns></returns>
		public bool FindUIElements()
		{

			if (textMesh == null)
				textMesh = (searchChildren) ? GetComponentInChildren<TextMesh>() : GetComponent<TextMesh>();

			/// Find Sprites
			if (searchChildren)
				GetComponentsInChildren(resuableFindSpriteRend);
			else
				GetComponents(resuableFindSpriteRend);

			//Debug.Log(name + " Find Rends " + resuableFindSpriteRend.Count);

			if (resuableFindSpriteRend.Count > 0 && barSprite == null)
				barSprite = resuableFindSpriteRend[0];
			if (resuableFindSpriteRend.Count > 1 && backSprite == null)
				backSprite = resuableFindSpriteRend[1];

			if (UIText == null)
				UIText = (searchChildren) ? GetComponentInChildren<Text>() : GetComponent<Text>();

			if (UIImage == null)
				UIImage = (searchChildren) ? GetComponentInChildren<Image>() : GetComponent<Image>();

			return textMesh || barSprite || UIText || UIImage;

		}

		public override void UpdateGraphics(Vital vital)
		{
			if (vital == null)
				return;

			var vitalDef = vital.VitalDef;

			double val = /*(vital == null) ? 1 :*/
				(targetField == TargetField.Value) ? vital.VitalData.Value :
				(targetField == TargetField.Max) ? vitalDef.FullValue :
				vitalDef.MaxValue;

			/// NegativeInfinity is my indicator that this vitalData value has no data (may be a delta frame) - so it is not null... its just unknown.
			if (val == float.NegativeInfinity)
				return;

			if (textMesh)
				textMesh.text = ((int)val).ToString();

			if (barSprite)
				barSprite.size = new Vector2((float)((val / vitalDef.MaxValue) * widthMultiplier), barSprite.size.y);

			if (UIText != null)
			{
				UIText.text = ((int)val).ToString();
			}

			// Resize the healthbar
			if (UIImage != null)
			{
				double fullval = vitalDef.MaxValue;

				if (UIImage.type == Image.Type.Filled && UIImage.sprite != null)
					UIImage.fillAmount = (float)((val / fullval) * widthMultiplier);
				else
					UIImage.rectTransform.localScale = new Vector3(
						(float)((val / fullval) * widthMultiplier),
						UIImage.rectTransform.localScale.y,
						UIImage.rectTransform.localScale.z);
			}
		}

		public void LateUpdate()
		{
			var cam = Camera.main;
			if (!cam)
				return;

			transform.LookAt(cam.transform, new Vector3(0, 1, 0));
			var curr = transform.eulerAngles;
			transform.eulerAngles = new Vector3(0, curr.y + 180f, 0);
		}
	}


#if UNITY_EDITOR

	[CustomEditor(typeof(VitalUI))]
	[CanEditMultipleObjects]
	public class VitalUIEditor : VitalUIBaseEditor
	{
		bool uiElementsExpanded = true;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			VitalUI _target = (VitalUI)target;

			_target.FindUIElements();
			_target.Recalculate();

			uiElementsExpanded = EditorGUILayout.Foldout(uiElementsExpanded, "UI Elements");

			if (uiElementsExpanded)
			{
				BeginVerticalBox();

				EditorGUI.BeginChangeCheck();

				var sp = serializedObject.FindProperty("searchChildren");

				EditorGUILayout.PropertyField(sp);

				/// Draw all remaining hidden fields
				while (sp.Next(false))
					EditorGUILayout.PropertyField(sp);

				if (EditorGUI.EndChangeCheck())
					serializedObject.ApplyModifiedProperties();

				EndVerticalBox();
			}
		}
	}

#endif

}
