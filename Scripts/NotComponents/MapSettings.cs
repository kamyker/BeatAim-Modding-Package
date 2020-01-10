using UnityEngine;

[CreateAssetMenu( fileName = "MapSettings", menuName = "ScriptableObjects/MapSettings", order = 1 )]
public class MapSettings : ScriptableObject
{
	public bool DefaultMapEnabled = false;
	public bool DefaultCloudsEnabled = true;
	public MinMaxSmoothFloat VisualizeToMusic_GlobalScaling = new MinMaxSmoothFloat(0,1,100, false);
}
