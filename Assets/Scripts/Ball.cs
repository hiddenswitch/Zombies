using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

	[HideInInspector]public GameController gameController;
	public TrailRenderer trailRenderer;
	public Transform ballObject;

	public bool gravityActive = true;
	public float baseGravity = 65f;
	[HideInInspector]public float gravity = 55f;
	[HideInInspector]public bool aftertouchActive;

	public float aftertouchForce = 100;
	public float maxXVelocity = 15;
	public float despawnTime = 1;
	private float despawnTimer;

	public Transform rotationObject;

	public float maxRotationY = 30;
	public float baseObjectRotation = 90;


	void OnSpawned(){
		Init();
	}
	// Use this for initialization
	void Start () {
		Init();
	}

	void Init(){
		gameController = FindObjectOfType<GameController>();
		despawnTimer = despawnTime;
		gravity = baseGravity;
	}
	
	// Update is called once per frame
	void Update () {
		despawnTimer -= Time.deltaTime;
		if(despawnTimer <= 0){
			SpawnManager.DespawnPrefab(this.transform, PoolName.Default);
		}

		Vector3 ballObjectRotation = ballObject.localEulerAngles;

		float inverseLerp = Mathf.InverseLerp(-8.5f, 8.5f, rigidbody.velocity.x);

		print (inverseLerp);
	
		ballObjectRotation.y = baseObjectRotation + Mathf.Lerp(-maxRotationY, maxRotationY, inverseLerp);

		ballObject.localEulerAngles = ballObjectRotation;
	}
	
	void FixedUpdate ()
	{
		ApplyGravity ();
		
		if (!aftertouchActive){
			return;
		}
		
		var impulse = gameController.player.playerController.afterTouchDirection.normalized;
		impulse.y = 0;
		impulse.z = 0;
		impulse.x *= aftertouchForce;
		
		
		if ((impulse.x > 0 && rigidbody.velocity.x < maxXVelocity) || (impulse.x < 0 && rigidbody.velocity.x > -maxXVelocity)){
			rigidbody.AddForce(impulse * Time.fixedDeltaTime, ForceMode.VelocityChange);
		}
	}

	void ApplyGravity ()
	{
		if (gravityActive) {
			rigidbody.AddForce (new Vector3 (0, -gravity, 0), ForceMode.Acceleration);
		}
	}

	public void Hit (Vector3 force)
	{
		ClearForces ();
		rigidbody.AddForce (force, ForceMode.VelocityChange);
	}
	
	public void ClearForces ()
	{
		rigidbody.Sleep ();
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		rigidbody.WakeUp ();
	}
	
	public Vector3 GetGroundedVector (Vector3 vector)
	{
		vector.y = 0;
		return vector;
	}
}
