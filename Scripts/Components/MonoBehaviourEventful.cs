using System;
using UnityEngine;

public abstract class MonoBehaviourEventful<T> : MonoBehaviour
    where T : MonoBehaviourEventful<T>
{
    //this also used in MonoSystem
    public static event Action<bool, T> Enabled;

    protected void OnEnable()
        => Enabled?.Invoke( true, (T) this );

    protected void OnDisable()
        => Enabled?.Invoke( false, (T) this );
}