using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Dialogue {
	[CreateAssetMenu(menuName = "Dialogue/CharacterInfo")]
	public class CharacterInfo : ScriptableObject {
		public Color color;
		public TMP_FontAsset font;
	}
}