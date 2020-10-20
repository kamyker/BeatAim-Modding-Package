
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

[System.Serializable]
public class MinMaxSmooth<T>
{
	[SerializeField] public T minValue;
	[SerializeField] public T maxValue;
	[SerializeField] public float speed = 4;
	[SerializeField] public bool opposite;
	public MinMaxSmooth( T minValue, T maxValue, float speed, bool opposite )
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.speed = speed;
		this.opposite = opposite;
	}
}
