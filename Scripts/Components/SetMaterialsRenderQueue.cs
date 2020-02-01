using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialsRenderQueue : MonoBehaviour
{
	[System.Serializable]
	class MatWithQueue
	{
		public Material Material;
		public int Queue = 2501;
	}

	MatWithQueue[] materials = new MatWithQueue[0];


	private void Awake()
	{
		foreach ( var m in materials )
			m.Material.renderQueue = m.Queue;
	}
}
