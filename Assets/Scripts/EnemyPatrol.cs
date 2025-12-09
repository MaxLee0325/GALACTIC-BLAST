using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Apple;
using System.Collections;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
      
    GameObject player;
    NavMeshAgent agent;

    [SerializeField] LayerMask groundLayer, playerLayer;
    
    //patrol units 
     Vector3 destPoint;

     bool walkpointSet;
     
     // state change variables
     [SerializeField] private float sightRange, attackRange;
     bool playerInSightRange;
     private bool playerInAttackRange;

    [SerializeField]  float range;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] Animator anim;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        playerInAttackRange = Physics . CheckSphere(transform . position, attackRange, playerLayer) ;

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrol();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            Chase();
        }
        else if (playerInSightRange && playerInAttackRange)
        {
            Attack();
        }
        
    }

    void Chase()
    {
        anim.SetBool("IsWalking", true);
        anim.SetBool("IsAttacking", false);
        agent.SetDestination(player.transform.position);
    }

    void Attack()
    {
        //attack the player
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsAttacking", true);
        agent.SetDestination(transform.position);
    }

    void Patrol()
    {
        anim.SetBool("IsWalking", true);
        anim.SetBool("IsAttacking", false);
        if (!walkpointSet) searchForDestination() ;
        if (walkpointSet)
        {
            agent.SetDestination(destPoint); 
        }
        
        if (Vector3.Distance(transform.position, destPoint) < 10) walkpointSet = false ;
    }

    void searchForDestination()
    {
        float z = Random.Range(-range, range);
        float x = Random.Range(-range, range);
        
        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
        {
            walkpointSet = true;
        }
    }

    void OnHit()
    {
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsHit", true);
    }

    void Die()
    {
        anim.SetBool("IsDead", true);
        agent.isStopped = true;
    }
}
