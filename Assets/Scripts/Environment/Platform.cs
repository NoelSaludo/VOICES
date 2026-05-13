using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(PlatformEffector2D))]
public class Platform : MonoBehaviour
{
	[SerializeField] private float dropThroughDuration = 0.25f;
	[SerializeField] private float maxDropThroughDuration = 0.75f;
	[SerializeField] private float topCheckTolerance = 0.05f;
	[SerializeField] private float clearBelowTolerance = 0.02f;
	[SerializeField] private float minDropVelocity = 1.5f;
	[SerializeField] private string playerTag = "Player";

	private Collider2D platformCollider;
	private PlatformEffector2D effector;
	private readonly HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

	private void Awake()
	{
		platformCollider = GetComponent<Collider2D>();
		effector = GetComponent<PlatformEffector2D>();

		if (platformCollider != null)
		{
			platformCollider.usedByEffector = true;
		}

		if (effector != null)
		{
			effector.useOneWay = true;
			effector.useOneWayGrouping = true;
			effector.surfaceArc = 180f;
		}

	}

	public void RequestDrop(Collider2D playerCollider)
	{
		if (platformCollider == null || playerCollider == null)
		{
			return;
		}

		if (!playerCollider.CompareTag(playerTag))
		{
			return;
		}

		if (!IsPlayerAbovePlatform(playerCollider))
		{
			return;
		}

		if (!ignoredColliders.Contains(playerCollider))
		{
			StartCoroutine(DropThroughRoutine(playerCollider));
		}
	}

	private bool IsPlayerAbovePlatform(Collider2D playerCollider)
	{
		Bounds playerBounds = playerCollider.bounds;
		Bounds platformBounds = platformCollider.bounds;
		return playerBounds.min.y >= platformBounds.max.y - topCheckTolerance;
	}

	private bool IsPlayerBelowPlatform(Collider2D playerCollider)
	{
		Bounds playerBounds = playerCollider.bounds;
		Bounds platformBounds = platformCollider.bounds;
		return playerBounds.max.y <= platformBounds.min.y + clearBelowTolerance;
	}

	private IEnumerator DropThroughRoutine(Collider2D playerCollider)
	{
		ignoredColliders.Add(playerCollider);
		Physics2D.IgnoreCollision(platformCollider, playerCollider, true);

		Rigidbody2D playerBody = playerCollider.attachedRigidbody;
		if (playerBody != null && minDropVelocity > 0f)
		{
			Vector2 velocity = playerBody.linearVelocity;
			if (velocity.y > -minDropVelocity)
			{
				velocity.y = -minDropVelocity;
				playerBody.linearVelocity = velocity;
			}
		}

		float minEndTime = Time.time + dropThroughDuration;
		float maxEndTime = Time.time + Mathf.Max(dropThroughDuration, maxDropThroughDuration);
		while (Time.time < maxEndTime)
		{
			if (playerCollider == null)
			{
				break;
			}

			if (Time.time >= minEndTime && IsPlayerBelowPlatform(playerCollider))
			{
				break;
			}

			yield return null;
		}

		if (playerCollider != null)
		{
			Physics2D.IgnoreCollision(platformCollider, playerCollider, false);
		}

		ignoredColliders.Remove(playerCollider);
	}

	private void OnValidate()
	{
		if (platformCollider == null)
		{
			platformCollider = GetComponent<Collider2D>();
		}

		if (effector == null)
		{
			effector = GetComponent<PlatformEffector2D>();
		}

		if (platformCollider != null)
		{
			platformCollider.usedByEffector = true;
		}

		if (effector != null)
		{
			effector.useOneWay = true;
			effector.useOneWayGrouping = true;
			if (effector.surfaceArc <= 0f)
			{
				effector.surfaceArc = 180f;
			}
		}
	}
}