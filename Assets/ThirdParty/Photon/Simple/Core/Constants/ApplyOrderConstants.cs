// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	public static class ApplyOrderConstants
	{
		public const int MAX_ORDER_VAL = 24;
#if UNITY_EDITOR
		public const string TOOLTIP =
			"Manually set the order in which callbacks occur. When components share a order value, " +
			"they will execute in the order in which they exist in the GameObjects hierarchy." +
			"It is recommended you leave this setting at the default, as strange behavior can result with some component orders.";

		//public static Dictionary<System.Type, int> applyOrderForType = new Dictionary<System.Type, int>();
#endif
		public const int COLLISIONS = 2;
        public const int STATE_TIMER = 3;
        public const int STATES = 5;
        public const int TRANSFORM = 9;
        public const int ANIMATOR = 11;
		public const int DEFAULT = 13;
		public const int VITALS = 15;
		public const int HITSCAN = 17;
		public const int WEAPONS = 19;
        public const int OWNERSHIP = 21;

    }
}
