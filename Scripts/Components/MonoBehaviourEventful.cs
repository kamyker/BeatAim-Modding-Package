
using System;
using UnityEngine;

public abstract class MonoBehaviourEventful<T> : MonoBehaviour
	where T : MonoBehaviourEventful<T>
{
	public static event Action<T> Enabled;
	public static event Action<T> Disabled;

	protected abstract T _this { get; }

	private void OnEnable()
		=> Enabled?.Invoke( _this );

	private void OnDisable()
		=> Disabled?.Invoke( _this );
}
