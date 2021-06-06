// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR


using emotitron.Compression;
using Photon.Compression;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun.Simple.Assists
{
	public static class SyncTransformAssists
	{

		public const string SYNC_TRANS_FOLDER = AssistHelpers.ADD_TO_OBJ_FOLDER + "SyncTransform Defaults/";

		//public static SyncTransform AddDefaultSyncTransform()
		//{
		//	var selection = Selection.activeGameObject;

		//	if (!selection)
		//	{
		//		Debug.LogWarning("No Object Selected.");
		//		return null;
		//	}

		//	SyncTransform st = selection.GetComponent<SyncTransform>();
		//	if (!st)
		//		st = selection.AddComponent<SyncTransform>();

		//	return st;
		//}
		//public static SyncTransform AddDefaultSyncTransform(Transform t)
		//{
		//	if (ReferenceEquals(t, null))
		//		t = Selection.activeTransform;

		//	if (ReferenceEquals(t, null))
		//		return null;

		//	SyncTransform st = t.GetComponent<SyncTransform>();
		//	if (!st)
		//		st = t.gameObject.AddComponent<SyncTransform>();

		//	return st;
		//}

		public static SyncTransform AddDefaultDisabledSyncTransform(Transform t)
		{
			if (ReferenceEquals(t, null))
				t = Selection.activeTransform;

			if (ReferenceEquals(t, null))
				return null;

			SyncTransform st = t.GetComponent<SyncTransform>();
			if (!st)
			{
				st = t.gameObject.AddComponent<SyncTransform>();
				Debug.Log("Added SyncTransform to " + t.name);
				var tc = st.transformCrusher;
				var pc = tc.PosCrusher;
				var rc = tc.RotCrusher;
				var sc = tc.SclCrusher;

				pc.XCrusher.Enabled = false;
				pc.YCrusher.Enabled = false;
				pc.ZCrusher.Enabled = false;

				rc.TRSType = TRSType.Euler;
				rc.XCrusher.Enabled = false;
				rc.YCrusher.Enabled = false;
				rc.ZCrusher.Enabled = false;

				sc.uniformAxes = ElementCrusher.UniformAxes.NonUniform;
				sc.XCrusher.Enabled = false;
				sc.YCrusher.Enabled = false;
				sc.ZCrusher.Enabled = false;
			}

			return st;
		}

		[MenuItem(SYNC_TRANS_FOLDER + "Default 3D", false, AssistHelpers.PRIORITY)]
        public static SyncTransform AddDefaultSyncTransform3D()
		{
			return AddDefaultSyncTransform3D(Selection.activeTransform);
		}
		public static SyncTransform AddDefaultSyncTransform3D(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var pc = tc.PosCrusher;
			var rc = tc.RotCrusher;
			var sc = tc.SclCrusher;

			pc.XCrusher.Enabled = true;
			pc.YCrusher.Enabled = true;
			pc.ZCrusher.Enabled = true;

			rc.TRSType = TRSType.Euler;
			rc.XCrusher.Enabled = true;
			rc.YCrusher.Enabled = true;
			rc.ZCrusher.Enabled = true;

			sc.uniformAxes = ElementCrusher.UniformAxes.XYZ;
			sc.UCrusher.Enabled = true;

			return st;
		}

		[MenuItem(SYNC_TRANS_FOLDER + "Default 3D Rigidbody", false, AssistHelpers.PRIORITY)]
        public static SyncTransform AddDefaultSyncTransform3DRigidbody()
		{
			return AddDefaultSyncTransform3DRigidbody(Selection.activeTransform);
		}
		public static SyncTransform AddDefaultSyncTransform3DRigidbody(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var pc = tc.PosCrusher;
			var rc = tc.RotCrusher;
			var sc = tc.SclCrusher;

			pc.XCrusher.Enabled = true;
			pc.YCrusher.Enabled = true;
			pc.ZCrusher.Enabled = true;

			rc.TRSType = TRSType.Quaternion;
			rc.Enabled = true;

			return st;
		}


		[MenuItem(SYNC_TRANS_FOLDER + "Default 2D", false, AssistHelpers.PRIORITY)]
        public static SyncTransform AddDefaultSyncTransform2D()
		{
			return AddDefaultSyncTransform2D(Selection.activeTransform);
		}
		public static SyncTransform AddDefaultSyncTransform2D(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var pc = tc.PosCrusher;
			var rc = tc.RotCrusher;
			var sc = tc.SclCrusher;

			pc.XCrusher.Enabled = true;
			pc.YCrusher.Enabled = true;
			pc.ZCrusher.Enabled = false;

			rc.TRSType = TRSType.Euler;
			rc.XCrusher.Enabled = false;
			rc.YCrusher.Enabled = false;
			rc.ZCrusher.Enabled = true;

			sc.uniformAxes = ElementCrusher.UniformAxes.NonUniform;
			sc.XCrusher.Enabled = true;
			sc.YCrusher.Enabled = false;
			sc.ZCrusher.Enabled = false;

			return st;
		}

		[MenuItem(SYNC_TRANS_FOLDER + "3D Position Only", false, AssistHelpers.PRIORITY)]
        public static SyncTransform Add3dPosOnly()
		{
			return Add3dPosOnly(Selection.activeTransform);
		}
		public static SyncTransform Add3dPosOnly(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var pc = tc.PosCrusher;

			pc.XCrusher.Enabled = true;
			pc.YCrusher.Enabled = true;
			pc.ZCrusher.Enabled = true;

			return st;
		}

		[MenuItem(SYNC_TRANS_FOLDER + "3D Rotation Only", false, AssistHelpers.PRIORITY)]
        public static SyncTransform Add3dRotOnly()
		{
			return Add3dRotOnly(Selection.activeTransform);
		}
		public static SyncTransform Add3dRotOnly(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var rc = tc.RotCrusher;

			rc.TRSType = TRSType.Quaternion;
			rc.QCrusher.Enabled = true;

			return st;
		}

		[MenuItem(SYNC_TRANS_FOLDER + "3D Euler Only", false, AssistHelpers.PRIORITY)]
        public static SyncTransform Add3dEulerOnly()
		{
			return Add3dEulerOnly(Selection.activeTransform);
		}
		public static SyncTransform Add3dEulerOnly(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var rc = tc.RotCrusher;

			rc.TRSType = TRSType.Euler;
			rc.XCrusher.Enabled = true;
			rc.YCrusher.Enabled = true;
			rc.ZCrusher.Enabled = true;

			return st;
		}


		[MenuItem(SYNC_TRANS_FOLDER + "2D Position Only", false, AssistHelpers.PRIORITY)]
        public static SyncTransform Add2dPosOnly()
		{
			return Add2dPosOnly(Selection.activeTransform);

		}

		public static SyncTransform Add2dPosOnly(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var pc = tc.PosCrusher;

			pc.XCrusher.Enabled = true;
			pc.YCrusher.Enabled = true;
			pc.ZCrusher.Enabled = false;

			return st;
		}


		[MenuItem(SYNC_TRANS_FOLDER + "3D Hands Pos", false, AssistHelpers.PRIORITY)]
        public static SyncTransform Add3DHandsPos()
		{
			return Add3DHandsPos(Selection.activeTransform);

		}

		public static SyncTransform Add3DHandsPos(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var pc = tc.PosCrusher;

			pc.local = true;
			pc.XCrusher.Enabled = true;
			pc.YCrusher.Enabled = true;
			pc.ZCrusher.Enabled = true;

			pc.XCrusher.SetRange(10, -2f, 2f);
			pc.YCrusher.SetRange(10, -2f, 2f);
			pc.ZCrusher.SetRange(10, -2f, 2f);

			return st;
		}

		public static SyncTransform Add3DHandsRot(this Transform t)
		{
			var st = AddDefaultDisabledSyncTransform(t);
			if (!st)
				return null;

			var tc = st.transformCrusher;
			var rc = tc.RotCrusher;

			rc.local = true;
			rc.TRSType = TRSType.Quaternion;
			rc.QCrusher.Enabled = true;

			rc.QCrusher.Bits = 44;

			return st;
		}
	}

}

#endif
