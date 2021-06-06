// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using emotitron.Compression;

using Photon.Pun;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable CS0618 // UNET obsolete

namespace emotitron.Utilities.Example
{
	/// <summary>
	/// A VERY basic player movement and position/rotation sync example using UNET.
	/// </summary>
	public class BasicController : MonoBehaviour
	{

		private PhotonView pv;

		private Rigidbody rb;
#if !DISABLE_PHYSICS_2D
		private Rigidbody2D rb2D;
#endif

		[HideInInspector]
		public TransformCrusher TransformCrusherRef;

		/// <summary>
		/// Reference to a transform crusher that we will use to constrain our movements.
		/// </summary>
		private TransformCrusher tc;

		public enum Timing { Auto, Fixed, Update, LateUpdate };
		public Timing timing = Timing.Fixed;
		public bool moveRelative = true;

		[Space]
		public KeyCode moveLeft = KeyCode.A;
		public KeyCode moveRight = KeyCode.D;
		public KeyCode moveFwd = KeyCode.W;
		public KeyCode moveBwd = KeyCode.S;
		public KeyCode moveUp = KeyCode.Space;
		public KeyCode moveDn = KeyCode.Z;

		[Space]
		public KeyCode pitchPos = KeyCode.R;
		public KeyCode pitchNeg = KeyCode.C;
		public KeyCode yawPos = KeyCode.E;
		public KeyCode yawNeg = KeyCode.Q;
		public KeyCode rollPos = KeyCode.Alpha4;
		public KeyCode rollNeg = KeyCode.Alpha4;

		[Space]
		public bool clampToCrusher = false;
		public float moveSpeed = 5f;
		public float turnSpeed = 60f;
		public float moveForce = 12f;
		public float turnForce = 100f;
		public float scaleSpeed = 1f;

		private bool isMine;

		void Awake()
		{
			rb = GetComponent<Rigidbody>();
#if !DISABLE_PHYSICS_2D
			rb2D = GetComponent<Rigidbody2D>();
#endif

#if PUN_2_OR_NEWER
			pv = GetComponent<PhotonView>();
#endif

		}

		private bool IsMine
		{
			get
			{
				return (pv == null || pv.IsMine);
			}
		}

		private void Start()
		{
			var iTC = GetComponent<IHasTransformCrusher>();
			if (iTC != null)
				tc = GetComponent<IHasTransformCrusher>().TC;

			if (!IsMine)
			{
				if (rb)
					rb.isKinematic = true;
#if !DISABLE_PHYSICS_2D
				if (rb2D)
					rb2D.isKinematic = true;
#endif
			}
		}

		void FixedUpdate()
		{
			if (timing == Timing.Fixed || (timing == Timing.Auto && rb))
				Apply();
		}

		void Update()
		{
			if (timing == Timing.Update || (timing == Timing.Auto && !rb))
				Apply();
		}

		void LateUpdate()
		{
			if (timing == Timing.LateUpdate)
				Apply();
		}

		private void SumKeys(out Vector3 move, out Vector3 turn)
		{

			move = new Vector3(0, 0, 0);

			if (Input.touchCount > 0)
			{
				var touchpos = Input.GetTouch(0).rawPosition;

				if (touchpos.x < Screen./*currentResolution.*/width * .333f)
					move.x -= 1;
				else if (touchpos.x > Screen./*currentResolution.*/width * .666f)
					move.x += 1;

				if (touchpos.y < Screen./*currentResolution.*/height * .333f)
					move.z -= 1;
				else if (touchpos.y > Screen./*currentResolution.*/height * .666f)
					move.z += 1;
			}

			//if (Input.GetTouch(0).rawPosition)

			if (Input.GetKey(moveRight))
				move.x += 1;
			if (Input.GetKey(moveLeft))
				move.x -= 1;

			if (Input.GetKey(moveUp))
				move.y += 1;
			if (Input.GetKey(moveDn))
				move.y -= 1;

			if (Input.GetKey(moveFwd))
				move.z += 1;
			if (Input.GetKey(moveBwd))
				move.z -= 1;

			move = Vector3.ClampMagnitude(move, 1);

			turn = new Vector3(0, 0, 0);

			if (Input.GetKey(pitchPos))
				turn.x += 1;
			if (Input.GetKey(pitchNeg))
				turn.x -= 1;

			if (Input.GetKey(yawPos))
				turn.y += 1;
			if (Input.GetKey(yawNeg))
				turn.y -= 1;

			if (Input.GetKey(rollPos))
				turn.z += 1;
			if (Input.GetKey(rollNeg))
				turn.z -= 1;



		}

		void Apply()
		{

			if (!IsMine)
				return;

			Vector3 move, turn;
			SumKeys(out move, out turn);

			/// POSITION

			if (rb && !rb.isKinematic)
			{

				/// Clamp the results of the previous Fixed
				/// This really doesn't belong here, but this is just quick and dirty sample code
				if (rb)
					if (clampToCrusher && tc != null)
						rb.MovePosition(tc.PosCrusher.Clamp(rb.position));

				move *= moveForce * Time.deltaTime;

				if (moveRelative)
					rb.AddRelativeForce(move, ForceMode.VelocityChange);
				else
					rb.AddForce(move, ForceMode.VelocityChange);

			}
#if !DISABLE_PHYSICS_2D
			else if (rb2D && !rb2D.isKinematic)
			{
				/// Clamp the results of the previous Fixed
				/// This really doesn't belong here, but this is just quick and dirty sample code
				if (rb2D)
					if (clampToCrusher && tc != null)
						rb2D.MovePosition(tc.PosCrusher.Clamp(rb2D.position));

				move *= moveForce * Time.deltaTime;

				if (moveRelative)
					rb2D.AddRelativeForce(move, ForceMode2D.Impulse);
				else
					rb2D.AddForce(move, ForceMode2D.Impulse);
			}
#endif
			else
			{

				Vector3 pos = (rb) ? rb.position : transform.position;

				if (moveRelative)
					pos += (transform.localRotation * move) * moveSpeed * Time.deltaTime;
				else
					pos += move * moveSpeed * Time.deltaTime;

				// If we have a reference to the transform crusher being used for compression, lets contrain our movements to our network limits.
				if (clampToCrusher && tc != null && tc.PosCrusher != null)
					pos = tc.PosCrusher.Clamp(pos);

				if (rb)
					//rb.position = pos;
					rb.MovePosition(pos);
				else
				{
					transform.position = pos;
				}
			}

			/// ROTATION

			if (rb && !rb.isKinematic)
			{
				turn *= turnForce * Time.deltaTime;
				rb.AddRelativeTorque(turn, ForceMode.VelocityChange);
			}
			else
			{

				if (clampToCrusher && tc != null && tc.RotCrusher.TRSType != TRSType.Quaternion)
				{
					Vector3 v = tc.RotCrusher.Clamp(transform.eulerAngles += turn * turnSpeed * Time.deltaTime);
					transform.localEulerAngles = v;
				}
				else
				{
					transform.rotation *= Quaternion.Euler(turn * turnSpeed * Time.deltaTime);
				}
			}
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(BasicController))]
	[CanEditMultipleObjects]

	public class BasicControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("This basic controller makes use of TransformCrusher.Clamp(), which allows it to restrict movement to the ranges of the crusher.", MessageType.None);
			base.OnInspectorGUI();
		}
	}

#endif

}

#pragma warning restore CS0618 // UNET obsolete
