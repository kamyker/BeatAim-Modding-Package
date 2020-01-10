
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeToMusic_MaterialFloat  : MonoBehaviour
{
	public static event Action<VisualizeToMusic_MaterialFloat> Enabled;
	public static event Action<VisualizeToMusic_MaterialFloat> Disabled;

	[SerializeField] public string ShaderFieldName = "_Exposure";
	[SerializeField] public MinMaxSmoothFloat ReactionToMusicLoudness = new MinMaxSmoothFloat(0.05f,0.35f,3, false);

	private void OnEnable()
	{
		Enabled?.Invoke( this );
	}
	private void OnDisable()
	{
		Disabled?.Invoke( this );
	}
}
