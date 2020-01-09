
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
		Enabled.Invoke( this );
	}
	private void OnDisable()
	{
		Disabled.Invoke( this );
	}
}

[System.Serializable]
public class MinMaxSmoothFloat
{
	[SerializeField] public float minValue = 0;
	[SerializeField] public float maxValue = 1;
	[SerializeField] public float speed = 4;
	[SerializeField] public bool opposite;
	public MinMaxSmoothFloat( float minValue, float maxValue, float speed, bool opposite )
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.speed = speed;
		this.opposite = opposite;
	}
}
