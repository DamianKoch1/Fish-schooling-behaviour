using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField]
    private FlockingSettings flockingSettings;

    public float perceptionRadius;

    [Range(0, 180)]
    public float maxPerceptionAngle = 120;

    public float separationRadius;

    public float avoidanceRange;

    public float minSpeed;

    public float maxSpeed;

    private Vector3 cohereDir;
    private Vector3 alignDir;
    private Vector3 separateDir;

    private Vector3 acceleration;
    private Vector3 velocity;

    public Color cohesionColor;
    public Color alignmentColor;
    public Color separationColor;

    private FishSpawner spawner;



    public void Initialize(FishSpawner _spawner)
    {
        spawner = _spawner;
        velocity = transform.forward * Random.Range(minSpeed, maxSpeed);
        cohereDir = Vector3.zero;
        alignDir = Vector3.zero;
        separateDir = Vector3.zero;
    }

    public void RandomizeStats(float randomness = 0.1f)
    {
        if (randomness <= 0) return;
        perceptionRadius    *= Random.Range(1 - randomness, 1 + randomness);
        maxPerceptionAngle  *= Random.Range(1 - randomness, 1 + randomness);
        separationRadius    *= Random.Range(1 - randomness, 1 + randomness);
        minSpeed            *= Random.Range(1 - randomness, 1 + randomness);
        maxSpeed            *= Random.Range(1 - randomness, 1 + randomness);
    }

    public void RandomizeScale(float randomness = 0.1f)
    {
        if (randomness <= 0) return;
        transform.localScale *= Random.Range(1 - randomness, 1 + randomness);
    }

    private void Update()
    {
        UpdateAcceleration();

        UpdateVelocity();

        Move();
    }

    private void UpdateAcceleration()
    {
        acceleration = Vector3.zero;

        if (spawner.spawnedFish.Count > 0)
        {
            FlockingBehaviour();
        }

        AvoidCollisions();
    }

    private void FlockingBehaviour()
    {
        Vector3 flockCenter = Vector3.zero;
        int perceivedFlockmateCount = 0;
        int flockmatesToAvoidCount = 0;
        foreach (var fish in spawner.spawnedFish)
        {
            if (!IsFlockmate(fish)) continue;

            perceivedFlockmateCount++;
            flockCenter += fish.transform.position;

            alignDir += fish.velocity;

            if (Vector3.Distance(transform.position, fish.transform.position) <= separationRadius)
            {
                flockmatesToAvoidCount++;
                separateDir += transform.position - fish.transform.position;
            }
        }
        if (perceivedFlockmateCount > 0)
        {
            flockCenter /= perceivedFlockmateCount;
            alignDir /= perceivedFlockmateCount;
        }
        else
        {
            flockCenter = transform.position;
            alignDir = Vector3.zero;
        }

        if (flockmatesToAvoidCount > 0)
        {
            separateDir /= flockmatesToAvoidCount;
        }
        else
        {
            separateDir = Vector3.zero;
        }

        cohereDir = flockCenter - transform.position;

        var cohereForce = GetSteerForce(cohereDir);
        var alignForce = GetSteerForce(alignDir);
        var separateForce = GetSteerForce(separateDir);

        acceleration += cohereForce * flockingSettings.cohereStrength;
        acceleration += alignForce * flockingSettings.alignStrength;
        acceleration += separateForce * flockingSettings.separateStrength;
    }

    private void AvoidCollisions()
    {
        if (!IsApproachingObstacle()) return;
        var avoidanceDir = GetClearDir();
        acceleration += GetSteerForce(avoidanceDir) * flockingSettings.avoidanceStrength;
    }

    private void UpdateVelocity()
    {
        velocity += acceleration * Time.deltaTime;
        if (velocity.sqrMagnitude < minSpeed * minSpeed)
        {
            velocity = velocity.normalized * minSpeed;
        }
        else if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }
    }

    private void Move()
    {
        if (velocity == Vector3.zero) return;
        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized;
    }

    private bool IsFlockmate(Fish fish)
    {
        if (fish == this) return false;
        if (Vector3.Distance(transform.position, fish.transform.position) > perceptionRadius) return false;
        return Vector3.Angle(transform.forward, fish.transform.position - transform.position) <= maxPerceptionAngle;
    }

    private bool IsApproachingObstacle()
    {
        return Physics.Raycast(transform.position, transform.forward, out var hit, avoidanceRange, flockingSettings.obstacleLayer);
    }

    private Vector3 GetClearDir()
    {
        float angleStep = 15 * Mathf.Deg2Rad;
        Ray ray = new Ray(transform.position, Vector3.zero);

        for (float curAngle = angleStep; curAngle <= 360; curAngle += angleStep)
        {
            var forward = transform.forward * Mathf.Cos(curAngle);
            var right = transform.right * Mathf.Sin(curAngle);
            var up = transform.up * Mathf.Sin(curAngle);

            ray.direction = forward + right;
            if (!Physics.Raycast(ray, avoidanceRange, flockingSettings.obstacleLayer))
            {
                return ray.direction;
            }

            ray.direction = forward - right;
            if (!Physics.Raycast(ray, avoidanceRange, flockingSettings.obstacleLayer))
            {
                return ray.direction;
            }

            ray.direction = forward + up;
            if (!Physics.Raycast(ray, avoidanceRange, flockingSettings.obstacleLayer))
            {
                return ray.direction;
            }

            ray.direction = forward - up;
            if (!Physics.Raycast(ray, avoidanceRange, flockingSettings.obstacleLayer))
            {
                return ray.direction;
            }

            curAngle += angleStep;
        }
        return -transform.forward;
    }

    private Vector3 GetSteerForce(Vector3 dir)
    {
        var force = dir.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(force, flockingSettings.maxSteerForce);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!UnityEditor.Selection.Contains(gameObject)) return;

        var pos = transform.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos, perceptionRadius);

        if (spawner != null)
        {
            foreach (var fish in spawner.spawnedFish)
            {
                if (!IsFlockmate(fish)) continue;
                Gizmos.DrawLine(pos, fish.transform.position);
            }
        }

        Gizmos.DrawLine(pos, pos + Quaternion.AngleAxis(maxPerceptionAngle, transform.up) * transform.forward * perceptionRadius);
        Gizmos.DrawLine(pos, pos + Quaternion.AngleAxis(-maxPerceptionAngle, transform.up) * transform.forward * perceptionRadius);

        Gizmos.DrawLine(pos, pos + Quaternion.AngleAxis(maxPerceptionAngle, transform.right) * transform.forward * perceptionRadius);
        Gizmos.DrawLine(pos, pos + Quaternion.AngleAxis(-maxPerceptionAngle, transform.right) * transform.forward * perceptionRadius);

        Gizmos.color = separationColor;
        Gizmos.DrawLine(pos, pos + separateDir.normalized * 4);

        Gizmos.color = alignmentColor;
        Gizmos.DrawLine(pos, pos + alignDir.normalized * 4);

        Gizmos.color = cohesionColor;
        Gizmos.DrawLine(pos, pos + cohereDir.normalized * 4);


        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(pos + transform.forward * perceptionRadius * Mathf.Cos(Mathf.Deg2Rad * maxPerceptionAngle),
            transform.forward, Mathf.Sin(Mathf.Deg2Rad * maxPerceptionAngle) * perceptionRadius);

        Gizmos.color = Color.red;

    }
#endif
}
