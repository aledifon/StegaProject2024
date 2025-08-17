using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ElevatorMovement : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Vector2 pointEndOffset;
    Vector2[] patrolPoints = new Vector2[2];          // Patrol's points positions    

    Vector3 targetPosition;
    int indexTargetPos;

    // Movement vars
    [Header("Movement")]
    [SerializeField] int normalSpeed;             // Elevator's normal speed
                                                  // 

    BoxCollider2D collider;
    [SerializeField, Range(0f, 5f)] float waitingTime;
    bool waitingTimerEnabled;
     

    void Awake()
    {                
        // Get all the patrol's points positions        
        patrolPoints[0] = transform.position;
        patrolPoints[1] = (Vector2)transform.position + pointEndOffset;

        // Set the initial Target Pos
        indexTargetPos = 0;
        // Set the initial patrol position
        targetPosition = patrolPoints[indexTargetPos];        

        collider = GetComponentInChildren<BoxCollider2D>();
        if (collider == null)
            Debug.LogError("Collider Not found on any child of the Platform");
    }    
    private void Update()
    {
        UpdateTargetPosition();
        if(!waitingTimerEnabled)
            Patrol();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(transform, true);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null, true);
        }
    }
    void UpdateTargetPosition()
    {
        // Update the patrol target points
        if (Vector2.Distance(transform.position, targetPosition) < Mathf.Epsilon)
        {
            if (indexTargetPos == patrolPoints.Length - 1)
                indexTargetPos = 0;
            else
                indexTargetPos++;

            targetPosition = patrolPoints[indexTargetPos];
            StartCoroutine(nameof(EnableWaitingTimer));
        }
    }
    IEnumerator EnableWaitingTimer()
    {
        waitingTimerEnabled = true;
        yield return new WaitForSeconds(waitingTime);
        waitingTimerEnabled = false;
    }
    void Patrol()
    {        
        // Update the ant's position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, normalSpeed * Time.deltaTime);
    }    
}
