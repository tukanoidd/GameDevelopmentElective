using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(BoxCollider))]
public class Ship : MonoBehaviour
{
    enum Direction
    {
        Left,
        Right
    }

    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
    [Range(1, 100)] public float health = 100;
    [Range(10, 50)] public float speed = 10;
    [Range(10, 25)] public float rotationSpeed = 10;
    [Range(5, 15)] public float reloadSpeed = 5;

    [NonSerialized] public bool dying = false;

    [SerializeField] [Range(70, 100)] private float accuracy = 90;
    [SerializeField] private VisionSphere visionSphere;

    private HashSet<Ship> _visibleShips = new HashSet<Ship>();
    private BoxCollider _shipCollider;
    private CharacterController _shipController;

    private bool _rotating = false;
    private bool _moving = false;

    private void Awake()
    {
        UpdatePositionRotation();

        _shipCollider = GetComponent<BoxCollider>();
        _shipController = GetComponent<CharacterController>();
    }

    private void Start()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
        CannonBall checkCannonBall = other.gameObject.GetComponent<CannonBall>();
        if (checkCannonBall) ApplyDamage(checkCannonBall.damage);
    }
    
    private void UpdatePositionRotation()
    {
        position = transform.position;
        rotation = transform.rotation;
    }

    private void ApplyDamage(float damage)
    {
        health = Mathf.Clamp(health - damage, 0, 100);

        if (Mathf.Abs(health) < 0.05f) Death();
    }

    private void Death()
    {
        dying = true;

        //todo explosion animation
        CompetitionManager.current.RemoveShip(this);

        Destroy(gameObject);
    }

    private IEnumerator Rotate(float angle, Direction direction)
    {
        if (_moving) yield break;

        _rotating = true;
        
        int numFrames = (int) (angle / (rotationSpeed * Time.fixedDeltaTime));
        for (int frame = 0; frame < numFrames; frame++)
        {
            Debug.Log("Rotating");
            transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime * (direction == Direction.Left ? -1 : 1), 0f);

            yield return new WaitForFixedUpdate();
        }

        _rotating = false;
    }

    private IEnumerator MoveForward(float distance)
    {
        if (_rotating) yield break;

        _moving = true;
        
        float distanceWent = 0f;
        Vector3 startPos;
        
        while (distanceWent <= distance)
        {
            startPos = transform.position;
            _shipController.SimpleMove(transform.forward * speed);
            distanceWent += Vector3.Distance(startPos, transform.position);
            yield return null;
        }

        _moving = false;
    } 

    public void CheckDeadShip(Ship ship)
    {
        if (_visibleShips.Contains(ship)) _visibleShips.Remove(ship);
    }

    public void AddVisibleShip(Ship ship)
    {
        _visibleShips.Add(ship);
    }

    public void RemoveVisibleShip(Ship ship)
    {
        _visibleShips.Remove(ship);
    }
}