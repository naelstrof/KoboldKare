// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Compression
{
	public enum KeyRate { UseDefault = -1, Never, Every, Every2nd, Every3rd, Every4th, Every5th, Every10th = 10 }

	/// <summary>
	/// Indicates how a [Pack] syncvar acts.
	/// </summary>
	public enum SyncAs
	{
		/// <summary>
		/// For PackObjects this uses the default of State. For Pack syncvar fields this indicates they should use the PackObject setting.
		/// </summary>
		Auto,
		/// <summary>
		/// Pack syncvar replicates changes in value to other clients, and stays at that value until changed by the owner.
		/// </summary>
		State,
		/// <summary>
		/// Pack syncvar resets value on owner after its value is captured/sent. Reset is done with value = new T(). Only works on struct types.
		/// </summary>
		Trigger
	}
	/// <summary>
	/// Callbacks assigned to Pack attributes can be called before or after the value is set. 
	/// You can also opt for the value not to be automatically set at all, and handle it yourself in the callback;
	/// </summary>
	public enum SetValueTiming { Never, AfterCallback, BeforeCallback }

	/// <summary>
	/// This enum indicates if an extra bit should be added to indicate cases of Zero value.
	/// When value is zero, only the one bit is sent. Useful for values that spend more time at zero than not, and delta frames are not an option.
	/// </summary>
	public enum IndicatorBit { None, IsZero }
	/// <summary>
	/// This enum indicates if an extra bit should be added to indicate cases of Zero value, XXXXX preValue, or two bits should be added to indicate Zero, Min, Max.
	/// When value is one of those, only the indicator bits will be written. Use this if these values are expected to spend most of their life
	/// at zero, min or max.
	/// </summary>
	public enum IndicatorBits { None, IsZero, IsZeroMidMinMax }
}