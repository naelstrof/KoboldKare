// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using Photon.Utilities;
using emotitron.Compression;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{
	public interface IOnElementCrusherChange
	{
		void OnCrusherChange(ElementCrusher ec);
	}

	[System.Serializable]
	public class ElementCrusher : Crusher<ElementCrusher>, IEquatable<ElementCrusher>, ICrusherCopy<ElementCrusher>
	{
		public enum UniformAxes { NonUniform = 0, XY = 3, XZ = 5, YZ = 6, XYZ = 7 }
		public enum StaticTRSType
		{
			Position = 0,
			Euler = 1,
			Quaternion = 2,
			Scale = 3,
			Generic = 4
		}
		/// <summary>
		/// Experiemental collection of non-changing crushers, to avoid redundant crushers.
		/// </summary>
		public static Dictionary<int, ElementCrusher> staticElementCrushers = new Dictionary<int, ElementCrusher>();

		/// Temporary CompressedMatrix used internally when a non-alloc is not provided and no return CE required.
		private static readonly CompressedElement reusableCE = new CompressedElement();

		#region Static Crushers

		public static ElementCrusher GetStaticPositionCrusher(Bounds bounds, int resolution)
		{
			// Build a new EC with the supplied values.
			ElementCrusher ec = new ElementCrusher(StaticTRSType.Position)
			{
				XCrusher = FloatCrusher.GetStaticFloatCrusher(resolution, bounds.min.x, bounds.max.x, Axis.Generic, TRSType.Position),
				YCrusher = FloatCrusher.GetStaticFloatCrusher(resolution, bounds.min.y, bounds.max.y, Axis.Generic, TRSType.Position),
				ZCrusher = FloatCrusher.GetStaticFloatCrusher(resolution, bounds.min.z, bounds.max.z, Axis.Generic, TRSType.Position)
			};
			// See if this crusher is a repeat of an existing one
			return CheckAgainstStatics(ec);
		}

		public static ElementCrusher GetStaticQuatCrusher(int minBits)
		{
			// Build a new EC with the supplied values.
			ElementCrusher ec = new ElementCrusher(StaticTRSType.Quaternion)
			{
				QCrusher = new QuatCrusher(false, false) { Bits = minBits }
			};
			// See if this crusher is a repeat of an existing one
			return CheckAgainstStatics(ec);
		}

		/// <summary>
		/// Checks to see if a crusher with the same settings has been registered with our dictionary. If a crusher with the same value exists there, 
		/// that crusher is returned. If no duplicate of of the supplied crusher exists, the supplied crusher is returned.
		/// </summary>
		/// <param name="ec"></param>
		/// <returns></returns>
		public static ElementCrusher CheckAgainstStatics(ElementCrusher ec, bool CheckAgainstFloatCrushersAsWell = true)
		{
			if (ReferenceEquals(ec, null))
				return null;

			if (CheckAgainstFloatCrushersAsWell)
			{
				if (ec.cache_xEnabled)
					ec.XCrusher = FloatCrusher.CheckAgainstStatics(ec._xcrusher);
				if (ec.cache_yEnabled)
					ec.YCrusher = FloatCrusher.CheckAgainstStatics(ec._ycrusher);
				if (ec.cache_zEnabled)
					ec.ZCrusher = FloatCrusher.CheckAgainstStatics(ec._zcrusher);
				if (ec.cache_uEnabled)
					ec.UCrusher = FloatCrusher.CheckAgainstStatics(ec._ucrusher);
			}

			int hash = ec.GetHashCode();

			if (staticElementCrushers.ContainsKey(hash))
			{
				return staticElementCrushers[hash];
			}

			staticElementCrushers.Add(hash, ec);
			return ec;
		}

		public static ElementCrusher defaultUncompressedElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		{
			XCrusher = FloatCrusher.defaultUncompressedCrusher,
			YCrusher = FloatCrusher.defaultUncompressedCrusher,
			ZCrusher = FloatCrusher.defaultUncompressedCrusher,
			UCrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultUncompressedPosCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Position)
		{
			XCrusher = FloatCrusher.defaultUncompressedCrusher,
			YCrusher = FloatCrusher.defaultUncompressedCrusher,
			ZCrusher = FloatCrusher.defaultUncompressedCrusher,
			UCrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultUncompressedSclCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Position)
		{
			XCrusher = FloatCrusher.defaultUncompressedCrusher,
			YCrusher = FloatCrusher.defaultUncompressedCrusher,
			ZCrusher = FloatCrusher.defaultUncompressedCrusher,
			UCrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultHalfFloatElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		{
			XCrusher = FloatCrusher.defaultUncompressedCrusher,
			YCrusher = FloatCrusher.defaultUncompressedCrusher,
			ZCrusher = FloatCrusher.defaultUncompressedCrusher,
			UCrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultHalfFloatPosCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Position)
		{
			XCrusher = FloatCrusher.defaulHalfFloatCrusher,
			YCrusher = FloatCrusher.defaulHalfFloatCrusher,
			ZCrusher = FloatCrusher.defaulHalfFloatCrusher,
			UCrusher = FloatCrusher.defaulHalfFloatCrusher,
		});

		public static ElementCrusher defaultHalfFloatSclCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Scale)
		{
			XCrusher = FloatCrusher.defaulHalfFloatCrusher,
			YCrusher = FloatCrusher.defaulHalfFloatCrusher,
			ZCrusher = FloatCrusher.defaulHalfFloatCrusher,
			UCrusher = FloatCrusher.defaulHalfFloatCrusher,
		});

		///// <summary>
		///// Static Constructor
		///// </summary>
		//static ElementCrusher()
		//{
		//	ElementCrusher defaultUncompressedElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		//	{
		//		xcrusher = FloatCrusher.defaultUncompressedCrusher,
		//		ycrusher = FloatCrusher.defaultUncompressedCrusher,
		//		zcrusher = FloatCrusher.defaultUncompressedCrusher,
		//		ucrusher = FloatCrusher.defaultUncompressedCrusher,
		//	});
		//	ElementCrusher defaultHalfFloatElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		//	{
		//		xcrusher = FloatCrusher.defaulHalfFloatCrusher,
		//		ycrusher = FloatCrusher.defaulHalfFloatCrusher,
		//		zcrusher = FloatCrusher.defaulHalfFloatCrusher,
		//		ucrusher = FloatCrusher.defaulHalfFloatCrusher,
		//	});

		//	Debug.Log("STATIC CONS EC " + (ElementCrusher.defaultUncompressedElementCrusher != null) + " " 
		//		+ (ElementCrusher.defaultUncompressedElementCrusher.xcrusher != null));

		//}

		#endregion

		#region Inspector

#if UNITY_EDITOR
		public bool isExpanded = true;
