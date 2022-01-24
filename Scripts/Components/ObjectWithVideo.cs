using UnityEngine;

public class ObjectWithVideo : MonoBehaviourEventful<ObjectWithVideo>
{
	[SerializeField] public bool DisableGameObjectWhenVideoNotAvailable = true;
	[SerializeField] public MeshRenderer MeshToReceiveVideoMaterial;
}