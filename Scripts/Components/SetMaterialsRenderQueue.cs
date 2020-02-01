using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialsRenderQueue : MonoBehaviour
{
	List<KeyValuePair<Material, int>> materials = new List<KeyValuePair<Material, int>>();


	private void Awake()
	{
		foreach ( var m in materials )
			m.Key.renderQueue = m.Value;
	}
}
