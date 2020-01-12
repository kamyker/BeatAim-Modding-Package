
using System;
using UnityEngine;

public class VisualizeToMusic_MaterialFloat : MonoBehaviourEventful<VisualizeToMusic_MaterialFloat>
{
	[SerializeField] public string ShaderFieldName = "_Exposure";
	[SerializeField] public MinMaxSmoothFloat ReactionToMusicLoudness = new MinMaxSmoothFloat(0.05f,0.35f,3, false);
	[SerializeField] public bool ChangeSharedMaterial = false;

	protected override VisualizeToMusic_MaterialFloat _this => this;
}
