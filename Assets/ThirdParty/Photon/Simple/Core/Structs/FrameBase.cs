// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	public enum FrameContents { Empty, Partial, NoChange, Extrapolated, Complete }

	/// <summary>
	/// Extend this base class for derived SyncObjectTFrame to include networked variables.
	/// </summary>
	public abstract class FrameBase
	{
		public int frameId;
		public FrameContents content;
		//public bool isCompleteFrame;

		public FrameBase()
		{

		}
		public FrameBase(int frameId)
		{
			this.frameId = frameId;
		}

        public static TFrame Instantiate<TFrame>(int frameId) where TFrame : FrameBase
        {
            var newframe = (TFrame)System.Activator.CreateInstance(typeof(TFrame));
            newframe.frameId = frameId;
            return newframe;
        }

		public virtual void CopyFrom(FrameBase sourceFrame)
		{
			content = sourceFrame.content;
		}
		//public abstract bool Compare(FrameBase frame, FrameBase holdframe);

		public virtual void Clear()
		{
			content = FrameContents.Empty;
		}

		public static void PopulateFrames<TFrame>(ref TFrame[] frames) where TFrame : FrameBase/*, new()*/
		{
			int frameCount = TickEngineSettings.frameCount;
			frames = new TFrame[frameCount + 1];
			for (int i = 0; i <= frameCount; ++i)
			{
                TFrame frame = Instantiate<TFrame>(i); // new TFrame() { frameId = i };
				frames[i] = frame;

			}
		}
	}
}
