using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectWorld : MonoBehaviourEventful<MoveObjectWorld>
{
	protected override MoveObjectWorld _this => this;

	[SerializeField] public Vector3 WorldDirection = Vector3.forward;
	[SerializeField] public float Speed = 1;
}
