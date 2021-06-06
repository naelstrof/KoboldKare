// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Pun.Simple.Internal;
#endif

namespace Photon.Pun.Simple
{


#if UNITY_EDITOR



    public class NetCoreHeaderEditor : HeaderEditorBase
    {
        //protected override bool UseThinHeader { get { return true; } }

        protected override string BackTexturePath
        {
            get { return "Header/RedBack"; }
        }

    }

    public class SystemHeaderEditor : HeaderEditorBase
    {
        protected override bool UseThinHeader { get { return true; } }

        protected override string TextTexturePath
        {
            get { return "Header/SystemText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/GrayBack"; }
        }

        protected override string IconTexturePath
        {
            get { return null; }
        }
    }

    public class AccessoryHeaderEditor : HeaderEditorBase
    {
        protected override bool UseThinHeader { get { return true; } }

        protected override string TextTexturePath
        {
            get { return "Header/AccessoryText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/GrayBack"; }
        }
    }

    public class MountSystemHeaderEditor : HeaderEditorBase
    {
        //protected override bool UseThinHeader { get { return true; } }

        protected override string TextTexturePath
        {
            get { return "Header/MountSystemText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/GreenBack"; }
        }
    }


    //public class NetUtilityHeaderEditor : HeaderEditorBase
    //{
    //    protected override bool UseThinHeader { get { return true; } }

    //    protected override string TextTexturePath
    //    {
    //        get { return "Header/NetUtilityText"; }
    //    }

    //    protected override string BackTexturePath
    //    {
    //        get { return "Header/GrayBack"; }
    //    }

    //    protected override string IconTexturePath
    //    {
    //        get { return null; }
    //    }
    //}

    public class ReactorHeaderEditor : HeaderEditorBase
    {
        protected override string HelpURL
        {
            get { return SimpleDocsURLS.SUBSYS_PATH + "#contact_system"; }
        }

        protected override string TextTexturePath
        {
            get { return "Header/ReactorText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/VioletBack"; }
        }

    }

    public class ContactSystemHeaderEditor : HeaderEditorBase
    {
        protected override string HelpURL { get { return SimpleDocsURLS.SUBSYS_PATH + "#contact_system"; } }

        protected override string TextTexturePath
        {
            get { return "Header/ContactSystemText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/GreenBack"; }
        }
    }

    public class ContactReactorHeaderEditor : ReactorHeaderEditor
    {
        protected override string HelpURL { get { return SimpleDocsURLS.SUBSYS_PATH + "#contact_system"; } }

        protected override string TextTexturePath
        {
            get { return "Header/ContactReactorText"; }
        }
    }

    public class StateReactorHeaderEditor : ReactorHeaderEditor
    {
        protected override string HelpURL
        {
            get { return SimpleDocsURLS.SYNCCOMPS_PATH + "#onstatechangetoggle_component"; }
        }

        protected override string TextTexturePath
        {
            get { return "Header/StateReactorText"; }
        }
    }

    public class ContactGroupHeaderEditor : HeaderEditorBase
    {
        protected override string HelpURL { get { return SimpleDocsURLS.SUBSYS_PATH + "#contact_groups"; } }

        protected override string TextTexturePath
        {
            get { return "Header/ContactGroupsText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/CyanBack"; }
        }
    }

    public class AutomationHeaderEditor : HeaderEditorBase
    {
        protected override bool UseThinHeader { get { return true; } }

        protected override string TextTexturePath
        {
            get { return "Header/AutomationText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/GrayBack"; }
        }

    }

    public class TriggerHeaderEditor : HeaderEditorBase
    {
        protected override string HelpURL

        {
            get { return null; }
        }
        //protected override bool UseThinHeader { get { return true; } }

        protected override string TextTexturePath
        {
            get { return "Header/TriggerText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/OrangeBack"; }
        }

    }

    public class SampleCodeHeaderEditor : HeaderEditorBase
    {
        protected override bool UseThinHeader { get { return true; } }

        protected override string TextTexturePath
        {
            get { return "Header/SampleCodeText"; }
        }

        protected override string BackTexturePath
        {
            get { return "Header/GrayBack"; }
        }

        protected override string IconTexturePath
        {
            get { return null; }
        }
    }

    /// <summary>
    /// All of this just draws the pretty header graphic on components. Nothing to see here.
    /// </summary>
    [CustomEditor(typeof(Component))]
    [CanEditMultipleObjects]
    public class HeaderEditorBase : Editor
    {

        protected Texture2D textTexture;
        protected Texture2D gridTexture;
        protected Texture2D backTexture;
        protected Texture2D iconTexture;
        protected Texture2D leftCap;

