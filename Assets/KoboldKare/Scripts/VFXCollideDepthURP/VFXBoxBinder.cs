using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.VFX.Utility
{
    [AddComponentMenu("VFX/Property Binders/Box Binder")]
    [VFXBinder("AABB/Box")]
    class VFXBoxBinder : VFXBinderBase {
        public string Property { get { return (string)m_Property; } set { m_Property = value; UpdateSubProperties(); } }
        [VFXPropertyBinding("UnityEditor.VFX.Box"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_Parameter")]
        protected ExposedProperty m_Property = "Box";
        public BoxCollider boxCollider;
        private ExposedProperty BoxCenter;
        private ExposedProperty BoxSize;
        protected override void OnEnable() {
            base.OnEnable();
            UpdateSubProperties();
        }

        void OnValidate() {
            UpdateSubProperties();
        }

        void UpdateSubProperties() {
            BoxCenter = m_Property + "_center";
            BoxSize = m_Property + "_size";
        }

        public override bool IsValid(VisualEffect component) {
            return boxCollider != null && component.HasVector3((int)BoxCenter) && component.HasVector3((int)BoxSize);
        }

        public override void UpdateBinding(VisualEffect component) {
            component.SetVector3((int)BoxCenter, boxCollider.transform.TransformPoint(boxCollider.center));
            component.SetVector3((int)BoxSize, boxCollider.size);
        }

        public override string ToString() {
            return string.Format("Box : '{0}' -> {1}", m_Property, boxCollider == null ? "(null)" : boxCollider.ToString());
        }
    }
}
