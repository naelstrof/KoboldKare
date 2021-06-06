// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;

namespace Photon.Compression
{
	public enum DefaultKeyRate { Never, Every, Every2nd, Every3rd, Every4th, Every5th, Every10th = 10 }

	public interface IPackObj
	{

	}
	/// <summary>
	/// Indicates what automatically will be Packed.
	/// </summary>
	public enum DefaultPackInclusion {
		/// <summary>
		/// Only fields with a Pack Attribute will be included.
		/// </summary>
		Explicit,
		/// <summary>
		/// The default PackAttribute will be applied to all recognized public fields.
		/// </summary>
		AllPublic }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class PackObjectAttribute : Attribute
	{
		public DefaultKeyRate defaultKeyRate;
		public DefaultPackInclusion defaultInclusion = DefaultPackInclusion.Explicit;
		public SyncAs syncAs = SyncAs.State;

		public string postSnapCallback;
		public string postApplyCallback;

		/// <summary>
		/// BufferSize is the number of bytes that will be allocated for each frame.
		/// </summary>
		/// <param name="bufferSize"></param>
		public PackObjectAttribute(DefaultKeyRate defaultKeyRate = DefaultKeyRate.Every)
		{
			this.defaultKeyRate = defaultKeyRate;
		}
	}
}
