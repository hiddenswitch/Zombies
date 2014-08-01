using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public Player player;
	public AudioManager audioManager;
	public InputHandler inputHandler;
	public Camera mainCamera;
	public Camera guiCamera;

	public Collider groundCollider;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Vector3? ToCourtSpace (Vector2 screenPoint)
	{
		if (mainCamera == null) {
			return null;
		}
		
		Ray ray = mainCamera.ScreenPointToRay (screenPoint);
		RaycastHit hit;
		if (groundCollider.Raycast (ray, out hit, mainCamera.farClipPlane)) {
			return hit.point;
		}
		
		return null;
	}
}