#endif
		public bool hideFieldName = false;

		[SerializeField] private TRSType _trsType;
		public TRSType TRSType
		{
			get { return _trsType; }
			set
			{
				_trsType = value;
				_xcrusher.TRSType = value;
				_ycrusher.TRSType = value;
				_zcrusher.TRSType = value;
			}
		}

		[SerializeField] public Transform defaultTransform;
		[SerializeField] public UniformAxes uniformAxes;

		[SerializeField] private FloatCrusher _xcrusher;
		[SerializeField] private FloatCrusher _ycrusher;
		[SerializeField] private FloatCrusher _zcrusher;
		[SerializeField] private FloatCrusher _ucrusher;
		[SerializeField] private QuatCrusher _qcrusher;

		#region Crusher Properties

		[System.Obsolete("Use the XCrusher property instead.")]
		public FloatCrusher xcrusher { get { return XCrusher; } set { XCrusher = value; } }
		[System.Obsolete("Use the YCrusher property instead.")]
		public FloatCrusher ycrusher { get { return YCrusher; } set { YCrusher = value; } }
		[System.Obsolete("Use the ZCrusher property instead.")]
		public FloatCrusher zcrusher { get { return ZCrusher; } set { ZCrusher = value; } }
		[System.Obsolete("Use the UCrusher property instead.")]
		public FloatCrusher ucrusher { get { return UCrusher; } set { UCrusher = value; } }
		[System.Obsolete("Use the QCrusher property instead.")]
		public QuatCrusher qcrusher { get { return QCrusher; } set { QCrusher = value; } }

		public FloatCrusher XCrusher
		{
			get { return _xcrusher; }
			set
			{

				if (ReferenceEquals(_xcrusher, value))
					return;

				if (_xcrusher != null) _xcrusher.OnRecalculated -= OnCrusherChange;
				_xcrusher = value;
				if (_xcrusher != null) _xcrusher.OnRecalculated += OnCrusherChange;

				CacheValues();
			}
		}

		public FloatCrusher YCrusher
		{
			get { return _ycrusher; }
			set
			{
				if (ReferenceEquals(_ycrusher, value))
					return;

				if (_ycrusher != null) _ycrusher.OnRecalculated -= OnCrusherChange;
				_ycrusher = value;
				if (_ycrusher != null) _ycrusher.OnRecalculated += OnCrusherChange;

				CacheValues();
			}
		}
		public FloatCrusher ZCrusher
		{
			get { return _zcrusher; }
			set
			{
				if (ReferenceEquals(_zcrusher, value))
					return;

				if (_zcrusher != null) _zcrusher.OnRecalculated -= OnCrusherChange;
				_zcrusher = value;
				if (_zcrusher != null) _zcrusher.OnRecalculated += OnCrusherChange;

				CacheValues();
			}
		}
		public FloatCrusher UCrusher
		{
			get { return _ucrusher; }
			set
			{
				if (ReferenceEquals(_ucrusher, value))
					return;

				if (_ucrusher != null) _ucrusher.OnRecalculated -= OnCrusherChange;
				_ucrusher = value;
				if (_ucrusher != null) _ucrusher.OnRecalculated += OnCrusherChange;

				CacheValues();
			}
		}
		public QuatCrusher QCrusher
		{
			get { return _qcrusher; }
			set
			{
				if (ReferenceEquals(_qcrusher, value))
					return;

				if (_qcrusher != null) _qcrusher.OnRecalculated -= OnCrusherChange;
				_qcrusher = value;
				if (_qcrusher != null) _qcrusher.OnRecalculated += OnCrusherChange;

				CacheValues();
			}
		}

		public void OnCrusherChange(QuatCrusher crusher)
		{
			CacheValues();
		}

		public void OnCrusherChange(FloatCrusher crusher)
		{
			CacheValues();
		}

		#endregion

		[SerializeField] public bool local;
		[SerializeField] private bool useWorldBounds;
		[SerializeField]
		public bool UseWorldBounds
		{
			get { return useWorldBounds; }
			set
			{
				ApplyWorldCrusherSettings(value, boundsGroupId);
			}
		}

		[WorldBoundsSelectAttribute]
		[HideInInspector]
		[SerializeField]
		private int boundsGroupId;

		public int BoundsGroupId
		{
			get { return boundsGroupId; }
			set
			{
				ApplyWorldCrusherSettings(useWorldBounds, value);
			}
		}

		/// <summary>
		/// Handle callback from WorldBoundsSO letting this crusher know that it can now use the WBSO crusher references
		/// </summary>
		private void OnWorldBoundsReady()
		{
			ApplyWorldCrusherSettings();
			CacheValues();
		}

		/// <summary>
		/// Initial crusher references to match the WorldBoundsGroup specified by the current settings.
		/// </summary>
		public void ApplyWorldCrusherSettings()
		{

			if (useWorldBounds)
			{

				/// IF the WorldBoundsSO is not ready, stop. This method will be recalled when WorldBoundsSO is ready.
				if (WorldBoundsSettings.single == null)
				{
					WorldBoundsSettings.OnSingletonReady -= OnWorldBoundsReady;
					WorldBoundsSettings.OnSingletonReady += OnWorldBoundsReady;
					return;
				}

				WorldBoundsSettings.OnSingletonReady -= OnWorldBoundsReady;

				var wbgs = WorldBoundsSettings.single.worldBoundsGroups;

				// If the bounds group is no longer valid, revert to default (0)
				if (boundsGroupId >= wbgs.Count)
				{
					Debug.LogError("WorldBoundsGroup " + boundsGroupId + " no longer exists. Using Default(0).");
					boundsGroupId = 0;
				}

				var wbg = wbgs[boundsGroupId];

				wbg.OnWorldBoundChanged -= CacheValues;
				wbg.OnWorldBoundChanged += CacheValues;

				var wbgc = wbg.crusher;

				if (_xcrusher != wbgc._xcrusher)
					XCrusher = wbgc.XCrusher;

				if (_ycrusher != wbgc._ycrusher)
					YCrusher = wbgc.YCrusher;

				if (_zcrusher != wbgc._zcrusher)
					ZCrusher = wbgc.ZCrusher;

				local = wbgc.local;
			}

		}

		/// <summary>
		/// Swap the crusher references to match the WorldBoundsGroup specified by the current settings.
		/// </summary>
		public void ApplyWorldCrusherSettings(bool newUseBounds, int newBndsGrpId)
		{
			if (newUseBounds != useWorldBounds)
			{
				useWorldBounds = newUseBounds;
				if (!useWorldBounds)
				{
					Defaults(TRSType.Position);
				}
			}

			if (WorldBoundsSettings.single == null)
			{
				Debug.LogWarning("Not Ready to Change the World");
				return;
			}
			var wbgs = WorldBoundsSettings.single.worldBoundsGroups;

			/// If BoundsGroup ID has changed, change the event subscribe to that group and update the local fields.
			if (newBndsGrpId != boundsGroupId)
			{
				// If the previous group still exists, be sure we are no longer subscribed to its OnChange events
				if (boundsGroupId < wbgs.Count)
				{
					var prvWBG = wbgs[boundsGroupId];

					if (prvWBG != null)
						prvWBG.OnWorldBoundChanged -= CacheValues;
				}

				// If this is no longer a existing worldbounds layer, reset to default
				if (newBndsGrpId >= wbgs.Count)
					boundsGroupId = 0;
				else
					boundsGroupId = newBndsGrpId;

				var newWBG = wbgs[boundsGroupId];

				if (newWBG != null && useWorldBounds)
				{
					newWBG.OnWorldBoundChanged += CacheValues;

					var wbgc = newWBG.crusher;

					if (_xcrusher != wbgc._xcrusher)
						XCrusher = wbgc.XCrusher;

					if (_ycrusher != wbgc._ycrusher)
						YCrusher = wbgc.YCrusher;

					if (_zcrusher != wbgc._zcrusher)
						ZCrusher = wbgc.ZCrusher;

					local = wbgc.local;
				}
			}
		}

		private WorldBoundsGroup GetUsedWorldBounds()
		{
			if (_trsType == TRSType.Position && useWorldBounds)
			{
				// If this is no longer a existing worldbounds layer, reset to default
				if (boundsGroupId >= WorldBoundsSettings.Single.worldBoundsGroups.Count)
					boundsGroupId = 0;

				var wbs = WorldBoundsSettings.Single.worldBoundsGroups[boundsGroupId];
				return wbs;
			}
			else return null;
		}

		[SerializeField] public bool enableTRSTypeSelector;
		[SerializeField] public bool enableLocalSelector = true;

		#endregion

		#region Cached values

		// cache values
		[System.NonSerialized]
		private bool cached;

		[System.NonSerialized] private bool cache_xEnabled, cache_yEnabled, cache_zEnabled, cache_uEnabled, cache_qEnabled;
		[System.NonSerialized] private bool cache_isUniformScale;

		[System.NonSerialized] private readonly int[] cache_xBits = new int[4];
		[System.NonSerialized] private readonly int[] cache_yBits = new int[4];
		[System.NonSerialized] private readonly int[] cache_zBits = new int[4];
		[System.NonSerialized] private readonly int[] cache_uBits = new int[4];
		[System.NonSerialized] private readonly int[] cache_TotalBits = new int[4];

		public ReadOnlyCollection<int> Cached_TotalBits;

		[System.NonSerialized] private int cache_qBits;
		[System.NonSerialized] private bool cache_mustCorrectRotationX;

		//public ReadOnlyCollection<int> Cached_TotalBits { get { return Array.AsReadOnly(cache_TotalBits); } }

		public Bounds bounds = new Bounds();

		/// <summary>
		/// Get will return a Bounds struct with the ranges of the x/y/z crushers. Set will set the x/y/z crusher to match those of the Bounds value.
		/// </summary>
		public Bounds Bounds
		{
			get
			{
				bounds.SetMinMax(
					new Vector3(
						(_xcrusher != null) ? _xcrusher.Min : 0,
						(_ycrusher != null) ? _ycrusher.Min : 0,
						(_zcrusher != null) ? _zcrusher.Min : 0),
					new Vector3(
						(_xcrusher != null) ? _xcrusher.Max : 0,
						(_ycrusher != null) ? _ycrusher.Max : 0,
						(_zcrusher != null) ? _zcrusher.Max : 0)
					);

				return bounds;
			}

			set
			{
				if (_xcrusher != null)
					_xcrusher.SetRange(value.min.x, value.max.x);
				if (_ycrusher != null)
					_ycrusher.SetRange(value.min.y, value.max.y);
				if (_zcrusher != null)
					_zcrusher.SetRange(value.min.z, value.max.z);

				CacheValues();
			}
		}

		public override void OnBeforeSerialize() { }

		public override void OnAfterDeserialize()
		{
			//ApplyWorldCrusherSettings();
			//CacheValues();
		}

		public void CacheValues()
		{
			ApplyWorldCrusherSettings();

#if !UNITY_EDITOR
			NullUnusedCrushers();
#endif

			if (_trsType == TRSType.Quaternion)
			{

				cache_qEnabled = (_qcrusher != null) && _qcrusher.Enabled && _qcrusher.Bits > 0;
				cache_qBits = (cache_qEnabled) ? _qcrusher.Bits : 0;

				cache_TotalBits[0] = cache_qBits;
				cache_TotalBits[1] = cache_qBits;
				cache_TotalBits[2] = cache_qBits;
				cache_TotalBits[3] = cache_qBits;
				cache_isUniformScale = false;

			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				cache_uEnabled = (_ucrusher != null) && _ucrusher.Enabled;

				for (int i = 0; i < 4; ++i)
				{
					int bits = (cache_uEnabled) ? _ucrusher.GetBits((BitCullingLevel)i) : 0;
					cache_uBits[i] = bits;
					cache_TotalBits[i] = bits;
				}
				cache_isUniformScale = true;
			}

			else
			{
				cache_mustCorrectRotationX = _trsType == TRSType.Euler && _xcrusher.UseHalfRangeX;

				for (int i = 0; i < 4; ++i)
				{
					cache_xEnabled = (_xcrusher != null) && _xcrusher.Enabled;
					cache_yEnabled = (_ycrusher != null) && _ycrusher.Enabled;
					cache_zEnabled = (_zcrusher != null) && _zcrusher.Enabled;

					cache_xBits[i] = (cache_xEnabled) ? _xcrusher.GetBits((BitCullingLevel)i) : 0;
					cache_yBits[i] = (cache_yEnabled) ? _ycrusher.GetBits((BitCullingLevel)i) : 0;
					cache_zBits[i] = (cache_zEnabled) ? _zcrusher.GetBits((BitCullingLevel)i) : 0;
					cache_TotalBits[i] = cache_xBits[i] + cache_yBits[i] + cache_zBits[i];

					cache_isUniformScale = false;
				}
			}

			Cached_TotalBits = Array.AsReadOnly(cache_TotalBits);

			// TODO: Cached may no longer be needed. Bootstraps may now ensure it is cached propertly 100% of the time.
			cached = true;

			/// Trigger OnChange callback
			if (OnRecalculated != null)
				OnRecalculated(this);
		}

		/// <summary>
		/// Called at startup of builds to clear any crushers that were stored for the editor, but are not used in builds. Generates some startup GC, but the alternative is breaking
		/// unused crushers in SOs in the editor every time a build is made.
		/// </summary>
		private void NullUnusedCrushers()
		{
			if (_trsType == TRSType.Quaternion)
			{
				XCrusher = null;
				YCrusher = null;
				ZCrusher = null;
				UCrusher = null;
			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				XCrusher = null;
				YCrusher = null;
				ZCrusher = null;
				QCrusher = null;
			}
			else
			{
				QCrusher = null;
				UCrusher = null;
			}
		}

		/// <summary>
		/// Property that returns if this element crusher is effectively enabled (has any enabled float/quat crushers using bits > 0)
		/// </summary>
		public bool Enabled
		{
			get
			{
				if (TRSType == TRSType.Quaternion)
					return (_qcrusher.Enabled && _qcrusher.Bits > 0);

				else if (TRSType == TRSType.Scale && uniformAxes != 0)
					return _ucrusher.Enabled;

				return _xcrusher.Enabled | _ycrusher.Enabled | _zcrusher.Enabled;
			}
			set
			{
				if (TRSType == TRSType.Quaternion)
					_qcrusher.Enabled = value;

				else if (TRSType == TRSType.Scale && uniformAxes != 0)
					_ucrusher.Enabled = value;

				_xcrusher.Enabled = value;
				_ycrusher.Enabled = value;
				_zcrusher.Enabled = value;
				//CacheValues();
			}
		}

		#endregion

		#region Indexer

		/// <summary>
		/// Indexer returns the component crushers.
		/// </summary>
		/// <param name="axis"></param>
		/// <returns></returns>
		public FloatCrusher this[int axis]
		{
			get
			{
				switch (axis)
				{
					case 0:
						return _xcrusher;
					case 1:
						return _ycrusher;
					case 2:
						return _zcrusher;

					default:
						Debug.Log("AXIS " + axis + " should not be calling happening");
						return null;
				}
			}
		}

		#endregion

		#region Constructors

		public ElementCrusher()
		{
			Defaults(TRSType.Generic);
		}

		/// <summary>
		/// Static crushers don't initialize unused crusher. Using this constructor will leave unused crushers null, since these types are not meant
		/// to be shown in the inspector.
		/// </summary>
		/// <param name="staticTrsType"></param>
		internal ElementCrusher(StaticTRSType staticTrsType)
		{
			_trsType = (TRSType)staticTrsType;
		}

		// Constructor
		public ElementCrusher(bool enableTRSTypeSelector = true)
		{
			this._trsType = TRSType.Generic;
			Defaults(TRSType.Generic);

			this.enableTRSTypeSelector = enableTRSTypeSelector;
		}

		// Constructor
		public ElementCrusher(TRSType trsType, bool enableTRSTypeSelector = true)
		{
			this._trsType = trsType;
			Defaults(trsType);

			this.enableTRSTypeSelector = enableTRSTypeSelector;
		}

		public void Defaults(TRSType trs)
		{
			if (trs == TRSType.Quaternion || trs == TRSType.Euler)
			{
				XCrusher = new FloatCrusher(BitPresets.Bits10, -90f, 90f, Axis.X, TRSType.Euler, true);
				YCrusher = new FloatCrusher(BitPresets.Bits12, -180f, 180f, Axis.Y, TRSType.Euler, true);
				ZCrusher = new FloatCrusher(BitPresets.Bits10, -180f, 180f, Axis.Z, TRSType.Euler, true);
				//ucrusher = new FloatCrusher(Axis.Uniform, TRSType.Scale, true);
				QCrusher = new QuatCrusher(true, false);
			}
			else if (trs == TRSType.Scale)
			{
				XCrusher = new FloatCrusher(BitPresets.Bits12, 0f, 2f, Axis.X, TRSType.Scale, true);
				YCrusher = new FloatCrusher(BitPresets.Bits10, 0f, 2f, Axis.Y, TRSType.Scale, true);
				ZCrusher = new FloatCrusher(BitPresets.Bits10, 0f, 2f, Axis.Z, TRSType.Scale, true);
				UCrusher = new FloatCrusher(BitPresets.Bits10, 0f, 2f, Axis.Uniform, TRSType.Scale, true);
			}
			else
			{
				XCrusher = new FloatCrusher(BitPresets.Bits12, -20f, 20f, Axis.X, trs, true);
				YCrusher = new FloatCrusher(BitPresets.Bits10, -5f, 5f, Axis.Y, trs, true);
				ZCrusher = new FloatCrusher(BitPresets.Bits10, -5f, 5f, Axis.Z, trs, true);

			}
		}

		#endregion

		#region Byte[] Writers

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private void TryingToWriteQuatAsEulerError()
		{
			if (TRSType != TRSType.Quaternion)
				Debug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
					System.Enum.GetName(typeof(TRSType), TRSType) + ".");
		}

		private void TryingToCrushTransformUsingGenericWarning()
		{
			Debug.Log("You are sending a transform to be crushed, but the Element Type is Generic - did you want Position? Ideally change the crusher from Generic to the correct TRS.");
		}
