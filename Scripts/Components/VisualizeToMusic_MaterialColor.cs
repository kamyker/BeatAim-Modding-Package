
using System;
using UnityEngine;

public class VisualizeToMusic_MaterialColor : MonoBehaviourEventful<VisualizeToMusic_MaterialColor>
{
	[SerializeField] public string ShaderFieldName = "_Color";
	[SerializeField] public MinMaxSmooth<Color> ReactionToMusicLoudness = new MinMaxSmooth<Color>(Color.black,Color.white,3, false);
	[SerializeField] public bool ChangeSharedMaterial = false;
}
