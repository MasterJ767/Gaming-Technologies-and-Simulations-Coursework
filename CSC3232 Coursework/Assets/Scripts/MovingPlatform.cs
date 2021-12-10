using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Controls moving platforms
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    public Vector3[] points;
    private Vector3 nextPoint;
    private int currentIndex = 0;

    public float speed;
    public float delayDuration;

    private List<NavMeshAgent> agents;

    private float arrivalTime;

    private void Start()
    {
        if (points.Length > 0)
        {
            nextPoint = points[currentIndex];
        }

        agents = new List<NavMeshAgent>();
    }

    private void FixedUpdate()
    {
        if (transform.position != nextPoint)
        {
            Move();
        }
        else
        {
            ChangeTarget();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            other.gameObject.transform.SetParent(transform);
        }
        else if (other.gameObject.layer == 8 && other.gameObject.name != "Head")
        {
            other.gameObject.transform.SetParent(transform);
            agents.Add(other.gameObject.GetComponent<NavMeshAgent>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            other.gameObject.transform.SetParent(null);
        }
        else if (other.gameObject.layer == 8 && other.gameObject.name != "Head")
        {
            other.gameObject.transform.SetParent(null);
            agents.Remove(other.gameObject.GetComponent<NavMeshAgent>());
        }
    }

    /// <summary>
    /// Move platform and its passengers towards the next stopping point
    /// </summary>
    private void Move()
    {
        Vector3 direction = nextPoint - transform.position;
        foreach (NavMeshAgent agent in agents)
        {
            agent.Warp(agent.transform.position + (direction.normalized * speed * Time.fixedDeltaTime));
        }
        transform.position += direction.normalized * speed * Time.fixedDeltaTime;
        if (direction.magnitude < speed * Time.fixedDeltaTime)
        {
            transform.position = nextPoint;
            arrivalTime = Time.time;
        }
    }
    
    /// <summary>
    /// Change the target of the platform to the next point in the target's array
    /// </summary>
    private void ChangeTarget()
    {
        if (Time.time >= arrivalTime + delayDuration)
        {
            currentIndex = (currentIndex + 1) % points.Length;
            nextPoint = points[currentIndex];
        }
    }
}