#endif

		/// <summary>
		/// Automatically use the correct transform TRS element based on the TRSType and local settings of each Crusher.
		/// </summary>
		public void Write(CompressedElement nonalloc, Transform trans, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{

			switch (TRSType)
			{

				case TRSType.Position:
					Write(nonalloc, (local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles, bytes, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write(nonalloc, (local) ? trans.localRotation : trans.rotation, bytes, ref bitposition, bcl);
					return;

				case TRSType.Scale:
					Write(nonalloc, trans.localScale, bytes, ref bitposition, bcl);
					return;

				default:
#if UNITY_EDITOR
					TryingToCrushTransformUsingGenericWarning();
#endif
					Write(nonalloc, (local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);
					return;
			}
		}
		[System.Obsolete()]
		public CompressedElement Write(Transform trans, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					return Write((local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);

				case TRSType.Euler:
					return Write((local) ? trans.localEulerAngles : trans.eulerAngles, bytes, ref bitposition, bcl);

				case TRSType.Quaternion:
					return Write((local) ? trans.localRotation : trans.rotation, bytes, ref bitposition, bcl);

				case TRSType.Scale:
					return Write(trans.localScale, bytes, ref bitposition, bcl);

				default:
#if UNITY_EDITOR
					TryingToCrushTransformUsingGenericWarning();
#endif
					return Write((local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);
			}
		}

		/// <summary>
		/// Serialize a CompressedElement into a byte[] buffer.
		/// </summary>
		public void Write(CompressedElement ce, byte[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				_qcrusher.Write((ulong)ce.cQuat, buffer, ref bitposition);
			}
			else if (cache_isUniformScale)
			{
				_ucrusher.Write(ce.cUniform, buffer, ref bitposition, bcl);
			}
			else
			{
				if (cache_xEnabled && ((int)ia & 1) != 0) _xcrusher.Write(ce.cx, buffer, ref bitposition, bcl);
				if (cache_yEnabled && ((int)ia & 2) != 0) _ycrusher.Write(ce.cy, buffer, ref bitposition, bcl);
				if (cache_zEnabled && ((int)ia & 4) != 0) _zcrusher.Write(ce.cz, buffer, ref bitposition, bcl);
			}
		}

		/// <summary>
		/// Serialize a CompressedElement into a uint[] buffer.
		/// </summary>
		public void Write(CompressedElement ce, uint[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				_qcrusher.Write((ulong)ce.cQuat, buffer, ref bitposition);
			}
			else if (cache_isUniformScale)
			{
				_ucrusher.Write(ce.cUniform, buffer, ref bitposition, bcl);
			}
			else
			{
				if (cache_xEnabled && ((int)ia & 1) != 0) _xcrusher.Write(ce.cx, buffer, ref bitposition, bcl);
				if (cache_yEnabled && ((int)ia & 2) != 0) _ycrusher.Write(ce.cy, buffer, ref bitposition, bcl);
				if (cache_zEnabled && ((int)ia & 4) != 0) _zcrusher.Write(ce.cz, buffer, ref bitposition, bcl);
			}
		}

		/// <summary>
		/// Serialize a CompressedElement into a ulong[] buffer.
		/// </summary>
		public void Write(CompressedElement ce, ulong[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				_qcrusher.Write((ulong)ce.cQuat, buffer, ref bitposition);
			}
			else if (cache_isUniformScale)
			{
				_ucrusher.Write(ce.cUniform, buffer, ref bitposition, bcl);
			}
			else
			{
				if (cache_xEnabled && ((int)ia & 1) != 0) _xcrusher.Write(ce.cx, buffer, ref bitposition, bcl);
				if (cache_yEnabled && ((int)ia & 2) != 0) _ycrusher.Write(ce.cy, buffer, ref bitposition, bcl);
				if (cache_zEnabled && ((int)ia & 4) != 0) _zcrusher.Write(ce.cz, buffer, ref bitposition, bcl);
			}
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, byte[] bytes, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Write(nonalloc, v3, bytes, ref bitposition);
		}
		[System.Obsolete()]
		public CompressedElement Write(Vector3 v3, byte[] bytes, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return Write(v3, bytes, ref bitposition);
		}

		/// <summary>
		/// Compress and then write a vector3 value into a byte[] buffer.
		/// </summary>
		/// <param name="nonalloc">Overwrite this CompressedElement with the compressed value.</param>
		/// <param name="v3">The uncompressed value to compress and serialize.</param>
		/// <param name="bytes">The target buffer.</param>
		/// <param name="bitposition">Write position of the target buffer.</param>
		/// <param name="bcl"></param>
		public void Write(CompressedElement nonalloc, Vector3 v3, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_isUniformScale)
			{
				CompressedFloat c = _ucrusher.Write(uniformAxes == UniformAxes.YZ ? v3.y : v3.x, bytes, ref bitposition, bcl);
				nonalloc.Set(this, (uint)c.cvalue);
			}

			else if (TRSType == TRSType.Quaternion)
			{
				ulong c = _qcrusher.Write(Quaternion.Euler(v3), bytes, ref bitposition);
				nonalloc.Set(this, c);
			}
			else
			{
				if (cache_mustCorrectRotationX)
					v3 = FloatCrusherUtilities.GetXCorrectedEuler(v3);

				nonalloc.Set(
					this,
					cache_xEnabled ? _xcrusher.Write(v3.x, bytes, ref bitposition, bcl) : new CompressedFloat(),
					cache_yEnabled ? _ycrusher.Write(v3.y, bytes, ref bitposition, bcl) : new CompressedFloat(),
					cache_zEnabled ? _zcrusher.Write(v3.z, bytes, ref bitposition, bcl) : new CompressedFloat());
			}
		}
		[System.Obsolete()]
		public CompressedElement Write(Vector3 v3, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_isUniformScale)
			{
				CompressedValue c = _ucrusher.Write(uniformAxes == UniformAxes.YZ ? v3.y : v3.x, bytes, ref bitposition, bcl);
				return new CompressedElement(this, (uint)c.cvalue);
			}

			else if (TRSType == TRSType.Quaternion)
			{
				ulong c = _qcrusher.Write(Quaternion.Euler(v3), bytes, ref bitposition);
				return new CompressedElement(this, c);
			}

			else if (cache_mustCorrectRotationX)
				v3 = FloatCrusherUtilities.GetXCorrectedEuler(v3);

			return new CompressedElement(
				this,
				cache_xEnabled ? (uint)_xcrusher.Write(v3.x, bytes, ref bitposition, bcl) : 0,
				cache_yEnabled ? (uint)_ycrusher.Write(v3.y, bytes, ref bitposition, bcl) : 0,
				cache_zEnabled ? (uint)_zcrusher.Write(v3.z, bytes, ref bitposition, bcl) : 0);
		}

		public void Write(CompressedElement nonalloc, Quaternion quat, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			TryingToWriteQuatAsEulerError();
#endif
			nonalloc.Set(this, _qcrusher.Write(quat, bytes, ref bitposition));
		}
		[System.Obsolete()]
		public CompressedElement Write(Quaternion quat, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			TryingToWriteQuatAsEulerError();
#endif
			return new CompressedElement(this, _qcrusher.Write(quat, bytes, ref bitposition));
		}

		#endregion

		#region ulong[] Writers

		public void Write(CompressedElement nonalloc, Transform trans, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:

					Write(nonalloc, (local) ? trans.localPosition : trans.position, buffer, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles, buffer, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write(nonalloc, (local) ? trans.localRotation : trans.rotation, buffer, ref bitposition, bcl);
					return;

				case TRSType.Scale:
					Write(nonalloc, trans.localScale, buffer, ref bitposition, bcl);
					return;

				default:
#if UNITY_EDITOR
					Debug.Log("You are sending a transform to be crushed, but the Element Type is Generic - did you want Position?");
#endif
					Write(nonalloc, (local) ? trans.localPosition : trans.position, buffer, ref bitposition, bcl);
					return;
			}
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_isUniformScale)
			{
				CompressedFloat c = _ucrusher.Write(uniformAxes == UniformAxes.YZ ? v3.y : v3.x, buffer, ref bitposition, bcl);
				nonalloc.Set(this, (uint)c.cvalue);
			}

			else if (TRSType == TRSType.Quaternion)
			{
				ulong c = _qcrusher.Write(Quaternion.Euler(v3), buffer, ref bitposition);
				nonalloc.Set(this, c);
			}
			else
			{
				if (cache_mustCorrectRotationX)
					v3 = FloatCrusherUtilities.GetXCorrectedEuler(v3);

				nonalloc.Set(
					this,
					cache_xEnabled ? _xcrusher.Write(v3.x, buffer, ref bitposition, bcl) : new CompressedFloat(),
					cache_yEnabled ? _ycrusher.Write(v3.y, buffer, ref bitposition, bcl) : new CompressedFloat(),
					cache_zEnabled ? _zcrusher.Write(v3.z, buffer, ref bitposition, bcl) : new CompressedFloat());
			}
		}

		public void Write(CompressedElement nonalloc, Quaternion quat, ulong[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (TRSType != TRSType.Quaternion)
				Debug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
					System.Enum.GetName(typeof(TRSType), TRSType) + ".");
#endif
			nonalloc.Set(this, _qcrusher.Write(quat, bytes, ref bitposition));
		}

		#endregion

		#region byte[] Readers

		/// <summary>
		/// Read CompressedElement from buffer.
		/// </summary>
		public void Read(CompressedElement nonalloc, byte[] bytes, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(nonalloc, bytes, ref bitposition, ia, bcl);
		}

		/// <summary>
		/// Read CompressedElement from buffer.
		/// <para>WARNING: Returned CompressedElement is a recycled class and values should be used immediately.</para>
		/// </summary>
		public CompressedElement Read(byte[] buffer, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(CompressedElement.reusable, buffer, ref bitposition, ia, bcl);
			return CompressedElement.reusable;
		}

		/// <summary>
		/// Reads out the commpressed value for this vector/quaternion from a buffer. Needs to be decompressed still to become vector3/quaterion.
		/// </summary>
		public void Read(CompressedElement nonalloc, byte[] bytes, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, (ulong)bytes.Read(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)bytes.Read(ref bitposition, cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx, cy, cz;
				if ((ia & IncludedAxes.X) != 0)
				{
					int xbits = cache_xBits[(int)bcl];
					cx = cache_xEnabled ? new CompressedFloat(_xcrusher, (uint)bytes.Read(ref bitposition, xbits)) : new CompressedFloat();
				}
				else cx = new CompressedFloat();

				if ((ia & IncludedAxes.Y) != 0)
				{
					int ybits = cache_yBits[(int)bcl];
					cy = cache_yEnabled ? new CompressedFloat(_ycrusher, (uint)bytes.Read(ref bitposition, ybits)) : new CompressedFloat();

				}
				else cy = new CompressedFloat();

				if ((ia & IncludedAxes.Z) != 0)
				{
					int zbits = cache_zBits[(int)bcl];
					cz = cache_zEnabled ? new CompressedFloat(_zcrusher, (uint)bytes.Read(ref bitposition, zbits)) : new CompressedFloat();
				}
				else cz = new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}

		}

		/// <summary>
		/// Deserialize a compressed Vector/Quaternion and return as a CompressedElement.
		/// <para>WARNING: The returned CompressedElement is recycled and its values need to be used immediately.</para>
		/// <para>Supply a pre-allocated CompressedElement as an argument if you intend to store these values.</para>
		/// </summary>
		public CompressedElement Read(byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(CompressedElement.reusable, bytes, ref bitposition);
			return CompressedElement.reusable;
		}

		/// <summary>
		/// Reads out the commpressed value for this vector/quaternion from a buffer. Needs to be decompressed still to become vector3/quaterion.
		/// </summary>
		public void Read(CompressedElement nonalloc, ulong[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, (ulong)buffer.Read(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)buffer.Read(ref bitposition, cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx, cy, cz;
				if ((ia & IncludedAxes.X) != 0)
				{
					int xbits = cache_xBits[(int)bcl];
					cx = cache_xEnabled ? new CompressedFloat(_xcrusher, (uint)buffer.Read(ref bitposition, xbits)) : new CompressedFloat();
				}
				else cx = new CompressedFloat();

				if ((ia & IncludedAxes.Y) != 0)
				{
					int ybits = cache_yBits[(int)bcl];
					cy = cache_yEnabled ? new CompressedFloat(_ycrusher, (uint)buffer.Read(ref bitposition, ybits)) : new CompressedFloat();

				}
				else cy = new CompressedFloat();

				if ((ia & IncludedAxes.Z) != 0)
				{
					int zbits = cache_zBits[(int)bcl];
					cz = cache_zEnabled ? new CompressedFloat(_zcrusher, (uint)buffer.Read(ref bitposition, zbits)) : new CompressedFloat();
				}
				else cz = new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}
		}

		/// <summary>
		/// Reads out the commpressed value for this vector/quaternion from a buffer. Needs to be decompressed still to become vector3/quaterion.
		/// </summary>
		public void Read(CompressedElement nonalloc, uint[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, (ulong)buffer.Read(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)buffer.Read(ref bitposition, cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx, cy, cz;
				if ((ia & IncludedAxes.X) != 0)
				{
					int xbits = cache_xBits[(int)bcl];
					cx = cache_xEnabled ? new CompressedFloat(_xcrusher, (uint)buffer.Read(ref bitposition, xbits)) : new CompressedFloat();
				}
				else cx = new CompressedFloat();

				if ((ia & IncludedAxes.Y) != 0)
				{
					int ybits = cache_yBits[(int)bcl];
					cy = cache_yEnabled ? new CompressedFloat(_ycrusher, (uint)buffer.Read(ref bitposition, ybits)) : new CompressedFloat();

				}
				else cy = new CompressedFloat();

				if ((ia & IncludedAxes.Z) != 0)
				{
					int zbits = cache_zBits[(int)bcl];
					cz = cache_zEnabled ? new CompressedFloat(_zcrusher, (uint)buffer.Read(ref bitposition, zbits)) : new CompressedFloat();
				}
				else cz = new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}
		}


		public Element ReadAndDecompress(byte[] bytes, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCE, bytes, ref bitposition, ia, bcl);
			return Decompress(reusableCE);
		}

		#endregion

		#region ULong Buffer Writers

		/// <summary>
		/// Automatically use the correct transform element based on the TRSType for this Crusher.
		/// </summary>
		public void Write(CompressedElement nonalloc, Transform trans, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					Write(nonalloc, (local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write(nonalloc, (local) ? trans.localRotation : trans.rotation, ref buffer, ref bitposition);
					return;

				case TRSType.Scale:
					Write(nonalloc, trans.localScale, ref buffer, ref bitposition, bcl);
					return;

				default:
#if UNITY_EDITOR
					TryingToCrushTransformUsingGenericWarning();
#endif
					Write(nonalloc, (local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;
			}
		}
		public void Write(Transform trans, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					Write((local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write((local) ? trans.localEulerAngles : trans.eulerAngles, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write((local) ? trans.localRotation : trans.rotation, ref buffer, ref bitposition);
					return;

				case TRSType.Scale:
					Write(trans.localScale, ref buffer, ref bitposition, bcl);
					return;

				default:
#if UNITY_EDITOR
					TryingToCrushTransformUsingGenericWarning();
#endif
					Write((local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;
			}
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, ref ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Write(nonalloc, v3, ref buffer, ref bitposition, bcl);
		}
		public void Write(Vector3 v3, ref ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Write(v3, ref buffer, ref bitposition, bcl);
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			CompressedFloat cx = cache_xEnabled ? _xcrusher.Write(v3.x, ref buffer, ref bitposition, bcl) : new CompressedFloat();
			CompressedFloat cy = cache_yEnabled ? _ycrusher.Write(v3.y, ref buffer, ref bitposition, bcl) : new CompressedFloat();
			CompressedFloat cz = cache_zEnabled ? _zcrusher.Write(v3.z, ref buffer, ref bitposition, bcl) : new CompressedFloat();

			nonalloc.Set(this, cx, cy, cz);
		}
		public void Write(Vector3 v3, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_xEnabled) _xcrusher.Write(v3.x, ref buffer, ref bitposition, bcl);
			if (cache_yEnabled) _ycrusher.Write(v3.y, ref buffer, ref bitposition, bcl);
			if (cache_zEnabled) _zcrusher.Write(v3.z, ref buffer, ref bitposition, bcl);

		}

		public void Write(CompressedElement nonalloc, Quaternion quat, ref ulong buffer)
		{
			int bitposition = 0;
			Write(nonalloc, quat, ref buffer, ref bitposition);
		}
		public void Write(Quaternion quat, ref ulong buffer)
		{
			int bitposition = 0;
			Write(quat, ref buffer, ref bitposition);
		}

		public void Write(CompressedElement nonalloc, Quaternion quat, ref ulong buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			ulong cq = cache_qEnabled ? _qcrusher.Write(quat, ref buffer, ref bitposition) : 0;

			nonalloc.Set(this, cq);
		}
		public void Write(Quaternion quat, ref ulong buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			if (cache_qEnabled) _qcrusher.Write(quat, ref buffer, ref bitposition);
		}

		public CompressedElement Write(CompressedElement ce, ref ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return Write(ce, ref buffer, ref bitposition, bcl);
		}
		public CompressedElement Write(CompressedElement ce, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_qEnabled)
				ce.cQuat.cvalue.Inject(ref buffer, ref bitposition, cache_qBits);
			else if (cache_uEnabled)
				ce.cUniform.cvalue.Inject(ref buffer, ref bitposition, cache_uBits[(int)bcl]);
			else
			{
				if (cache_xEnabled)
					ce.cx.cvalue.Inject(ref buffer, ref bitposition, cache_xBits[(int)bcl]);
				if (cache_yEnabled)
					ce.cy.cvalue.Inject(ref buffer, ref bitposition, cache_yBits[(int)bcl]);
				if (cache_zEnabled)
					ce.cz.cvalue.Inject(ref buffer, ref bitposition, cache_zBits[(int)bcl]);

			}
			return ce;
		}

		#endregion

		#region ULong Buffer Readers


		public Element Read(ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();
			int bitposition = 0;

			if (TRSType == TRSType.Quaternion)
			{
				ulong c = buffer.Read(ref bitposition, cache_qBits);
				return _qcrusher.Decompress(c);
			}

			else if (cache_isUniformScale)
			{
				float val = _ucrusher.ReadAndDecompress(buffer, ref bitposition, bcl);
				return new Vector3(val, val, val);
			}
			else
			{
				return new Vector3(

				cache_xEnabled ? _xcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_yEnabled ? _ycrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_zEnabled ? _zcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f
				);
			}

		}

		/// <summary>
		/// Deserialize a compressed element directly from the buffer stream into a vector3/quaternion. This is the most efficient read, but
		/// it does not return any intermediary compressed values.
		/// </summary>
		/// <param name="buffer">Serialized source.</param>
		/// <param name="bitposition">Current read position in source.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public Element Read(ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				ulong val = buffer.Read(ref bitposition, cache_qBits);
				return _qcrusher.Decompress(val);
			}

			else if (cache_isUniformScale)
			{
				float f = _ucrusher.ReadAndDecompress(buffer, ref bitposition, bcl);
				return new Vector3(f, f, f);
			}

			return new Vector3(
				cache_xEnabled ? _xcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_yEnabled ? _ycrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_zEnabled ? _zcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f
				);
		}

		public void Read(CompressedElement nonalloc, ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(nonalloc, buffer, ref bitposition, bcl);
		}
		public void Read(CompressedElement nonalloc, ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, (ulong)buffer.Read(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)buffer.Read(ref bitposition, cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx = cache_xEnabled ? new CompressedFloat(_xcrusher, buffer.Read(ref bitposition, cache_xBits[(int)bcl])) : new CompressedFloat();
				CompressedFloat cy = cache_yEnabled ? new CompressedFloat(_ycrusher, buffer.Read(ref bitposition, cache_yBits[(int)bcl])) : new CompressedFloat();
				CompressedFloat cz = cache_zEnabled ? new CompressedFloat(_zcrusher, buffer.Read(ref bitposition, cache_zBits[(int)bcl])) : new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}
		}

		#endregion

		#region Fragment Readers

		public static ulong[] reusableArray64 = new ulong[2];

		public void Read(CompressedElement nonalloc, ulong frag0, ulong frag1 = 0, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			int bitposition = 0;
			reusableArray64.Write(frag0, ref bitposition, 64);
			reusableArray64.Write(frag1, ref bitposition, 64);

			bitposition = 0;
			Read(nonalloc, reusableArray64, ref bitposition, IncludedAxes.XYZ, bcl);
		}

		public CompressedElement Read(ulong frag0, ulong frag1 = 0, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(CompressedElement.reusable, frag0, frag1, bcl);
			return CompressedElement.reusable;
		}

		#endregion

		#region Compress

		public void Compress(CompressedElement nonalloc, Transform trans)
		{
			//Debug.Log(_trsType + " " + XCrusher.Bits + " " + YCrusher.Bits + " " + ZCrusher.Bits + " " + Cached_TotalBits[0]);
			switch (TRSType)
			{
				case TRSType.Position:
					Compress(nonalloc, (local) ? trans.localPosition : trans.position);
					return;

				case TRSType.Euler:
					Compress(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles);
					return;

				case TRSType.Quaternion:
					Compress(nonalloc, (local) ? trans.localRotation : trans.rotation);
					return;

				case TRSType.Scale:
					Compress(nonalloc, (local) ? trans.localScale : trans.lossyScale);
					return;

				default:
#if UNITY_EDITOR
					TryingToCrushTransformUsingGenericWarning();
#endif
					Compress(nonalloc, (local) ? trans.localPosition : trans.position);
					return;
			}
		}

		public CompressedElement Compress(Transform trans)
		{
			Compress(CompressedElement.reusable, trans);
			return CompressedElement.reusable;
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Transform trans, byte[] buffer, ref int bitposition)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					CompressAndWrite((local) ? trans.localPosition : trans.position, buffer, ref bitposition);
					break;

				case TRSType.Euler:
					CompressAndWrite((local) ? trans.localEulerAngles : trans.eulerAngles, buffer, ref bitposition);
					break;

				case TRSType.Quaternion:
					CompressAndWrite((local) ? trans.localRotation : trans.rotation, buffer, ref bitposition);
					break;

				case TRSType.Scale:
					CompressAndWrite((local) ? trans.localScale : trans.lossyScale, buffer, ref bitposition);
					break;

				default:
#if UNITY_EDITOR
					TryingToCrushTransformUsingGenericWarning();
#endif
					CompressAndWrite((local) ? trans.localPosition : trans.position, buffer, ref bitposition);
					break;
			}
		}

		public void Compress(CompressedElement nonalloc, Element e)
		{
			if (TRSType == TRSType.Quaternion)
				Compress(nonalloc, e.quat);
			else
				Compress(nonalloc, e.v);
		}

		/// <summary>
		/// Compress an Element (Vector/Quaternion wrapper) and return as a CompressedElement.
		/// <para>WARNING: The returned CompressedElement is recycled and its values need to be used immediately.</para>
		/// <para>Supply a pre-allocated CompressedElement as an argument if you intend to store these values.</para>
		/// </summary>
		public CompressedElement Compress(Element e)
		{
			if (TRSType == TRSType.Quaternion)
				Compress(CompressedElement.reusable, e.quat);
			else
				Compress(CompressedElement.reusable, e.v);

			return CompressedElement.reusable;
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Element e, byte[] buffer, ref int bitposition)
		{
			if (TRSType == TRSType.Quaternion)
				CompressAndWrite(e.quat, buffer, ref bitposition);
			else
				CompressAndWrite(e.v, buffer, ref bitposition);
		}

		public void Compress(CompressedElement nonalloc, Rigidbody rb, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			switch (_trsType)
			{
				case TRSType.Scale:
					{
						Compress(nonalloc, local ? rb.transform.localScale : rb.transform.lossyScale, ia);
						break;
					}

				case TRSType.Quaternion:
					if (cache_qEnabled)
					{
						Quaternion localizedQ;
						if (local)
						{
							var par = rb.transform.parent;
							localizedQ = (par) ? (Quaternion.Inverse(par.rotation) * rb.rotation) : rb.rotation;
						}
						else
							localizedQ = rb.rotation;

						nonalloc.Set(this, _qcrusher.Compress(localizedQ));
					}

					break;

				case TRSType.Euler:
					{
						Vector3 localizedE;
						if (local)
						{
							var par = rb.transform.parent;
							localizedE = (par) ? (Quaternion.Inverse(par.rotation) * rb.rotation).eulerAngles : rb.rotation.eulerAngles;
						}
						else
							localizedE = rb.rotation.eulerAngles;

						CompressedFloat cx = (cache_xEnabled && ((int)ia & 1) != 0 ? _xcrusher.Compress(localizedE.x) : new CompressedFloat());
						CompressedFloat cy = (cache_yEnabled && ((int)ia & 2) != 0 ? _ycrusher.Compress(localizedE.y) : new CompressedFloat());
						CompressedFloat cz = (cache_zEnabled && ((int)ia & 4) != 0 ? _zcrusher.Compress(localizedE.z) : new CompressedFloat());
						nonalloc.Set(this, cx, cy, cz);

						break;
					}

				case TRSType.Position:
					{
						Vector3 localizedP;
						if (local)
						{
							var par = rb.transform.parent;
							localizedP = (par) ? par.InverseTransformPoint(rb.position) : rb.position;
						}
						else
							localizedP = rb.position;

						CompressedFloat cx = (cache_xEnabled && ((int)ia & 1) != 0 ? _xcrusher.Compress(localizedP.x) : new CompressedFloat());
						CompressedFloat cy = (cache_yEnabled && ((int)ia & 2) != 0 ? _ycrusher.Compress(localizedP.y) : new CompressedFloat());
						CompressedFloat cz = (cache_zEnabled && ((int)ia & 4) != 0 ? _zcrusher.Compress(localizedP.z) : new CompressedFloat());
						nonalloc.Set(this, cx, cy, cz);

						break;
					}

				default:
					{
						nonalloc.Clear();
						break;
					}
			}
		}

		public void Compress(CompressedElement nonalloc, Vector3 v, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Quaternion)
			{
				Debug.LogError("We shouldn't be seeing this. Quats should not be getting compressed from Eulers!");
				if (cache_qEnabled)
					nonalloc.Set(this, _qcrusher.Compress(Quaternion.Euler(v)));
			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				if (cache_uEnabled)
					nonalloc.Set(this, _ucrusher.Compress((uniformAxes == UniformAxes.YZ) ? v.y : v.x));
			}
			else
			{
				CompressedFloat cx = (cache_xEnabled && ((int)ia & 1) != 0 ? _xcrusher.Compress(v.x) : new CompressedFloat());
				CompressedFloat cy = (cache_yEnabled && ((int)ia & 2) != 0 ? _ycrusher.Compress(v.y) : new CompressedFloat());
				CompressedFloat cz = (cache_zEnabled && ((int)ia & 4) != 0 ? _zcrusher.Compress(v.z) : new CompressedFloat());

				nonalloc.Set(this, cx, cy, cz);
			}
		}

		public CompressedElement Compress(Vector3 v)
		{
			Compress(CompressedElement.reusable, v);
			return CompressedElement.reusable;
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Vector3 v, byte[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ)
		{

			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				//Debug.Log(this + " Compress <b>UNIFORM</b>");
				ulong cu = (cache_uEnabled) ? _ucrusher.Compress((uniformAxes == UniformAxes.YZ) ? v.y : v.x) : (ulong)0;
				buffer.Write(cu, ref bitposition, cache_uBits[0]);

			}
			else if (_trsType == TRSType.Quaternion)
			{
				Debug.Log("We shouldn't be seeing this. Quats should not be getting compressed from Eulers!");
				if (cache_qEnabled)
					buffer.Write(_qcrusher.Compress(Quaternion.Euler(v)), ref bitposition, cache_qBits);
			}
			else
			{
				//FloatCrusherUtilities.CheckBitCount(xcrusher.GetBits(0) + ycrusher.GetBits(0) + zcrusher.GetBits(0), 96);
				if (cache_xEnabled)
					buffer.Write(_xcrusher.Compress(v.x).cvalue, ref bitposition, cache_xBits[0]);
				if (cache_yEnabled)
					buffer.Write(_ycrusher.Compress(v.y).cvalue, ref bitposition, cache_yBits[0]);
				if (cache_zEnabled)
					buffer.Write(_zcrusher.Compress(v.z).cvalue, ref bitposition, cache_zBits[0]);
			}
		}

		/// <summary>
		/// Compress and bitpack the enabled vectors into a generic unsigned int.
		/// </summary>
		public void Compress(CompressedElement nonalloc, Quaternion quat)
		{
			if (!cached)
				CacheValues();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			TryingToWriteQuatAsEulerError();
#endif

			if (cache_qEnabled)
				nonalloc.Set(this, _qcrusher.Compress(quat));
		}

		public CompressedElement Compress(Quaternion quat)
		{
			Compress(CompressedElement.reusable, quat);
			return CompressedElement.reusable;
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Quaternion quat, byte[] buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			TryingToWriteQuatAsEulerError();
#endif

			if (cache_qEnabled)
				buffer.Write(_qcrusher.Compress(quat), ref bitposition, cache_qBits);
		}
		#endregion

		#region Decompress

		/// <summary>
		/// Decode (decompresss) and restore an element that was compressed by this crusher.
		/// </summary>
		public Element Decompress(CompressedElement compressed)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				float val = _ucrusher.Decompress((uint)compressed.cUniform);
				return new Vector3(val, val, val);
			}
			else if (_trsType == TRSType.Quaternion)
			{
				return _qcrusher.Decompress((ulong)compressed.cQuat);
			}
			else
			{
				// Issue log error for trying to write more than 64 bits to the ulong buffer
				//FloatCrusherUtilities.CheckBitCount(cache_TotalBits[0], 64);

				return new Vector3(
					cache_xEnabled ? (_xcrusher.Decompress((uint)compressed.cx)) : 0,
					cache_yEnabled ? (_ycrusher.Decompress((uint)compressed.cy)) : 0,
					cache_zEnabled ? (_zcrusher.Decompress((uint)compressed.cz)) : 0
					);
			}
		}

		public Element Decompress(ulong cval, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				float val = _ucrusher.Decompress((uint)cval);
				return new Vector3(val, val, val);
			}
			else if (_trsType == TRSType.Quaternion)
			{
				//Debug.Log("We should not see this! Quats should be getting called to DecompressToQuat");
				return _qcrusher.Decompress(cval);
			}
			else
			{
				// Issue log error for trying to write more than 64 bits to the ulong buffer
				//FloatCrusherUtilities.CheckBitCount(cache_TotalBits[0], 64);

				int bitposition = 0;
				return new Vector3(
					(cache_xEnabled && ((int)ia & 1) != 0) ? (_xcrusher.ReadAndDecompress(cval, ref bitposition)) : 0,
					(cache_yEnabled && ((int)ia & 2) != 0) ? (_ycrusher.ReadAndDecompress(cval, ref bitposition)) : 0,
					(cache_zEnabled && ((int)ia & 4) != 0) ? (_zcrusher.ReadAndDecompress(cval, ref bitposition)) : 0
					);
			}
		}

		//public Quaternion DecompressToQuat(CompressedElement compressed)
		//{
		//	if (!cached)
		//		CacheValues();

		//	DebugX.LogError("You seem to be trying to decompress a Quaternion from a crusher that is set up for " +
		//		System.Enum.GetName(typeof(TRSType), TRSType) + ". This likely won't end well.", TRSType != TRSType.Quaternion, true);

		//	Quaternion quat = qcrusher.Decompress(compressed.cQuat);
		//	return quat;
		//}

		#endregion

		#region Rigidbody Set/Move

		/// <summary>
		/// Applies only the enabled axes to the transform, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		[System.Obsolete("Apply for Rigidbody has been replaced with Move and Set, to indicate usage of MovePosition/Rotation vs rb.position/rotation.")]
		public void Apply(Rigidbody rb, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Apply(rb, Decompress(ce), ia);
		}

		/// <summary>
		/// Applies only the enabled axes to the rigidbody, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		[System.Obsolete("Apply for Rigidbody has been replaced with Move and Set, to indicate usage of MovePosition/Rotation vs rb.position/rotation.")]
		public void Apply(Rigidbody rb, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			switch (_trsType)
			{
				case TRSType.Quaternion:

					if (cache_qEnabled)
					{
						if (local && rb.transform.parent)
						{
							rb.transform.localRotation = e.quat;
						}
						else
						{
							rb.MoveRotation(e.quat);
						}
					}
					return;

				case TRSType.Position:

					var localized = (local & rb.transform.parent) ? rb.transform.TransformPoint(e.v) : e.v;
					var curr = rb.position;

					rb.MovePosition(new Vector3(
						cache_xEnabled && ((int)ia & 1) != 0 ? localized.x : curr.x,
						cache_yEnabled && ((int)ia & 2) != 0 ? localized.y : curr.y,
						cache_zEnabled && ((int)ia & 4) != 0 ? localized.z : curr.z
						));
					return;

				case TRSType.Euler:

					var currE = rb.rotation.eulerAngles;

					if (local && rb.transform.parent)
					{
						//rb.rotation = Quaternion.Euler(
						rb.transform.eulerAngles = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : currE.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : currE.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : currE.z
							);
					}
					else
					{
						//rb.rotation = Quaternion.Euler(
						rb.MoveRotation(Quaternion.Euler(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : currE.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : currE.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : currE.z
							));
					}

					return;

				default:
					Debug.LogError("Are you trying to Apply scale to a Rigidbody?");
					return;
			}
		}

		/// <summary>
		/// Rigidbody.MovePosition/MoveRotation only the enabled axes, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Move(Rigidbody rb, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Move(rb, Decompress(ce), ia);
		}

		/// <summary>
		/// Rigidbody.MovePosition/MoveRotation only the enabled axes, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Move(Rigidbody rb, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			{
				if (!cached)
					CacheValues();

				switch (_trsType)
				{
					case TRSType.Quaternion:

						if (cache_qEnabled)
						{
							if (local && rb.transform.parent)
								rb.MoveRotation(rb.transform.parent.rotation * e.quat);
								//rb.transform.localRotation = e.quat;
							else
								rb.MoveRotation(e.quat);
						}
						return;

					case TRSType.Position:

						var localized = (local & rb.transform.parent) ? rb.transform.TransformPoint(e.v) : e.v;

						rb.MovePosition(new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? localized.x : rb.position.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? localized.y : rb.position.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? localized.z : rb.position.z
							));
						return;

					case TRSType.Euler:

						var currE = rb.rotation.eulerAngles;

						if (local && rb.transform.parent)
						{
							//rb.rotation = Quaternion.Euler(
							rb.transform.eulerAngles = new Vector3(
								cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : currE.x,
								cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : currE.y,
								cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : currE.z
								);
						}
						else
						{
							//rb.rotation = Quaternion.Euler(
							rb.MoveRotation(Quaternion.Euler(
								cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : currE.x,
								cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : currE.y,
								cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : currE.z
								));
						}

						return;

					default:
						Debug.LogError("Are you trying to Apply scale to a Rigidbody?");
						return;
				}
			}
		}

		/// <summary>
		/// Set RB pos/rot using only the enabled axes, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Set(Rigidbody rb, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Set(rb, Decompress(ce), ia);
		}

		/// <summary>
		/// Set RB pos/rot using only the enabled axes, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Set(Rigidbody2D rb, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Set(rb, Decompress(ce), ia);
		}
		/// <summary>
		/// Set RB pos/rot using only the enabled axes, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Set(Rigidbody rb, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			{
				if (!cached)
					CacheValues();

				switch (_trsType)
				{
					case TRSType.Quaternion:

						if (cache_qEnabled)
						{
							if (local && rb.transform.parent)
								rb.rotation = rb.transform.parent.rotation * e.quat;
								//rb.transform.localRotation = e.quat;
							else
								rb.rotation = e.quat;
						}

						return;

					case TRSType.Position:

						var localized = (local & rb.transform.parent) ? rb.transform.TransformPoint(e.v) : e.v;
						var currP = rb.position;

						rb.position = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? localized.x : currP.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? localized.y : currP.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? localized.z : currP.z);

						return;

					case TRSType.Euler:

						var currE = rb.rotation.eulerAngles;

						if (local && rb.transform.parent)
						{
							rb.transform.eulerAngles = new Vector3(
								cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : currE.x,
								cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : currE.y,
								cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : currE.z
								);
						}
						else
						{
							rb.rotation = Quaternion.Euler(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : currE.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : currE.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : currE.z);
						}
						return;


					default:
						Debug.LogError("Are you trying to Apply scale to a Rigidbody?");
						return;
				}
			}
		}

		/// <summary>
		/// Set RB pos/rot using only the enabled axes, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb2d"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Set(Rigidbody2D rb2d, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			{
				if (!cached)
					CacheValues();

				switch (_trsType)
				{
					case TRSType.Quaternion:

						if (cache_qEnabled)
						{
							if (local && rb2d.transform.parent)
								rb2d.transform.localRotation = e.quat;
							else
								rb2d.rotation = e.quat.z;
						}

						return;

					case TRSType.Position:

						var localized = (local & rb2d.transform.parent) ? rb2d.transform.TransformPoint(e.v) : e.v;

						rb2d.position = new Vector2(
							cache_xEnabled && ((int)ia & 1) != 0 ? localized.x : rb2d.position.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? localized.y : rb2d.position.y);
						return;

					case TRSType.Euler:
						if (local && rb2d.transform.parent)
						{
							rb2d.transform.localEulerAngles = new Vector3(0, 0, (cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : rb2d.rotation));
						}
						else
						{
							rb2d.rotation = cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : rb2d.rotation;
						}
						return;

					default:
						Debug.LogError("Are you trying to Apply scale to a Rigidbody?");
						return;
				}
			}
		}

		#endregion

		#region Transform Apply

		/// <summary>
		/// Applies only the enabled axes to the transform, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Apply(Transform trans, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Apply(trans, Decompress(ce), ia);
		}


		/// <summary>
		/// Applies only the enabled axes to the transform, leaving the disabled axes untouched.
		/// </summary>
		public void Apply(Transform trans, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			switch (_trsType)
			{
				case TRSType.Quaternion:

					if (cache_qEnabled)
					{
						if (local)
							trans.localRotation = e.quat;
						else
							trans.rotation = e.quat;
					}

					return;

				case TRSType.Position:

					if (local)
					{
						trans.localPosition = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.localPosition.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.localPosition.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.localPosition.z
							);
					}
					else
					{
						trans.position = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.position.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.position.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.position.z
							);
					}
					return;

				case TRSType.Euler:

					if (local)
					{
						trans.localEulerAngles = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.localEulerAngles.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.localEulerAngles.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.localEulerAngles.z
							);
					}
					else
					{
						trans.eulerAngles = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.eulerAngles.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.eulerAngles.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.eulerAngles.z
							);
					}
					return;

				default:
					if (local)
					{
						if (uniformAxes == UniformAxes.NonUniform)
						{
							trans.localScale = new Vector3(

							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.localScale.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.localScale.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.localScale.z
							);
						}

						// Is a uniform scale
						else
						{
							float uniform = ((int)uniformAxes & 1) != 0 ? e.v.x : e.v.y;
							trans.localScale = new Vector3
								(
								((int)uniformAxes & 1) != 0 ? uniform : trans.localScale.x,
								((int)uniformAxes & 2) != 0 ? uniform : trans.localScale.y,
								((int)uniformAxes & 4) != 0 ? uniform : trans.localScale.z
								);
						}
					}
					else
					{
						if (uniformAxes == UniformAxes.NonUniform)
						{
							trans.localScale = new Vector3(

							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.lossyScale.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.lossyScale.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.lossyScale.z
							);
						}

						// Is a uniform scale
						else
						{
							float uniform = ((int)uniformAxes & 1) != 0 ? e.v.x : e.v.y;
							trans.localScale = new Vector3
								(
								((int)uniformAxes & 1) != 0 ? uniform : trans.lossyScale.x,
								((int)uniformAxes & 2) != 0 ? uniform : trans.lossyScale.y,
								((int)uniformAxes & 4) != 0 ? uniform : trans.lossyScale.z
								);
						}
					}
					return;
			}
		}

		//public void Apply(Transform trans, Quaternion q)
		//{
		//	if (!cached)
		//		CacheValues();

		//	if (_trsType == TRSType.Quaternion)
		//	{
		//		if (cache_qEnabled)
		//			if (local)
		//				trans.rotation = q;
		//			else
		//				trans.localRotation = q;
		//		return;
		//	}

		//	DebugX.LogError("You seem to be trying to apply a Quaternion to " + System.Enum.GetName(typeof(TRSType), _trsType) + ".", true, true);
		//}

		#endregion

		#region  Utilities

		/// <summary>
		/// Return a value clamped to the Min/Max values defined for each axis by this Crusher.
		/// </summary>
		/// <param name="v3"></param>
		/// <returns></returns>
		public Vector3 Clamp(Vector3 v3)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError("You cannot clamp a quaternion.");
