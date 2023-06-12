
using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
	public float gravity = 15;
	public float radius;
	public LayerMask terrainMask;
	public GameObject explosionPrefab;
	public GenTest genTest;

	// System vars
	bool hasExploded = false;
	Vector3 desiredLocalVelocity;
	Transform cameraTransform;
	Rigidbody rigidBody;
	CapsuleCollider capsuleCollider;

	void Awake()
	{
		//Cursor.visible = false;
		cameraTransform = Camera.main.transform;
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.useGravity = false;
		rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		capsuleCollider = GetComponent<CapsuleCollider>();
		Time.fixedDeltaTime = 1f / 60f;
	}

	void FixedUpdate()
	{
		Vector3 planetCentre = Vector3.zero;
		Vector3 gravityUp = (rigidBody.position - planetCentre).normalized;

		// Align body's up axis with the centre of planet
		Vector3 localUp = MathUtility.LocalToWorldVector(rigidBody.rotation, Vector3.up);
		rigidBody.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidBody.rotation;

		rigidBody.velocity = CalculateNewVelocity(localUp);
	}

	void LateUpdate()
	{
		if (terraUpdate)
		{
			Vector3 localUp = MathUtility.LocalToWorldVector(rigidBody.rotation, Vector3.up);
			//Debug.Log("Update");
			TerraTest(localUp);
			terraUpdate = false;
			//Debug.Break();
		}
	}
	void OnCollisionEnter(Collision collision)
	{
		if (!hasExploded)
		{
			hasExploded = true;

			// Get the position of the bomb upon collision
			Vector3 bombPosition = transform.position;

			// Spawn the explosion prefab at the bomb position
			Instantiate(explosionPrefab, bombPosition, Quaternion.identity);
			genTest.Terraform(bombPosition, 5f, radius);

			// Destroy the bomb object
			Destroy(gameObject);
		}
	}

	void TerraTest(Vector3 localUp)
	{
		float heightOffset = 5f;
		Vector3 a = transform.position - localUp * (capsuleCollider.height / 2 + capsuleCollider.radius - heightOffset);
		Vector3 b = transform.position + localUp * (capsuleCollider.height / 2 + capsuleCollider.radius + heightOffset);
		RaycastHit hitInfo;

		if (Physics.CapsuleCast(a, b, capsuleCollider.radius, -localUp, out hitInfo, heightOffset, terrainMask))
		{
			hp = hitInfo.point;
			Vector3 newPos = (hp + transform.up * 1);
			float deltaY = Vector3.Dot(transform.up, (newPos - transform.position));
			if (deltaY > 0.05f)
			{
				transform.position = newPos;
			}
		}

	}

	public void NotifyTerrainChanged(Vector3 point, float radius)
	{
		float dstFromCam = (point - cameraTransform.position).magnitude;
		if (dstFromCam < radius + 3)
		{
			terraUpdate = true;
		}
	}

	bool terraUpdate;
	Vector3 hp;

	Vector3 CalculateNewVelocity(Vector3 localUp)
	{
		// Apply movement and gravity to rigidbody
		float deltaTime = Time.fixedDeltaTime;
		Vector3 currentLocalVelocity = MathUtility.WorldToLocalVector(rigidBody.rotation, rigidBody.velocity);

		float localYVelocity = currentLocalVelocity.y + (-gravity) * deltaTime;

		Vector3 desiredGlobalVelocity = MathUtility.LocalToWorldVector(rigidBody.rotation, desiredLocalVelocity);
		desiredGlobalVelocity += localUp * localYVelocity;
		return desiredGlobalVelocity;
	}

	//bool IsGrounded()
	//{

	//	Vector3 centre = rigidBody.position;
	//	Vector3 upDir = transform.up;

	//	Vector3 castOrigin = centre + upDir * (-capsuleCollider.height / 2f + capsuleCollider.radius);
	//	float groundedRayRadius = capsuleCollider.radius * groundedRaySizeFactor;

	//	float groundedRayDst = capsuleCollider.radius - groundedRayRadius + groundedRayLength;
	//	RaycastHit hitInfo;

	//	if (Physics.SphereCast(castOrigin, groundedRayRadius, -upDir, out hitInfo, groundedRayDst, terrainMask))
	//	{
	//		return true;
	//	}

	//	return false;
	//}
}