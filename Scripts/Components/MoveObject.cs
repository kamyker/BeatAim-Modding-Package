using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviourEventful<MoveObject>
{
	[SerializeField] public Vector3 LocalDirection = Vector3.forward;
	[SerializeField] public float Speed = 1;
}