#endif
				return v3;
			}
			if (TRSType == TRSType.Scale)
			{
				if (uniformAxes == UniformAxes.NonUniform)
				{
					return new Vector3(
						(cache_xEnabled) ? _xcrusher.Clamp(v3.x) : 0,
						(cache_yEnabled) ? _ycrusher.Clamp(v3.y) : 0,
						(cache_zEnabled) ? _zcrusher.Clamp(v3.z) : 0
					);
				}
				else
				{
					return new Vector3(
					((uniformAxes & (UniformAxes)1) != 0) ? _ucrusher.Clamp(v3.x) : 0,
					((uniformAxes & (UniformAxes)2) != 0) ? _ucrusher.Clamp(v3.x) : 0,
					((uniformAxes & (UniformAxes)4) != 0) ? _ucrusher.Clamp(v3.x) : 0
					);
				}
			}
			if (TRSType == TRSType.Euler)
			{
				return new Vector3(
					(cache_xEnabled) ? _xcrusher.ClampRotation(v3.x) : 0,
					(cache_yEnabled) ? _ycrusher.ClampRotation(v3.y) : 0,
					(cache_zEnabled) ? _zcrusher.ClampRotation(v3.z) : 0
					);
			}
			else
			{
				return new Vector3(
						(cache_xEnabled) ? _xcrusher.Clamp(v3.x) : 0,
						(cache_yEnabled) ? _ycrusher.Clamp(v3.y) : 0,
						(cache_zEnabled) ? _zcrusher.Clamp(v3.z) : 0
					);
			}
		}
		/// <summary>
		/// Return the smallest bit culling level that will be able to communicate the changes between two compressed elements.
		/// </summary>
		public BitCullingLevel FindBestBitCullLevel(CompressedElement a, CompressedElement b, BitCullingLevel maxCulling)
		{
			/// Quats can't cull upper bits, so its an all or nothing. Either the bits match or they don't
			if (TRSType == TRSType.Quaternion)
			{
				if ((ulong)a.cQuat == (ulong)b.cQuat)
					return BitCullingLevel.DropAll;
				else
					return BitCullingLevel.NoCulling;
			}

			if (maxCulling == BitCullingLevel.NoCulling || !TestMatchingUpper(a, b, BitCullingLevel.DropThird))
				return BitCullingLevel.NoCulling;

			if (maxCulling == BitCullingLevel.DropThird || !TestMatchingUpper(a, b, BitCullingLevel.DropHalf))
				return BitCullingLevel.DropThird;

			if (maxCulling == BitCullingLevel.DropHalf || !TestMatchingUpper(a, b, BitCullingLevel.DropAll))
				return BitCullingLevel.DropHalf;

			// both values are the same
			return BitCullingLevel.DropAll;
		}

		private bool TestMatchingUpper(uint a, uint b, int lowerbits)
		{
			return (((a >> lowerbits) << lowerbits) == ((b >> lowerbits) << lowerbits));
		}

		public bool TestMatchingUpper(CompressedElement a, CompressedElement b, BitCullingLevel bcl)
		{
			return
				(
				TestMatchingUpper(a.cx, b.cx, _xcrusher.GetBits(bcl)) &&
				TestMatchingUpper(a.cy, b.cy, _ycrusher.GetBits(bcl)) &&
				TestMatchingUpper(a.cz, b.cz, _zcrusher.GetBits(bcl))
				);
		}

		/// <summary>
		/// Get the total number of bits needed to serialize an element with this crusher.
		/// </summary>
		public int TallyBits(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				return (_ucrusher != null && _ucrusher.Enabled) ? _ucrusher.GetBits(bcl) : 0;
			}
			else if (_trsType == TRSType.Quaternion)
			{
				return (_qcrusher != null && _qcrusher.Enabled) ? _qcrusher.Bits : 0;
			}
			else
			{
				if (_trsType == TRSType.Position && useWorldBounds)
				{
					return WorldBoundsSettings.TallyBits(ref boundsGroupId);
				}
				return
					((_xcrusher != null && _xcrusher.Enabled) ? _xcrusher.GetBits(bcl) : 0) +
					((_ycrusher != null && _ycrusher.Enabled) ? _ycrusher.GetBits(bcl) : 0) +
					((_zcrusher != null && _zcrusher.Enabled) ? _zcrusher.GetBits(bcl) : 0);
			}
		}

		public void CopyFrom(ElementCrusher src)
		{
			_trsType = src._trsType;
			uniformAxes = src.uniformAxes;
			if (_xcrusher != null && src._xcrusher != null) _xcrusher.CopyFrom(src._xcrusher);
			if (_ycrusher != null && src._ycrusher != null) _ycrusher.CopyFrom(src._ycrusher);
			if (_zcrusher != null && src._zcrusher != null) _zcrusher.CopyFrom(src._zcrusher);
			if (_ucrusher != null && src._ucrusher != null) _ucrusher.CopyFrom(src._ucrusher);
			if (_qcrusher != null && src._qcrusher != null) _qcrusher.CopyFrom(src._qcrusher);
			local = src.local;
		}

		public override string ToString()
		{
			return "ElementCrusher [" + _trsType + "] ";
		}

		#endregion

		#region IEquatable

		public override bool Equals(object obj)
		{
			return Equals(obj as ElementCrusher);
		}

		public bool Equals(ElementCrusher other)
		{
			return other != null &&
				   _trsType == other._trsType &&
				   EqualityComparer<Transform>.Default.Equals(defaultTransform, other.defaultTransform) &&
				   uniformAxes == other.uniformAxes &&

				   (_xcrusher == null ? (other._xcrusher == null) : _xcrusher.Equals(other._xcrusher)) &&
				   (_ycrusher == null ? (other._ycrusher == null) : _ycrusher.Equals(other._ycrusher)) &&
				   (_zcrusher == null ? (other._zcrusher == null) : _zcrusher.Equals(other._zcrusher)) &&
				   (_ucrusher == null ? (other._ucrusher == null) : _ucrusher.Equals(other._ucrusher)) &&
				   (_qcrusher == null ? (other._qcrusher == null) : _qcrusher.Equals(other._qcrusher)) &&

				   local == other.local;
		}

		public override int GetHashCode()
		{

			var hashCode = -1042106911;
			hashCode = hashCode * -1521134295 + _trsType.GetHashCode();
			//hashCode = hashCode * -1521134295 + EqualityComparer<Transform>.Default.GetHashCode(defaultTransform);
			hashCode = hashCode * -1521134295 + uniformAxes.GetHashCode();
			hashCode = hashCode * -1521134295 + ((_xcrusher == null) ? 0 : _xcrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((_ycrusher == null) ? 0 : _ycrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((_zcrusher == null) ? 0 : _zcrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((_ucrusher == null) ? 0 : _ucrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((_qcrusher == null) ? 0 : _qcrusher.GetHashCode());
			//hashCode = hashCode * -1521134295 + EqualityComparer<FloatCrusher>.Default.GetHashCode(ycrusher);
			//hashCode = hashCode * -1521134295 + EqualityComparer<FloatCrusher>.Default.GetHashCode(zcrusher);
			//hashCode = hashCode * -1521134295 + EqualityComparer<FloatCrusher>.Default.GetHashCode(ucrusher);
			//hashCode = hashCode * -1521134295 + EqualityComparer<QuatCrusher>.Default.GetHashCode(qcrusher);
			hashCode = hashCode * -1521134295 + local.GetHashCode();

			return hashCode;
		}

		public static bool operator ==(ElementCrusher crusher1, ElementCrusher crusher2)
		{
			return EqualityComparer<ElementCrusher>.Default.Equals(crusher1, crusher2);
		}

		public static bool operator !=(ElementCrusher crusher1, ElementCrusher crusher2)
		{
			return !(crusher1 == crusher2);
		}

		#endregion
	}

	#region UnityEditor Drawer

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(ElementCrusher))]
	[CanEditMultipleObjects]
	[AddComponentMenu("Crusher/Element Crusher")]

	public class ElementCrusherDrawer : CrusherDrawer
	{
		public const float TOP_PAD = 2f;
		public const float BTM_PAD = 2f;
		//public const float BTM_PAD_SINGLE = 2f;
		private const float TITL_HGHT = 16f;
		bool haschanged;

		private static GUIContent gc_label = new GUIContent();
		public static readonly GUIContent GC_USE_WRLDBNDS_LONG = new GUIContent("Use World Bounds", "Use position crusher generated by 'WorldBounds' components attached to scene objects.");
		public static readonly GUIContent GC_USE_WRLDBNDS_SHRT = new GUIContent("World Bnds", "Use position crusher generated by 'WorldBounds' components attached to scene objects.");
		readonly GUIContent GC_LCL = new GUIContent("Lcl");

		private int holdindent;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			haschanged = false;
			EditorGUI.BeginChangeCheck();

			bool isWorldBounds = label.text.Contains("World Bounds");
			bool disableRanges = (label.tooltip != null && label.tooltip.Contains("DISABLE_RANGES"));

			gc_label.text = label.text;
			gc_label.tooltip = (disableRanges) ? null : label.tooltip;

			base.OnGUI(r, property, label);

			holdindent = EditorGUI.indentLevel;

			// Hacky way to get the real object
			ElementCrusher target = (ElementCrusher)DrawerUtils.GetParent(property.FindPropertyRelative("_xcrusher"));

			//SerializedProperty uniformAxes = property.FindPropertyRelative("uniformAxes");
			SerializedProperty x = property.FindPropertyRelative("_xcrusher");
			SerializedProperty y = property.FindPropertyRelative("_ycrusher");
			SerializedProperty z = property.FindPropertyRelative("_zcrusher");
			SerializedProperty u = property.FindPropertyRelative("_ucrusher");
			SerializedProperty q = property.FindPropertyRelative("_qcrusher");
			SerializedProperty hideFieldName = property.FindPropertyRelative("hideFieldName");
			SerializedProperty useWorldBounds = property.FindPropertyRelative("useWorldBounds");
			SerializedProperty boundsGroupId = property.FindPropertyRelative("boundsGroupId");
			SerializedProperty isExpanded = property.FindPropertyRelative("isExpanded");

			float xh = EditorGUI.GetPropertyHeight(x);
			float yh = EditorGUI.GetPropertyHeight(y);
			float zh = EditorGUI.GetPropertyHeight(z);
			float wh = EditorGUI.GetPropertyHeight(u);
			float qh = EditorGUI.GetPropertyHeight(q);


			//bool isQuatCrush = target.TRSType == TRSType.Quaternion;
			//bool isUniformScale = target.TRSType == TRSType.Scale && target.uniformAxes != 0;
			bool isWrappedInTransformCrusher = DrawerUtils.GetParent(property) is TransformCrusher;
			bool showHeader = !hideFieldName.boolValue && !isWrappedInTransformCrusher;


			Rect ir = EditorGUI.IndentedRect(r);

			float currentline = r.yMin;

			if (showHeader)
			{
				EditorGUI.LabelField(new Rect(r.xMin, currentline, r.width, LINEHEIGHT), gc_label); //*/ property.displayName);
				currentline += LINEHEIGHT + SPACING;
				ir.yMin += LINEHEIGHT;
			}
			else
			{
				currentline += SPACING;
			}

			//ir.yMin += currentline;
			Rect framer = ir;
			if (!isWorldBounds)
			{
				framer.height -= BTTM_MARGIN;
				Rect oframer = new Rect(framer.xMin - 1, framer.yMin - 1, framer.width + 2, framer.height + 2);
				SolidTextures.DrawTexture(oframer, SolidTextures.lowcontrast2D);
				SolidTextures.DrawTexture(framer, SolidTextures.contrastgray2D);
			}
			else
			{
				const int unpad = 4;
				r.xMin -= unpad; r.xMax += unpad;
				ir.xMin -= unpad; ir.xMax += unpad;
			}


			//GUI.Box(new Rect(ir.xMin - 2, currentline - 2, ir.width + 4, ir.height - 2), GUIContent.none, (GUIStyle)"GroupBox");
			//SolidTextures.DrawTexture(new Rect(ir.xMin - 2, currentline -2, ir.width + 4, ir.height), SolidTextures.highcontrast2D);
			//SolidTextures.DrawTexture(new Rect(ir.xMin - 1, currentline - 1, ir.width + 2, ir.height - 1), SolidTextures.white2D);

			//SolidTextures.DrawTexture(new Rect(ir.xMin - 2, currentline - 2, ir.width + 4, ir.height - BTM_PAD), SolidTextures.lowcontrast2D);
			//SolidTextures.DrawTexture(new Rect(ir.xMin, currentline, ir.width, 16 + 1/*+ SPACING*/), SolidTextures.contrastgray2D);

#if UNITY_2019_3_OR_NEWER
			const int localtoggleleft = 74;
#else
			const int localtoggleleft = 76;
#endif
			float fcLeft = ir.xMin + ((isWorldBounds) ? 4 : 15);
			float enumwidth = (ir.width - 99) / 2 - 1;
			float fcLeft2 = fcLeft + enumwidth + 2;


			isExpanded.boolValue = (isWorldBounds) ? true :
				GUI.Toggle(new Rect(ir.xMin + 2, currentline, 16, LINEHEIGHT), isExpanded.boolValue, GUIContent.none, FOLDOUT_STYLE);

			if (target.enableTRSTypeSelector)
			{
				EditorGUI.indentLevel = 0;
				var trsType = (TRSType)EditorGUI.EnumPopup(new Rect(fcLeft - 2, currentline - 1, enumwidth, LINEHEIGHT), target.TRSType, (GUIStyle)"GV Gizmo DropDown");
				EditorGUI.indentLevel = holdindent;
				if (target.TRSType != trsType)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Select TRS Type");
					target.TRSType = trsType;
				}
			}
			else if (target.TRSType == TRSType.Quaternion || target.TRSType == TRSType.Euler)
			{
				EditorGUI.indentLevel = 0;
				var trsType = (TRSType)EditorGUI.EnumPopup(new Rect(fcLeft - 2, currentline - 1, enumwidth, LINEHEIGHT), GUIContent.none, (RotationType)target.TRSType, (GUIStyle)"GV Gizmo DropDown");
				EditorGUI.indentLevel = holdindent;
				if (target.TRSType != trsType)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Select Rotation Type");
					target.TRSType = trsType;
				}
			}
			else
			{
				GUIContent title =
					(isWrappedInTransformCrusher || showHeader) ? new GUIContent(Enum.GetName(typeof(TRSType), target.TRSType)) : gc_label; // + " Crshr");
				GUI.Label(new Rect(fcLeft + 2, currentline, r.width, LINEHEIGHT), title, MINI_LBL_STYLE);
			}

			/// WorldBounds Enum
			if (target.TRSType == TRSType.Position && !isWorldBounds)
			{
				EditorGUI.indentLevel = 0;

				GUIContent wb_gc = (r.width > 320) ? GC_USE_WRLDBNDS_LONG : GC_USE_WRLDBNDS_SHRT;

				EditorGUI.LabelField(new Rect(fcLeft2 + 14, currentline, enumwidth - 10, LINEHEIGHT), wb_gc, MINI_LBL_STYLE);
				bool useWrldBnds = GUI.Toggle(new Rect(fcLeft2, currentline, 16, LINEHEIGHT), useWorldBounds.boolValue, GUIContent.none, MINI_TGL_STYLE);
				if (useWrldBnds != useWorldBounds.boolValue)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Toggle Use World Bounds");
					target.UseWorldBounds = useWrldBnds;
				}
				EditorGUI.indentLevel = holdindent;
			}

			if (target.enableLocalSelector && (target.TRSType != TRSType.Position || !target.UseWorldBounds))
			{
				EditorGUI.indentLevel = 0;

				GUI.Label(new Rect(paddedright - localtoggleleft + 14 /*+ 14*/, currentline, 80, LINEHEIGHT), GC_LCL, MINI_LBL_STYLE);

				bool local = GUI.Toggle(new Rect(paddedright - localtoggleleft, currentline, 20, LINEHEIGHT), target.local, GUIContent.none, MINI_TGL_STYLE);

				if (target.local != local)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Toggle Local");
					target.local = local;
				}

				EditorGUI.indentLevel = holdindent;
			}

			EditorGUI.LabelField(new Rect(paddedleft, currentline, paddedwidth, 16), target.TallyBits() + " Bits", miniLabelRight);

			// Scale Uniform Enum
			if (target.TRSType == TRSType.Scale)
			{
				EditorGUI.indentLevel = 0;

				var uniformAxes =
					(ElementCrusher.UniformAxes)EditorGUI.EnumPopup(new Rect(fcLeft2, currentline - 1, enumwidth, LINEHEIGHT), GUIContent.none, target.uniformAxes, (GUIStyle)"GV Gizmo DropDown");
				if (target.uniformAxes != uniformAxes)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Select Uniform Axes");
					target.uniformAxes = uniformAxes;
				}
				EditorGUI.indentLevel = holdindent;
			}

			if (isExpanded.boolValue/* && (target.TRSType != TRSType.Position || !target.useWorldBounds)*/)
			{
				currentline += TITL_HGHT + SPACING;
				bool isSingleElement = (target.TRSType == TRSType.Scale && target.uniformAxes != 0) || (target.TRSType == TRSType.Quaternion);
				Rect propr = new Rect(r.xMin + PADDING, currentline, r.width - PADDING * 2, isSingleElement ?
					((target.TRSType == TRSType.Quaternion) ? qh : wh) :
					xh + yh + zh);

				if (target.TRSType == TRSType.Scale && target.uniformAxes != 0)
				{
					EditorGUI.PropertyField(propr, u);
				}
				else if (target.TRSType == TRSType.Quaternion)
				{
					EditorGUI.PropertyField(propr, q);
				}
				else
				{
					if (target.UseWorldBounds)
					{
						EditorGUI.BeginChangeCheck();
						EditorGUI.PropertyField(propr, boundsGroupId);
						if (EditorGUI.EndChangeCheck())
						{
							// Apply new value as a property in order to trigger the property.
							target.BoundsGroupId = boundsGroupId.intValue;
						}
					}
					else
					{
						var fcgc = new GUIContent("", (isWorldBounds && disableRanges) ? "DISABLE_RANGE" : "");
						EditorGUI.PropertyField(propr, x, fcgc);
						propr.yMin += xh;
						EditorGUI.PropertyField(propr, y, fcgc);
						propr.yMin += yh;
						EditorGUI.PropertyField(propr, z, fcgc);
					}
				}
			}

			if (EditorGUI.EndChangeCheck() || haschanged)
			{
				property.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}

			EditorGUI.indentLevel = holdindent;
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty trsType = property.FindPropertyRelative("_trsType");
			SerializedProperty uniformAxes = property.FindPropertyRelative("uniformAxes");
			SerializedProperty x = property.FindPropertyRelative("_xcrusher");
			SerializedProperty y = property.FindPropertyRelative("_ycrusher");
			SerializedProperty z = property.FindPropertyRelative("_zcrusher");
			SerializedProperty u = property.FindPropertyRelative("_ucrusher");
			SerializedProperty q = property.FindPropertyRelative("_qcrusher");
			SerializedProperty useWorldBounds = property.FindPropertyRelative("useWorldBounds");
			SerializedProperty boundsGroupId = property.FindPropertyRelative("boundsGroupId");
			SerializedProperty isExpanded = property.FindPropertyRelative("isExpanded");
			SerializedProperty hideFieldName = property.FindPropertyRelative("hideFieldName");

			bool showHeader = !hideFieldName.boolValue && !(DrawerUtils.GetParent(property) is TransformCrusher);
			bool isexpanded = isExpanded.boolValue /*&& (trsType.intValue != (int)TRSType.Position || !useWorldBounds.boolValue)*/;

			float topAndBottom = PADDING + TITL_HGHT + BTTM_MARGIN + ((showHeader) ? LINEHEIGHT : 0); // + TOP_PAD : TOP_PAD;

			if (!isexpanded)
				return topAndBottom;

			float h;
			if (trsType.enumValueIndex == (int)TRSType.Scale && uniformAxes.enumValueIndex != 0)
			{
				h = EditorGUI.GetPropertyHeight(u);
			}
			else if (trsType.enumValueIndex == (int)TRSType.Quaternion)
			{
				h = (isexpanded) ? EditorGUI.GetPropertyHeight(q) : 0;
			}
			else
			{
				if (useWorldBounds.boolValue)
				{
					h = EditorGUI.GetPropertyHeight(boundsGroupId);
				}
				else
				{
					float xh = EditorGUI.GetPropertyHeight(x);
					float yh = EditorGUI.GetPropertyHeight(y);
					float zh = EditorGUI.GetPropertyHeight(z);

					h = xh + yh + zh;
				}
			}

			return topAndBottom + SPACING + h + PADDING - 1;
		}
	}
#endif

	#endregion
}
