using UnityEngine;

public delegate void PressedEventHandler (Vector2 pos);
public delegate void ReleasedEventHandler (Vector2 pos);
public delegate void DraggedEventHandler (Vector2 pos,float distance);
public delegate void FlickedEventHandler (Vector2 startPos,Vector2 endPos,float velocity,float distance);
public delegate void SwippedEventHandler (SwipeDir dir,Vector2 startPos,Vector2 endPos,float velocity,float distance);
public delegate void TappedEventHandler (Vector2 pos,float tapTime);
public enum SwipeDir
{
	Invalid = 0,
	Left,
	Right,
	Up,
	Down
}
/*
-----------------------

InputHandler

-----------------------
*/
public class InputHandler : MonoBehaviour
{
	private bool pressed = false;
	private float pressedStartTime = 0.0f;
	private Vector2 startPos;
	private float lastDistanceSquared = -1.0f;
	private float swipeDirectionTolerance = 0.2f;
	private bool isStationary = false;
	static public float	minSwipeDistance = 120f;
	static public float minSwipeDistanceSquared = 14400f;
	static public float passiveFlickMultiplier = 2f;
//1.6f;
	static public float tabletFlickScaler = 0.75f;
	static private float defaultDpi = 320f;
	// see below
	static private float dpiModifier = 1.0f;
	static private float tapTimeThreshold = 0.5f;
	static private float tapDistanceThreshold = 40;

	public event PressedEventHandler Pressed;
	public event ReleasedEventHandler Released;
	public event DraggedEventHandler Dragged;
	public event FlickedEventHandler Flicked;
	public event SwippedEventHandler Swiped;
	public event TappedEventHandler Tapped;

	public void OnPressed (Vector2 pos)
	{
		Pressed (pos);
	}

	public void OnReleased (Vector2 pos)
	{
		Released (pos);
	}

	public void OnDragged (Vector2 pos, float distance)
	{
		Dragged (pos, distance);
	}

	public void OnFlicked (Vector2 startPos,Vector2 endPos,float velocity,float distance)
	{
		Flicked (startPos, endPos, velocity, distance);
	}

	public void OnSwiped (SwipeDir dir,Vector2 startPos,Vector2 endPos,float velocity,float distance)
	{
		Swiped (dir, startPos, endPos, velocity, distance);
	}

	public void OnTapped (Vector2 pos,float tapTime)
	{
		Tapped (pos, tapTime);
	}
	/*
	-----------------------
	OnEnable()
	-----------------------
	*/
	void OnEnable ()
	{
#if !UNITY_EDITOR && ( UNITY_IPHONE || UNITY_ANDROID )
		if ( Screen.dpi >= defaultDpi ) {
			dpiModifier = 1f;
		} else {
			dpiModifier = ( defaultDpi / Screen.dpi ) * tabletFlickScaler;
		}
#endif		
	}
	/*
	-----------------------
	Update()
	Update the touch and mouse position and send out appropriate events.
	-----------------------
	*/
	void Update ()
	{
		if (Input.touchCount > 0) {
			// Touch notifications
			Touch touch = Input.GetTouch (0);
			if (touch.phase == TouchPhase.Began) {
				OnPressEvent (touch.position);
			} else if ((touch.phase == TouchPhase.Moved) || (touch.phase == TouchPhase.Stationary)) {
				OnDragEvent (touch.position);
			} else if (touch.phase != TouchPhase.Stationary) {
				OnReleaseEvent (touch.position);
			}
			//Debug.Log( "Touch: " + touch.phase + " Time: " + Time.time );
		} else {
			// Mouse notifications
			if (Input.GetMouseButtonDown (0)) {
				OnPressEvent (Input.mousePosition);
			}
			if (pressed && Input.GetMouseButtonUp (0)) {
				OnReleaseEvent (Input.mousePosition);	
			} 
			if (pressed && Input.GetMouseButton (0)) {
				OnDragEvent (Input.mousePosition);	
			} 
		}
	}
	/*
	-----------------------
	OnPressEvent()
	Send out a press notification.
	-----------------------
	*/
	void OnPressEvent (Vector2 pos)
	{
		startPos = pos;
		pressed = true;
		pressedStartTime = Time.time;
		isStationary = false;
		if (Pressed != null) {
			Pressed (pos);
		}
	}

