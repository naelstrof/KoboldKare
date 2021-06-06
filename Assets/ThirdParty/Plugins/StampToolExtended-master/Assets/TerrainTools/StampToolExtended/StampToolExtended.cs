#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEditor.Experimental.TerrainAPI
{
    internal class StampToolExtended : TerrainPaintTool<StampToolExtended>
    {
        class Styles
        {
            public static readonly GUIContent stampHeight = EditorGUIUtility.TrTextContent("Stamp Height", "");
            public static readonly GUIContent invertStampHeight = EditorGUIUtility.TrTextContent("Invert Stamp Height", "");

            public static readonly GUIContent brushSize = EditorGUIUtility.TrTextContent("Brush Size", "");
            public static readonly GUIContent brushOpacity = EditorGUIUtility.TrTextContent("Opacity", "");
            public static readonly GUIContent brushRotation = EditorGUIUtility.TrTextContent("Rotation", "");

            public static readonly GUIContent previewEnabled = EditorGUIUtility.TrTextContent("3D Preview", "");
        }

        [SerializeField]
        private float m_StampHeight = 0.0f;

        [SerializeField]
        private float m_BrushRotation = 0.0f;

        [SerializeField]
        private float m_BrushSize = 40.0f;

        [SerializeField]
        private float m_BrushStrength = 1.0f;

        private const float brushSizeSafetyFactorHack = 0.9375f;
        private const float k_mouseWheelToHeightRatio = -0.0004f;

        #region BrushPreview3d

        [SerializeField]
        private bool preview3dEnabled = false;

        private class BrushPreview3d
        {
            public static int DefaultMeshTextureSize = 64;

            public int brushInstanceId;
            public int meshPreviewTextureSize;
            public float stampHeight;
            public Texture2D meshPreviewTexture;
            public Mesh mesh;
            public Material material;

            public BrushPreview3d(int meshPreviewTextureSize)
            {
                this.meshPreviewTextureSize = meshPreviewTextureSize;

                this.brushInstanceId = -1;
                this.meshPreviewTexture = null;
                this.mesh = null;
                this.material = null;
                this.stampHeight = 0f;
            }

            public bool IsValid()
            {
                return meshPreviewTexture != null && mesh != null;
            }

        }

        BrushPreview3d brushPreview3d = new BrushPreview3d(BrushPreview3d.DefaultMeshTextureSize);

        PreviewRenderUtility m_PreviewUtility;

        #endregion BrushPreview3d

        public override string GetName()
        {
            return "Stamp Terrain (Extended)";
        }

        public override string GetDesc()
        {
            return "Stamp on the terrain with additional mouse controls.\n\nLeft click: Stamp brush on the terrain.\nCtrl + Mouse drag left/right: Resize brush.\nCtrl + Mousewheel: Rotate brush.\nCtrl + Shift + Mousewheel: Adjust stamp height.";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            Event evt = Event.current;

            // brush rotation
            if (evt.control && !evt.shift && evt.type == EventType.ScrollWheel)
            {
                m_BrushRotation += Event.current.delta.y;

                if (m_BrushRotation >= 360)
                {
                    m_BrushRotation -= 360;
                }

                if (m_BrushRotation < 0)
                {
                    m_BrushRotation += 360;
                }

                m_BrushRotation %= 360;

                evt.Use();
                editContext.Repaint();
            }

            // brush resize
            if (evt.control && evt.type == EventType.MouseDrag)
            {

                m_BrushSize += Event.current.delta.x;

                evt.Use();
                editContext.Repaint();
            }

            // stamp height
            if (evt.control && evt.shift && evt.type == EventType.ScrollWheel)
            {
                m_StampHeight += Event.current.delta.y * k_mouseWheelToHeightRatio * editContext.raycastHit.distance;

                evt.Use();
                editContext.Repaint();
            }

            if (evt.type != EventType.Repaint)
                return;

            if (editContext.hitValidTerrain)
            {
                BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord, m_BrushSize, m_BrushRotation);
                PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

                Material material = TerrainPaintUtilityEditor.GetDefaultBrushPreviewMaterial();

                TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainPaintUtilityEditor.BrushPreview.SourceRenderTexture, editContext.brushTexture, brushXform, material, 0);

                ApplyBrushInternal(paintContext, m_BrushStrength, editContext.brushTexture, brushXform, terrain);

                RenderTexture.active = paintContext.oldRenderTexture;

                material.SetTexture("_HeightmapOrig", paintContext.sourceRenderTexture);

                TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainPaintUtilityEditor.BrushPreview.DestinationRenderTexture, editContext.brushTexture, brushXform, material, 1);

                TerrainPaintUtility.ReleaseContextResources(paintContext);
            }

            #region BrushPreview3d

            if (preview3dEnabled)
            {
                UpdateBrushPreview3d(editContext);
            }

            #endregion BrushPreview3d
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.BeginChangeCheck();

                // stamp height and inverted stamp height: keep height values positive for the user
                float stampHeight = Mathf.Abs(m_StampHeight);
                bool invertStampHeight = m_StampHeight < 0.0f;

                stampHeight = EditorGUILayout.Slider(Styles.stampHeight, stampHeight, 0, terrain.terrainData.size.y);
                invertStampHeight = EditorGUILayout.Toggle(Styles.invertStampHeight, invertStampHeight);

                if (EditorGUI.EndChangeCheck())
                {
                    m_StampHeight = (invertStampHeight ? -stampHeight : stampHeight);
                }
            }

            // show in-built brush selection
            editContext.ShowBrushesGUI(5, BrushGUIEditFlags.Select);

            // custom controls for brush
            m_BrushSize = EditorGUILayout.Slider(Styles.brushSize, m_BrushSize, 0.1f, Mathf.Round(Mathf.Min(terrain.terrainData.size.x, terrain.terrainData.size.z) * brushSizeSafetyFactorHack));
            m_BrushStrength = AddPercentSlider(Styles.brushOpacity, m_BrushStrength, 0, 1);
            m_BrushRotation = EditorGUILayout.Slider(Styles.brushRotation, m_BrushRotation, 0, 359);

            #region BrushPreview3d

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(Styles.previewEnabled);

                EditorGUILayout.BeginVertical();
                {
                    preview3dEnabled = EditorGUILayout.Toggle(GUIContent.none, preview3dEnabled);

                    if (preview3dEnabled)
                    {
                        AddPreviewMeshFromHeightmap(terrain, editContext);
                    }
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndHorizontal();

            #endregion BrushPreview3d

            if (EditorGUI.EndChangeCheck())
            {
                Save(true);

                // update scene view, otherwise eg changing the "show path" option wouldn't be visualized immediately
                SceneView.RepaintAll();
            }



            base.OnInspectorGUI(terrain, editContext);
        }


        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            if (Event.current.type == EventType.MouseDrag)
                return true;

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, m_BrushSize, m_BrushRotation);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());

            ApplyBrushInternal(paintContext, m_BrushStrength, editContext.brushTexture, brushXform, terrain);

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Tools - Stamp Tool Extended");

            return true;
        }

        private void ApplyBrushInternal(PaintContext paintContext, float brushStrength, Texture brushTexture, BrushTransform brushXform, Terrain terrain)
        {
            Material material = TerrainPaintUtility.GetBuiltinPaintMaterial();

            float height = m_StampHeight / terrain.terrainData.size.y;

            Vector4 brushParams = new Vector4(brushStrength, 0.0f, height, 1.0f);

            material.SetTexture("_BrushTex", brushTexture);
            material.SetVector("_BrushParams", brushParams);

            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, material);

            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, material, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.StampHeight);
        }

        private float AddPercentSlider(GUIContent guiContent, float valueInPercent, float minValue, float maxValue)
        {
            EditorGUI.BeginChangeCheck();

            float value = EditorGUILayout.Slider(guiContent, Mathf.Round(valueInPercent * 100f), minValue * 100f, maxValue * 100f);

            if (EditorGUI.EndChangeCheck())
            {
                return value / 100f;
            }

            return valueInPercent;
        }

        #region BrushPreview3d

        private void UpdateBrushPreview3d(IOnSceneGUI editContext)
        {
            bool dirty = brushPreview3d.stampHeight != m_StampHeight;

            if (brushPreview3d.brushInstanceId == editContext.brushTexture.GetInstanceID() && !dirty)
                return;

            brushPreview3d.stampHeight = m_StampHeight;

            brushPreview3d.brushInstanceId = editContext.brushTexture.GetInstanceID();
            brushPreview3d.meshPreviewTexture = ScaleTexture(editContext.brushTexture as Texture2D, brushPreview3d.meshPreviewTextureSize, brushPreview3d.meshPreviewTextureSize);
            brushPreview3d.mesh = GenerateMeshFromHeightmap(brushPreview3d.meshPreviewTexture);

            // note: the red tint on the 3d preview is just a side effect of the scaling (grayscale, rgb). but i like it => leaving it as it is
            //       if you want grayscale, either just set color to black or do a proper conversion
            //Material material = new Material(Shader.Find("Standard"));
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.white;
            material.SetTexture("_MainTex", brushPreview3d.meshPreviewTexture);
            material.SetTextureScale("_MainTex", new Vector2(1.0f / brushPreview3d.meshPreviewTexture.width, 1.0f / brushPreview3d.meshPreviewTexture.height));

            brushPreview3d.material = material;
        }

        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int i = 0; i < result.height; ++i)
            {
                for (int j = 0; j < result.width; ++j)
                {
                    Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                    result.SetPixel(j, i, newColor);
                }
            }
            result.Apply();
            return result;
        }

        private void AddPreviewMeshFromHeightmap(Terrain terrain, IOnInspectorGUI editContext)
        {
            if (!brushPreview3d.IsValid())
                return;

            int previewSize = 256;

            Texture2D heightmap = brushPreview3d.meshPreviewTexture;
            Mesh mesh = brushPreview3d.mesh;
            Material material = brushPreview3d.material;

            Bounds bounds = mesh.bounds;

            /* center view:
            m_PreviewUtility.camera.clearFlags = CameraClearFlags.Color;

            float halfSize = bounds.extents.magnitude;
            float distance = 2.0f * halfSize;

            m_PreviewUtility.camera.transform.position = -Vector3.forward * distance;
            m_PreviewUtility.camera.transform.rotation = Quaternion.identity;
            m_PreviewUtility.camera.nearClipPlane = distance - halfSize * 1.1f;
            m_PreviewUtility.camera.farClipPlane = distance + halfSize * 1.1f;

            m_PreviewUtility.lights[0].intensity = 1.4f;
            m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 60f, 0);
            m_PreviewUtility.lights[1].intensity = 1.4f;

            //m_PreviewUtility.camera.transform.position += Vector3.up * distance;
            //m_PreviewUtility.camera.transform.LookAt(bounds.center);
            */

            Rect previewRect = new Rect(0, 0, previewSize, previewSize);
            this.m_PreviewUtility.BeginStaticPreview(previewRect);

            float angle = m_BrushRotation;
            Quaternion quaternion = Quaternion.Euler(0f, angle, 0f);
            Vector3 pos = quaternion * -bounds.center;

            m_PreviewUtility.DrawMesh(mesh, pos, quaternion, material, 0);

            m_PreviewUtility.camera.Render();

            Texture previewImage = this.m_PreviewUtility.EndStaticPreview();

            // draw image as label
            GUILayout.Label(previewImage);

        }

        /// <summary>
        /// Convert a heightmap to a mesh
        /// https://answers.unity.com/questions/1033085/heightmap-to-mesh.html
        /// </summary>
        /// <param name="heightmap"></param>
        /// <returns></returns>
        public Mesh GenerateMeshFromHeightmap(Texture2D heightmap)
        {
            int size = heightmap.width;
            float height = m_StampHeight;

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            //Bottom left section of the map, other sections are similar
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    //Add each new vertex in the plane
                    verts.Add(new Vector3(i, heightmap.GetPixel(i, j).grayscale * height, j));
                    //Skip if a new square on the plane hasn't been formed
                    if (i == 0 || j == 0) continue;
                    //Adds the index of the three vertices in order to make up each of the two tris
                    tris.Add(size * i + j); //Top right
                    tris.Add(size * i + j - 1); //Bottom right
                    tris.Add(size * (i - 1) + j - 1); //Bottom left - First triangle
                    tris.Add(size * (i - 1) + j - 1); //Bottom left 
                    tris.Add(size * (i - 1) + j); //Top left
                    tris.Add(size * i + j); //Top right - Second triangle
                }
            }

            Vector2[] uvs = new Vector2[verts.Count];
            for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
                uvs[i] = new Vector2(verts[i].x, verts[i].z);

            Mesh procMesh = new Mesh();
            procMesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
            procMesh.uv = uvs;
            procMesh.triangles = tris.ToArray();
            procMesh.RecalculateNormals(); //Determines which way the triangles are facing
            procMesh.RecalculateBounds();
            procMesh.RecalculateTangents();

            return procMesh;

        }

        private void InitPreview()
        {
            if (m_PreviewUtility == null)
            {
                m_PreviewUtility = new PreviewRenderUtility(false, true);
                m_PreviewUtility.cameraFieldOfView = 60.0f;
                m_PreviewUtility.camera.nearClipPlane = 0.1f;
                m_PreviewUtility.camera.farClipPlane = 220.0f;
                //m_PreviewUtility.camera.transform.position = new Vector3(80, 30, 80);
                m_PreviewUtility.camera.transform.position = new Vector3(50, 40, 50);
                m_PreviewUtility.camera.transform.rotation = Quaternion.identity;

                m_PreviewUtility.camera.transform.LookAt(new Vector3(0, 0, 0));

                m_PreviewUtility.lights[0].intensity = 1.4f;
                m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 30f, 0f);
                m_PreviewUtility.lights[1].intensity = 1.4f;

            }
            else
            {
                m_PreviewUtility.Cleanup();
                return;
            }

        }

        public override void OnEnable()
        {
            InitPreview();
        }

        public override void OnDisable()
        {

            if (this.m_PreviewUtility == null)
                return;

            this.m_PreviewUtility.Cleanup();
            this.m_PreviewUtility = (PreviewRenderUtility)null;

            this.brushPreview3d = (BrushPreview3d) null;
        }

        #endregion BrushPreview3d
    }
}
#endif
