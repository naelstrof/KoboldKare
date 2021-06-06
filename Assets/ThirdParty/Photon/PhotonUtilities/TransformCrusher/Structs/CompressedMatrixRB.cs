//// ---------------------------------------------------------------------------------------------
//// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
//// <author>developer@exitgames.com</author>
//// ---------------------------------------------------------------------------------------------


//namespace Photon.Compression
//{

//	/// <summary>
//	/// Future home of CompressedMatrix RB - Still in development, do not use.
//	/// </summary>
//	public class CompressedMatrixRB : CompressedMatrix
//	{
//		public CompressedElement cVel = new CompressedElement();
//		public CompressedElement cAng = new CompressedElement();

//		public new static CompressedMatrixRB reusable = new CompressedMatrixRB();

//		#region Constructors

//		// Constructor
//		public CompressedMatrixRB()
//		{
//		}

//		public CompressedMatrixRB(RigidbodyCrusher crusher)
//		{
//			this.crusher = crusher;
//		}
		
//		#endregion

//		public void CopyTo(CompressedMatrixRB copyTarget)
//		{
//			cPos.CopyTo(copyTarget.cPos);
//			cRot.CopyTo(copyTarget.cRot);
//			cScl.CopyTo(copyTarget.cScl);
//			cVel.CopyTo(copyTarget.cVel);
//			cAng.CopyTo(copyTarget.cAng);
//		}
//		public void CopyFrom(CompressedMatrixRB copySource)
//		{
//			cPos.CopyFrom(copySource.cPos);
//			cRot.CopyFrom(copySource.cRot);
//			cScl.CopyFrom(copySource.cScl);
//			cVel.CopyFrom(copySource.cVel);
//			cAng.CopyFrom(copySource.cAng);
//		}

//		public new void Clear()
//		{
//			crusher = null;
//			cPos.Clear();
//			cRot.Clear();
//			cScl.Clear();
//			cVel.Clear();
//			cAng.Clear();
//		}
//	}

//}