	void OnTapEvent (Vector2 pos, float tapTime)
	{

	}
	/*
	-----------------------
	OnReleaseEvent()
	Send out a release notification.
	-----------------------
	*/
	void OnReleaseEvent (Vector2 pos)
	{

		if (pressed) {
			if (Released != null) {
				Released (pos);
			}
			//Custom Tap
			if (Time.time - pressedStartTime <= tapTimeThreshold && Vector2.Distance (pos, startPos) <= tapDistanceThreshold &&
			    Tapped != null) {
				Tapped (pos, Time.time - pressedStartTime);
			}

#if FLICK_ENABLED
			Vector2 delta = pos - startPos;
			float distance = delta.magnitude;
#if !UNITY_EDITOR && ( UNITY_IPHONE || UNITY_ANDROID )
				//-----------------------------------------------
				// adjust the swipe/flick distance for screen dpi
				// this will affect the velocity ramp of the throw
				// note: default dpi is from the iPhone5
				//-----------------------------------------------
				//Debug.Log( "**** distance before: " + distance.ToString( "F3" ) );
				distance *= dpiModifier;//( defaultDpi / Screen.dpi );
				//Debug.Log( "**** distance after: " + distance.ToString( "F3" ) );
#endif		
			if ( distance < minSwipeDistance ) {
				// regular button up

			} else {
				// this was a flick
				if ( OnFlick != null ) {
					//Debug.LogWarning( "Flick dist: " + distance.ToString( "F3" ) );
					float velocity = distance / ( Time.time - pressedStartTime );	
					if ( isStationary ) {
						// ramp up the velocity a bit
						velocity *= InputHandler.passiveFlickMultiplier;
					}
					OnFlick( startPos, pos, velocity, distance );
				}
				if ( OnSwipe != null ) {
					SwipeDir dir = SwipeDir.Invalid;
					if ( GetSwipeDirection( delta, out dir ) ) {
						float velocity = distance / ( Time.time - pressedStartTime );	
						OnSwipe( dir, startPos, pos, velocity, distance );
					}
				}
			}
#endif
		}



		pressed = false;
		isStationary = false;
		lastDistanceSquared = -1.0f;
	}
	/*
	-----------------------
	OnDragEvent()
	Send out a drag notification.
	-----------------------
	*/
	void OnDragEvent (Vector2 pos)
	{
		if (pressed) {
			Vector2 delta = pos - startPos;
			float distance = delta.sqrMagnitude;
			//Debug.Log( " >> distance: " + distance.ToString( "F3" ) + " lastDistanceSquared: " + lastDistanceSquared.ToString( "F3" ) );
			if (distance > 0.001f) {
#if BACKWARDS_SWIPE_DETECT
				if ( ( distance > minSwipeDistanceSquared ) && ( distance <= lastDistanceSquared ) ) {
					// if the swipe goes backwards or is stationary act as if it's a release event
					isStationary = true;
					OnReleaseEvent( pos );
				} else {
#endif
				isStationary = false;
				if (Dragged != null) {
					Dragged (pos, distance);
#if BACKWARDS_SWIPE_DETECT
					}
#endif
				}
				lastDistanceSquared = distance;
			}
		}
	}
	/*
	-----------------------
	GetSwipeDirection()
	-----------------------
	*/
	bool GetSwipeDirection (Vector2 delta, out SwipeDir swipeDirection)
	{
		float minSwipeDot = Mathf.Clamp01 (1.0f - swipeDirectionTolerance); 
		Vector2 dir = delta.normalized;
		if (Vector2.Dot (dir, Vector2.right) >= minSwipeDot)
			swipeDirection = SwipeDir.Right;
		else if (Vector2.Dot (dir, -Vector2.right) >= minSwipeDot)
			swipeDirection = SwipeDir.Left;
		else if (Vector2.Dot (dir, Vector2.up) >= minSwipeDot)
			swipeDirection = SwipeDir.Up;
		else if (Vector2.Dot (dir, -Vector2.up) >= minSwipeDot)
			swipeDirection = SwipeDir.Down;
		else {
			swipeDirection = SwipeDir.Invalid;
		}
		return (swipeDirection != SwipeDir.Invalid);		
	}
	//	/*
	//	-----------------------
	//	Raycast()
	//	Helper function that raycasts into the screen to determine what's underneath the specified position.
	//	-----------------------
	//	*/
	//	GameObject Raycast (Vector2 pos)
	//	{
	//		RaycastHit hit;
	//
	//		if (Physics.Raycast(mCam.ScreenPointToRay(pos), out hit, 300f, eventReceiverMask))
	//		{
	//			worldPos = hit.point;
	//			return hit.collider.gameObject;
	//		}
	//		return null;
	//	}

	#region IEventPublisher implementation

	#endregion

	#region IEventPublisher implementation
	/*
	public void AddEventListener (Tennis.ITouchDelegate d)
	{
		Pressed += d.OnPressEventHandler;
		Released += d.OnReleaseEventHandler;
		Dragged += d.OnDragEventHandler;
		Tapped += d.OnTapEventHandler;
	}

	public void RemoveEventListener (Tennis.ITouchDelegate d)
	{
		Pressed -= d.OnPressEventHandler;
		Released -= d.OnReleaseEventHandler;
		Dragged -= d.OnDragEventHandler;
		Tapped -= d.OnTapEventHandler;
	}*/

	#endregion

	void OnDestroy()
	{
		Pressed = null;
		Released = null;
		Dragged = null;
		Flicked = null;
		Swiped = null;
		Tapped = null;
	}
}