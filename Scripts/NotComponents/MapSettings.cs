using UnityEngine;

[CreateAssetMenu( fileName = "MapSettings", menuName = "ScriptableObjects/MapSettings", order = 1 )]
public class MapSettings : ScriptableObject
{
	public bool DefaultMapEnabled = false;
	public bool DefaultCloudsEnabled = true;
	public bool DefaultVideoBillboardsEnabled = true;
	public float VisualizeToMusic_GlobalScaling = 1f;
}
