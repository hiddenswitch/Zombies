using UnityEngine;
using System.Collections;

public class Assistance : MonoBehaviour {

	public AnimationCurve assistanceByDeviation;
	
	public Transform leftPos;
	public Transform rightPos;
	
	
	public Vector3 AssistedShotVector (Ball ball, Vector3 intendedDirectionNormalized)
	{
		Vector3 ballPos = ball.transform.position;
		
		Vector3 leftPosDir = (leftPos.position - ballPos).normalized;
		Vector3 rightPosDir = (rightPos.position - ballPos).normalized;
		
		float eulerSwingY = Quaternion.LookRotation (intendedDirectionNormalized, Vector3.forward).eulerAngles.y;
		float eulerLeftY = Quaternion.LookRotation (leftPosDir, Vector3.forward).eulerAngles.y;
		float eulerRightY = Quaternion.LookRotation (rightPosDir, Vector3.forward).eulerAngles.y;
		
		//Workaround to be able to compare angles
		eulerSwingY = eulerSwingY < 180 ? eulerSwingY + 360 : eulerSwingY;
		eulerLeftY = eulerLeftY < 180 ? eulerLeftY + 360 : eulerLeftY;
		eulerRightY = eulerRightY < 180 ? eulerRightY + 360 : eulerRightY;
		
		if (eulerSwingY < eulerRightY && eulerSwingY > eulerLeftY) {
		} else if (eulerSwingY >= eulerRightY) {
			eulerSwingY = Mathf.Lerp(eulerSwingY, eulerRightY, assistanceByDeviation.Evaluate(Mathf.Abs(eulerSwingY - eulerRightY)));
		} else if (eulerSwingY <= eulerLeftY) {
			eulerSwingY = Mathf.Lerp(eulerSwingY, eulerLeftY, assistanceByDeviation.Evaluate(Mathf.Abs(eulerSwingY - eulerLeftY)));
		}
		
		ball.rotationObject.eulerAngles = new Vector3(0, eulerSwingY, 0);
		
		return ball.rotationObject.forward;
	}
}
