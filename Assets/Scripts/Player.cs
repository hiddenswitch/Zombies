using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	[Header("References")]
	public GameController gameController;
	public PlayerController playerController;
	public Animator animator;
	public CharacterController controller;
	public Assistance assistance;

	public PlayerStates playerState;

	[Header("Slide")]
	public float slideSpeed = 24;

	public Transform leftRail;
	public Transform middleRail;
	public Transform rightRail;
	
	public int currentRail = 0;
	
	private Vector3 slideDestination;
	public float arrivedEpsilon = 0.3f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.A)){
			animator.Play("Attack", 0, 0);

			SpawnManager.SpawnPrefab(Spawn.DustSlide, new Vector3(0, 0, 10));
		}

		UpdateMovement();
	}

	void UpdateMovement(){
		if (playerState == PlayerStates.Slide){
			controller.Move (GetDirection () * Time.deltaTime * slideSpeed);
		}
	}

	public void Slide (Vector3 direction)
	{
		if (direction.x > 0) {
			if (currentRail == -1){
				currentRail = 0;
				SetSlideDestination (middleRail.position);
			}else if (currentRail == 0){
				currentRail = 1;
				SetSlideDestination (rightRail.position);
			}
			SpawnManager.SpawnPrefab (Spawn.DustSlide, transform.position, PoolName.Default).localScale = new Vector3 (-1, 1, 1);
		} else {
			if (currentRail == 0){
				currentRail = -1;
				SetSlideDestination (leftRail.position);
			}else if (currentRail == 1){
				currentRail = 0;
				SetSlideDestination (middleRail.position);
			}
			SpawnManager.SpawnPrefab (Spawn.DustSlide, transform.position, PoolName.Default).localScale = Vector3.one;
		}
		gameController.audioManager.sfxSideStep.Play ();
	}

	public bool ReachedDestination ()
	{
		var distanceVector = Grounded (slideDestination) - Grounded (transform.position);
		bool reached = distanceVector.magnitude < arrivedEpsilon;
		
		if (playerState == PlayerStates.Slide && reached) {
			//   PlayerDidReachDestination ();
		}
		
		return reached;
	}

	public void SetSlideDestination (Vector3 destination)
	{
		if (playerState != PlayerStates.Idle && playerState != PlayerStates.Slide) {
			return;
		}
		ForcePlayerState (PlayerStates.Slide);
		slideDestination = destination;
	}

	public Vector3 GetDirection ()
	{
		if (ReachedDestination ()) {
			playerState = PlayerStates.Idle;
			return Vector3.zero;
		} else {
			return (Grounded (slideDestination) - Grounded (transform.position)).normalized;
		}
	}

	void ForcePlayerState(PlayerStates state){
		playerState = state;
	}

	public Vector3 Grounded (Vector3 vector)
	{
		vector.y = 0;
		return vector;
	}
}

public enum PlayerStates{
	Idle,
	Dead,
	Slide
}