        protected static Texture2D blackDot, leftCapThin;

        protected virtual string TextTexturePath { get { return null; } }
        protected virtual string TextFallbackPath { get { return null; } }
        protected virtual string GridTexturePath { get { return null; } }
        protected virtual string IconTexturePath { get { return "Header/PUN_Icon"; ; } }
        protected virtual string IconFallbackPath { get { return null; } }
        protected virtual string BackTexturePath { get { return "Header/GrayBack"; } }
        protected virtual string BackFallbackPath { get { return "Header/GrayBack"; } }
        //protected string LeftCapPath = "Header/LeftCap";
        protected string LeftCapThinPath = "Header/LeftCap_thin";
        protected string BlackDotPath = "Header/BlackDot";

        protected bool showInstructions;
        protected static GUIContent showInstrGC = new GUIContent("Instructions");
        protected static GUIStyle richBox;
        protected static GUIStyle richLabel;
        protected static GUIStyle labelright;

        protected static GUIContent reusableGC = new GUIContent();
        /// <summary>
        /// Override this property. Any non-null value will show up as an instructions foldout.
        /// </summary>
        protected virtual string Instructions { get { return null; } }

        protected virtual string Overview { get { return null; } }

        protected virtual bool UseThinHeader { get { return false; } }

        /// <summary>
        /// Override this property. Any non-null value will turn the header graphic into a clickable link to this url.
        /// </summary>
        protected virtual string HelpURL { get { return SimpleDocsURLS.SYNCCOMPS_PATH; } }

        public virtual void OnEnable()
        {
            //Debug.Log("Dev-Doc!");
            bool usethin = UseThinHeader || !TickEngineSettings.Single.showGUIHeaders;
            GetTextures(usethin);
        }

        private bool usedThin;
        private bool texturesInitialized;

        public virtual void GetTextures(bool usethin)
        {
            if (texturesInitialized && usethin == usedThin)
                return;

            string thintext = ((usethin) ? "_thin" : "");

            texturesInitialized = true;
            usedThin = usethin;

            if (textTexture == null)
                textTexture = (Texture2D)Resources.Load<Texture2D>(TextTexturePath + thintext);

            if (textTexture == null)
                textTexture = (Texture2D)Resources.Load<Texture2D>(TextFallbackPath + thintext);

            if (gridTexture == null)
                gridTexture = (Texture2D)Resources.Load<Texture2D>(GridTexturePath);

            if (iconTexture == null)
                iconTexture = (Texture2D)Resources.Load<Texture2D>(IconTexturePath + thintext);
            if (iconTexture == null)
                iconTexture = (Texture2D)Resources.Load<Texture2D>(IconFallbackPath + thintext);

            if (backTexture == null)
                backTexture = (Texture2D)Resources.Load<Texture2D>(BackTexturePath + thintext);
            if (backTexture == null)
                backTexture = (Texture2D)Resources.Load<Texture2D>(BackFallbackPath + thintext);

            if (leftCap == null)
                leftCap = (Texture2D)Resources.Load<Texture2D>(BackTexturePath + "Cap" + thintext);

            if (leftCapThin == null)
                leftCapThin = (Texture2D)Resources.Load<Texture2D>(LeftCapThinPath);

            if (blackDot == null)
                blackDot = (Texture2D)Resources.Load<Texture2D>(BlackDotPath);

        }

        protected virtual void EnsureStylesExist()
        {
            if (richBox == null)
                richBox = new GUIStyle("HelpBox")
                {
                    richText = true,
                    wordWrap = true,
                    padding = new RectOffset(6, 6, 6, 6),
                    stretchWidth = true,
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 10
                };

            if (richLabel == null)
                richLabel = new GUIStyle() { richText = true };

            if (labelright == null)
                labelright = new GUIStyle("Label") { alignment = TextAnchor.UpperRight };
        }

        public void OnUndoRedo()
        {
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            bool usethin = UseThinHeader || !TickEngineSettings.Single.showGUIHeaders;

            GetTextures(usethin);

            EnsureStylesExist();

            /// Draw headers
            if (usethin)
            {
                OverlayInstructions(ref showInstructions, Instructions, HelpURL, usethin);
                OverlayHeader(HelpURL, backTexture, gridTexture, iconTexture, textTexture, leftCap, usethin);
            }
            else
            {
                OverlayHeader(HelpURL, backTexture, gridTexture, iconTexture, textTexture, leftCap, usethin);
                OverlayInstructions(ref showInstructions, Instructions, HelpURL, usethin);
            }


            if (showInstructions)
                DrawInstructions(Instructions);

            OverlayOverview(Overview);

            OnInspectorGUIInjectMiddle();

            DrawSerializedObjectFields(serializedObject, false);

            OnInspectorGUIFooter();
        }

