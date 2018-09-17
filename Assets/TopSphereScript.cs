using Leap.Unity.Interaction;
using UnityEngine;
[RequireComponent(typeof(InteractionBehaviour))]
public class TopSphereScript : MonoBehaviour {
	private InteractionBehaviour _intObj;
	public GameObject _map;
	void Start() {
		_intObj = GetComponent<InteractionBehaviour>();
	}

	public void onBeginContact() {
		_map.transform.position = new Vector3 (_map.transform.position.x, -8.0f, _map.transform.position.z);
	}
}
