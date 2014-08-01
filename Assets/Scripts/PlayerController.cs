using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	[Header("References")]
	public GameController gameController;
	public Player player;
	public Transform swipeTrail;

	[Header("Swipe Recognition")]
	public SwipeStates swipeState;
	public float vectorFowardThreshold = 0.5f;
	public float vectorHorizontalThreshold = 0.8f;
	public float hitStrength = 60;
	public float hitStrengthY = 15;

	private Vector2 oldFingerPosition;
	private Vector2 dragStartPosition;
	private Vector2 lastAftertouchPosition;
	private float dragStartTime;
	public float deltaSwipeThreshold = 17f;
	public float swipeRecognitionRadius = 100f;
	public float aftertouchRecognitionRadius = 40;

	//Delta Average
	private int deltaSampleIndex = 0;
	private float[] deltaSampleTouchDistancesForSmoothing = new float[3];
	private float deltaAverage;

	[HideInInspector]public Vector3 afterTouchDirection;
	[HideInInspector]public Ball currentAftertouchBall;


	//public GameObject ball;

	void Start () {
		Subscriptions(true);
	}

	void Subscriptions(bool enable)
	{
		if (enable){
			gameController.inputHandler.Pressed += OnPressEventHandler;
			gameController.inputHandler.Released += OnReleaseEventHandler;
			gameController.inputHandler.Dragged += OnDragEventHandler;
		}else{
			gameController.inputHandler.Pressed -= OnPressEventHandler;
			gameController.inputHandler.Released -= OnReleaseEventHandler;
			gameController.inputHandler.Dragged -= OnDragEventHandler;
		}
	}

	void Update () {
	
	}

	void DeltaCalculations (Vector2 screenPosition)
	{
		deltaSampleTouchDistancesForSmoothing [deltaSampleIndex] = Vector2.Distance (screenPosition, oldFingerPosition);
		deltaSampleIndex = (deltaSampleIndex + 1) % deltaSampleTouchDistancesForSmoothing.Length;
		oldFingerPosition = screenPosition;
		
		float deltaSum = 0;
		for (int i = 0; i < deltaSampleTouchDistancesForSmoothing.Length; i++) {
			deltaSum += deltaSampleTouchDistancesForSmoothing [i];
		}
		deltaAverage = deltaSum / deltaSampleTouchDistancesForSmoothing.Length;
	}

	void ResetDelta ()
	{
		for (int i = 0; i < deltaSampleTouchDistancesForSmoothing.Length; i++) {
			deltaSampleTouchDistancesForSmoothing [i] = 0;
		}
	}

	void ResetAftertouch(){
		afterTouchDirection = Vector3.zero;
		//if (currentAftertouchBall != null){
		//	currentAftertouchBall.aftertouchActive = false;
		//	currentAftertouchBall = null;
		//}
	}

	void ResetDragInformation (Vector2 lastFingerPosition)
	{
		dragStartPosition = lastFingerPosition;
		dragStartTime = Time.time;
		oldFingerPosition = lastFingerPosition;
		ResetDelta ();
	}

	void MoveTrailRenderer (Vector3 pos)
	{
		swipeTrail.transform.position = pos;
	}

	#region Input

	public void OnPressEventHandler (Vector2 pos)
	{
		ResetDragInformation(pos);
	}
	
	public void OnReleaseEventHandler (Vector2 pos)
	{
		swipeState = SwipeStates.None;
		ResetDragInformation (pos);
		ResetAftertouch();
	}
	
	public void OnDragEventHandler (Vector2 screenPosition, float distance)
	{
		if (player.playerState == PlayerStates.Dead){
			return;
		}

		MoveTrailRenderer (gameController.guiCamera.ScreenToWorldPoint (screenPosition) + new Vector3 (0, 0, 1));

		if (swipeState == SwipeStates.None) {
			oldFingerPosition = screenPosition;
			swipeState = SwipeStates.DragActive;
		}

		if (swipeState == SwipeStates.Aftertouch){
			if (Vector2.Distance(screenPosition, lastAftertouchPosition) >= aftertouchRecognitionRadius){
				Vector3 dirVector = (gameController.ToCourtSpace(screenPosition).GetValueOrDefault() - gameController.ToCourtSpace(lastAftertouchPosition).GetValueOrDefault()).normalized;
				dirVector.y = 0;
				lastAftertouchPosition = screenPosition;
				afterTouchDirection = dirVector;
			}
		}

		DeltaCalculations (screenPosition);

		if (swipeState == SwipeStates.DragActive) {
			if (deltaAverage >= deltaSwipeThreshold) {
				swipeState = SwipeStates.SwipeActive;
			}
		}

		if (swipeState == SwipeStates.SwipeActive) {
			
			var swipeStartCourtSpace = gameController.ToCourtSpace (dragStartPosition).GetValueOrDefault ();
			var swipeEndCourtSpace = gameController.ToCourtSpace (screenPosition).GetValueOrDefault ();
			var direction = (swipeEndCourtSpace - swipeStartCourtSpace).normalized;
			
			var passedSwipeThresholdRadius = Vector2.Distance (screenPosition, dragStartPosition) > swipeRecognitionRadius;
			
			var isForwardSwipe = Vector3.Dot (direction, -(new Vector3 (0, 0, player.transform.position.z)).normalized) < vectorFowardThreshold;
			var isHorizontalSwipe = Mathf.Abs (Vector3.Dot (direction, Vector3.right)) > vectorHorizontalThreshold;

			print (Vector3.Dot (direction, -(new Vector3 (0, 0, player.transform.position.z)).normalized));

			if (passedSwipeThresholdRadius) {                
				if (isForwardSwipe) {
					
					swipeState = SwipeStates.Aftertouch;
					ResetDragInformation (screenPosition);
					print ("SHOT");

					/*
					float ballCharge = matchController.manager.HitBallId();
					
					ZombieModeBall ball = SpawnManager.SpawnPrefab (Spawn.ZombieModeBall, player.transform.position + new Vector3 (0, 3, 0), PoolName.Default).GetComponent<ZombieModeBall> ();
					*/
					Ball ball = SpawnManager.SpawnPrefab (Spawn.Ball, player.transform.position + new Vector3 (0, 3, 0), PoolName.Default).GetComponent<Ball> ();

					ball.transform.position = player.transform.position + new Vector3(0, 3, 1);
					player.animator.Play("Attack", 0, 0);
					gameController.audioManager.sfxBallHit.Play();
					
					Vector3 assistedDir = player.assistance.AssistedShotVector(ball, direction);
					
					currentAftertouchBall = ball;
					currentAftertouchBall.aftertouchActive = true;

					//matchController.manager.ResetCharge(0);
					ball.Hit (new Vector3 (assistedDir.x * hitStrength, hitStrengthY, assistedDir.z * hitStrength)); 
					//ball.SetBall(ballCharge);
				} 
				else if (isHorizontalSwipe) {
					player.Slide (direction);
					ResetDragInformation (screenPosition);
					swipeState = SwipeStates.SwipePerformed;

					/*
					matchController.player.SideStep (direction);
					ResetDragInformation (screenPosition);
					swipeState = SwipeStates.SwipePerformed;
					*/
				}
			}
		}
	}

	#endregion
}

public enum SwipeStates{
	None,
	DragActive,
	SwipeActive,
	Aftertouch,
	SwipePerformed
}