        protected virtual void OnInspectorGUIInjectMiddle()
        {
            //base.OnInspectorGUI();

        }
        protected virtual void OnInspectorGUIFooter()
        {

        }

        public static void OverlayHeader(string HelpURL,
            Texture2D backTexture, Texture2D gridTexture, Texture2D iconTexture, Texture2D textTexture, Texture2D leftCap, bool thin = false)
        {
            int h = thin ? 16 : 24;
            Rect r = EditorGUILayout.GetControlRect(true, thin ? h : (h + 2));

            float left = r.xMin;

            if (!thin)
                r.yMin += 2;

            r.xMin = thin ? r.xMin + EditorGUIUtility.labelWidth  /*+ 2*/ : 32;

            string url = HelpURL;
            if (url != null)
            {
                EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                    Application.OpenURL(url);
            }

            var hr = new Rect(r) /*{ xMin = r.xMin + 8 }*/;

            if (backTexture != null)
                GUI.DrawTexture(hr, backTexture);


            /// Draw left endcap
            if (thin)
            {
                // GUI.DrawTexture(new Rect(hr) { xMin = hr.xMin - 2, width = 2 }, leftCapThin);
                // for thin, add an extra bit of lower padding. Don't add an end cap.
                EditorGUILayout.GetControlRect(false, 2);
            }
            else
                GUI.DrawTexture(new Rect(r) { xMin = r.xMin - 16, width = 16 }, leftCap);

            /// Draw repeating pattern
            if (gridTexture != null)
                GUI.DrawTexture(hr, gridTexture, ScaleMode.ScaleAndCrop);

            /// Draw Icon layer
            if (iconTexture != null)
                GUI.DrawTexture(new Rect(hr.xMax - 248, hr.yMin, 248, h), iconTexture);

            /// Draw Text
            if (textTexture != null)
                GUI.DrawTexture(new Rect(hr) { x = thin ? hr.x + 6 : (hr.x - 8), width = 248 }, textTexture);

        }

        private GUIStyle instructionsStyle;
        private GUIContent nolabel = new GUIContent(" ");
        private GUIContent scirptlabel = new GUIContent("Script");

        public void OverlayInstructions(ref bool showInstructions, string instructions, string url, bool thin = false)
        {
            Rect r = thin ? EditorGUILayout.GetControlRect(false, 0) : EditorGUILayout.GetControlRect(true, 18);
            r.height = 18;

            if (instructions != null)
            {

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.LabelField(new Rect(r) { xMin = r.xMin + 14, yMin = r.yMin + 2 }, showInstrGC);
                EditorGUI.EndDisabledGroup();
                showInstructions = EditorGUI.Toggle(new Rect(r) { yMin = r.yMin + 2, width = EditorGUIUtility.labelWidth - 20 }, GUIContent.none, showInstructions, (GUIStyle)"Foldout");

                DrawScriptField(serializedObject, new Rect(r) { yMin = r.yMin + 2 }, nolabel);

            }
            else
            {
                DrawScriptField(serializedObject, new Rect(r) { yMin = r.yMin + 2 }, scirptlabel);
            }


            /// Draw Docs Link Ico
            if (/*thin && */HelpURL != null)
            {
                //var helpIcoRect = new Rect(r) { xMin = r.xMin - 8 - 16 - 2, width = 16 };
                EditorUtils.DrawDocsIcon(r.xMin + EditorGUIUtility.labelWidth - 16 - 4, r.yMin + 2, url);
            }
        }

        protected void DrawInstructions(string instructions)
        {
            if (instructionsStyle == null)
            {
                instructionsStyle = new GUIStyle("HelpBox");
                instructionsStyle.richText = true;
                instructionsStyle.padding = new RectOffset(6, 6, 6, 6);
            }
            EditorGUILayout.LabelField(instructions, instructionsStyle);
        }

        protected static GUIStyle overviewStyle;

        public void OverlayOverview(string text)
        {
            if (text == null)
                return;

            if (overviewStyle == null)
            {
                overviewStyle = new GUIStyle("HelpBox"); //GUI.skin.GetStyle("HelpBox");
                overviewStyle.richText = true;
                overviewStyle.padding = new RectOffset(6, 6, 6, 6);
            }

            EditorGUILayout.LabelField(text, overviewStyle);
        }

