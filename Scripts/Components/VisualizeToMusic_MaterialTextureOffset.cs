
using System;
using UnityEngine;

public class VisualizeToMusic_MaterialTextureOffset : MonoBehaviourEventful<VisualizeToMusic_MaterialTextureOffset>
{
	[SerializeField] public string ShaderFieldName = "_BaseMap";
	[SerializeField] public MinMaxSmooth<Vector2> ReactionToMusicLoudness = new MinMaxSmooth<Vector2>(new Vector2(0,0),new Vector2(0,-0.001f),3, false);
	[SerializeField] public bool ChangeSharedMaterial = false;
}
