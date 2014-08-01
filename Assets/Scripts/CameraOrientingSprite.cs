using UnityEngine;
using System.Collections;

public class CameraOrientingSprite : MonoBehaviour
{
	public bool eulerXAllign = true;
	public bool eulerYAllign = true;
	public bool eulerZAllign = true;

	// Use this for initialization
	void Awake ()
	{
	}

	void Start()
	{
		Orient ();
	}

	void Update()
	{
		Orient ();
	}

	public void Orient(Camera camera = null)
	{
		camera = camera ?? Camera.main;

		EulerAngleOrient(camera);
	}

	private void EulerAngleOrient(Camera camera)
	{
		Vector3 euler = transform.eulerAngles;

		if (eulerXAllign){
			euler.x = camera.transform.eulerAngles.x;
		}
		if (eulerYAllign){
			euler.y = camera.transform.eulerAngles.y;
		}
		if (eulerZAllign){
			euler.z = camera.transform.eulerAngles.z;
		}

		transform.eulerAngles = euler;
	}
}
