using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Fish : MonoBehaviour
{
    private List<Fish> flockmates;

    [SerializeField]
    private FlockingSettings flockingSettings;

    public float perceptionRadius;

    [Range(0, 180)]
    public float maxPerceptionAngle = 120;

    public float separationRadius;

    public float avoidanceRange;

    public float minSpeed;

    public float maxSpeed;

    private Rigidbody rb;

    private Vector3 cohereDir;
    private Vector3 alignDir;
    private Vector3 separateDir;

    private Vector3 acceleration;
    private Vector3 velocity;

    public Color cohesionColor;
    public Color alignmentColor;
    public Color separationColor;


    private void Start()
    {
        Initialize();
        GetComponent<SphereCollider>().isTrigger = true;
    }

    public void Initialize()
    {
        flockmates = new List<Fish>();
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * Random.Range(minSpeed, maxSpeed);
        cohereDir = Vector3.zero;
        alignDir = Vector3.zero;
        separateDir = Vector3.zero;
    }

    private void FixedUpdate()
    {
        UpdateAcceleration();

        UpdateVelocity();

        Move();

        transform.forward = velocity.normalized;
    }

    private void UpdateAcceleration()
    {
        acceleration = Vector3.zero;

        if (flockmates.Count > 0)
        {
            Vector3 flockCenter = Vector3.zero;
            int perceivedFlockmateCount = 0;
            int flockmatesToAvoidCount = 0;
            foreach (var fish in flockmates)
            {
                if (!CanSeeFlockmate(fish)) continue;

                perceivedFlockmateCount++;
                flockCenter += fish.rb.position;

                alignDir += fish.rb.velocity;

                if (Vector3.Distance(rb.position, fish.rb.position) <= separationRadius)
                {
                    flockmatesToAvoidCount++;
                    separateDir += rb.position - fish.rb.position;
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

        AvoidCollisions();
    }

    private void AvoidCollisions()
    {
        if (!IsApproachingObstacle()) return;
        var avoidanceDir = GetClearDir();
        print(avoidanceDir);
        acceleration += GetSteerForce(avoidanceDir) * flockingSettings.avoidanceStrength;
    }

    private void UpdateVelocity()
    {
        velocity = rb.velocity;
        velocity += acceleration * Time.fixedDeltaTime;
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
        rb.velocity = velocity;
    }


    private void OnValidate()
    {
        GetComponent<SphereCollider>().radius = perceptionRadius;
    }

    private bool CanSeeFlockmate(Fish flockmate)
    {
        if (!flockmates.Contains(flockmate)) return false;
        return Vector3.Angle(transform.forward, flockmate.rb.position - rb.position) <= maxPerceptionAngle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        var fish = other.GetComponent<Fish>();
        if (!fish) return;
        if (flockmates.Contains(fish)) return;
        flockmates.Add(fish);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        var fish = other.GetComponent<Fish>();
        if (!fish) return;
        if (!flockmates.Contains(fish)) return;
        flockmates.Remove(fish);
    }

    private bool IsApproachingObstacle()
    {
        return Physics.Raycast(rb.position, transform.forward, out var hit, avoidanceRange, flockingSettings.obstacleLayer);
    }

    private Vector3 GetClearDir()
    {
        float angleStep = 30 * Mathf.Deg2Rad;
        Ray ray = new Ray(rb.position, Vector3.zero);

        for (float curAngle = angleStep; curAngle <= 360; curAngle += angleStep)
        {
            var forward = transform.forward * Mathf.Cos(curAngle);
            var right = transform.right * Mathf.Sin(curAngle);

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

            curAngle += angleStep;
        }
        return transform.forward;
    }

    private Vector3 GetSteerForce(Vector3 dir)
    {
        var force = dir.normalized * maxSpeed - rb.velocity;
        return Vector3.ClampMagnitude(force, flockingSettings.maxSteerForce);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var pos = rb.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos, perceptionRadius);

        if (flockmates != null)
        {
            foreach (var fish in flockmates)
            {
                Gizmos.DrawLine(pos, fish.rb.position);
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
