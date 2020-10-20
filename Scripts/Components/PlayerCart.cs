using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCart : MonoBehaviourEventful<PlayerCart>
{
	protected override PlayerCart _this => this;
	public int Priority = 0;
	public Animator Animator;

	private void Reset()
	{
		if ( Animator == null )
			Animator = GetComponent<Animator>();
	}
}
