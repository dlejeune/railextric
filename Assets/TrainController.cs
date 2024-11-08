using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class TrainController : MonoBehaviour
{
    [SerializeField] private SplineContainer rail;
    private Rigidbody rb;
    private Spline currentSpline;

    [SerializeField] private float power;
    [SerializeField] private int speedFallOff;
    [SerializeField] private int maxSpeed;
    [SerializeField] private float friction;

    private Vector3 previousVelocity;


    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody>();

        currentSpline = rail.Splines[0];

        previousVelocity = new Vector3(0, 0, 0);
        transform.position = new Vector3(-81, 205, -143);

    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        var native = new NativeSpline(currentSpline);

        var local_point = rail.transform.InverseTransformPoint(transform.position);

        float distance = SplineUtility.GetNearestPoint(native,
            local_point,
            out float3 nearest,
            out float t);

        nearest = rail.transform.TransformPoint(nearest);

        transform.position = nearest;
        // new Vector3(nearest[0], nearest[1], nearest[2]) + new Vector3(-81, 205, -143);

        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t));
        Vector3 up = native.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 1, 0);
        var remappedUp = new Vector3(1, 0, 0);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;

        Vector3 engineForward = transform.up;

        // if (Vector3.Dot(rb.velocity, transform.forward) < 0)
        // {
        //     engineForward *= -1;
        //     Debug.Log("Changing Dir");

        // }

        rb.velocity = rb.velocity.magnitude * engineForward;

        // rb.velocity = rb.velocity.magnitude * engineForward;


        var max_speed = 50;

        var current_adj_speed = (currentSpline.EvaluateCurvature(t) + 1) * rb.velocity.magnitude;
        if (current_adj_speed > max_speed)
        {
            Debug.Log("Too fast!");
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 dir = power * transform.forward;
            rb.AddForce(dir, ForceMode.Impulse);

        }
        else
        {

            ApplyFriction(friction);
            // rb.velocity = rb.velocity.magnitude * -engineForward;
        }
    }

    private void Throttle(float power)
    {

        Vector3 dir = power * transform.forward;
        rb.AddForce(dir, ForceMode.Impulse);

    }

    private void ApplyFriction(float friction)
    {
        if (Vector3.Dot(rb.velocity, transform.forward) > 0)
        {
            // rb.AddForce(friction * -transform.forward, ForceMode.Impulse);
            rb.velocity -= rb.velocity * friction;
        }



    }

}