        public static void DrawScriptField(SerializedObject so)
        {
            SerializedProperty sp = so.GetIterator();
            sp.Next(true);

            sp.NextVisible(false);
            EditorGUI.BeginDisabledGroup(true);
            Rect r = EditorGUILayout.GetControlRect(false, EditorGUI.GetPropertyHeight(sp));
            EditorGUI.PropertyField(r, sp);
            EditorGUI.EndDisabledGroup();
        }

        public static void DrawScriptField(SerializedObject so, Rect r, GUIContent label)
        {
            SerializedProperty sp = so.GetIterator();
            sp.Next(true);

            sp.NextVisible(false);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(r, sp, label);
            EditorGUI.EndDisabledGroup();
        }

        public static void DrawSerializedObjectFields(SerializedObject so, bool includeScriptField)
        {
            SerializedProperty sp = so.GetIterator();
            sp.Next(true);

            // Skip drawing the script reference?
            if (!includeScriptField)
                sp.NextVisible(false);

            EditorGUI.BeginChangeCheck();

            int skipNextX = 0;
            int wrapNextX = 0;

            while (sp.NextVisible(false))
            {
                /// Skip entries if we have triggered a HideNextX
                if (skipNextX > 0)
                {
                    skipNextX--;
                    continue;
                }

                EditorGUILayout.PropertyField(sp);

                if (wrapNextX > 0)
                {
                    wrapNextX--;
                    if (wrapNextX == 0)
                        EditorGUILayout.EndVertical();
                }

                /// Handling for HideNextXAttribute
                var obj = sp.serializedObject.targetObject.GetType();
                var fld = obj.GetField(sp.name);
                if (fld != null)
                {
                    var attrs = fld.GetCustomAttributes(false);
                    foreach (var a in attrs)
                    {
                        var hnx = a as HideNextXAttribute;
                        if (hnx != null)
                            if (sp.propertyType == SerializedPropertyType.Boolean)
                                if (sp.boolValue == hnx.hideIf)
                                {
                                    skipNextX = (a as HideNextXAttribute).hideCount;
                                }
                                else
                                {
                                    wrapNextX = (a as HideNextXAttribute).hideCount;
                                    if (hnx.guiStyle != null || hnx.guiStyle == "")
                                        EditorGUILayout.BeginVertical((GUIStyle)hnx.guiStyle);
                                    else
                                        EditorGUILayout.BeginVertical();
                                }
                    }
                }

            }

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
            }
        }

        public static GUIStyle defaultVertBoxStyle;
        const int vertpad = 6;
        static int holdIndent;

        public static Rect BeginVerticalBox(GUIStyle gstyle = null)
        {
            if (defaultVertBoxStyle == null)
                defaultVertBoxStyle = new GUIStyle((GUIStyle)"HelpBox")
                {
                    margin = new RectOffset(),
                    padding = new RectOffset(vertpad, vertpad, vertpad, vertpad)
                };

            Rect r = EditorGUILayout.BeginVertical(defaultVertBoxStyle);

            return r;
        }

        public static void EndVerticalBox()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        protected static void Divider()
        {
            EditorGUILayout.Space();
            Rect r = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(r, Color.black);
            EditorGUILayout.Space();
        }

        protected bool IndentedFoldout(GUIContent gc, bool folded, int indent)
        {
            var holdindent = EditorGUI.indentLevel;
            EditorGUI.indentLevel += indent;
            var r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, gc);
            bool val = EditorGUI.Toggle(new Rect(r) { x = r.x - 12 }, GUIContent.none, folded, (GUIStyle)"Foldout");
            EditorGUI.indentLevel = holdindent;
            return val;
        }

        protected static void CustomGUIRender(SerializedObject so)
        {
            var property = so.GetIterator();
            property.Next(true);
            property.NextVisible(true);

            do
            {
                EditorGUILayout.PropertyField(property);
            }
            while (property.NextVisible(false));
        }

        /// <summary>
        /// Gets the Display Name and Tooltip for an SP using reflection, since Unity doesn't always return serializedProperty.toottip correctly. Returned GUIContent is recycled, so use immediately or copy it.
        /// </summary>
        /// <returns></returns>
        protected static GUIContent GetGUIContent(SerializedProperty sp)
        {
            reusableGC.text = sp.displayName;
            reusableGC.tooltip = null;

            var type = sp.serializedObject.targetObject.GetType();

            var field = type.GetField(sp.name, (System.Reflection.BindingFlags)int.MaxValue);

            if (field == null)
            {
                return reusableGC;
            }

            var attrs = field.GetCustomAttributes(true);

            foreach (var t in attrs)
            {
                var tooltiptype = t as TooltipAttribute;
                if (tooltiptype != null)
                {
                    reusableGC.tooltip = tooltiptype.tooltip;
                }
            }

            return reusableGC;
        }

    }



#endif

}

