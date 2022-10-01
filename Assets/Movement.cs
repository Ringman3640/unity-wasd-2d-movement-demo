using UnityEngine;
using System;

// Bound structure for storing camera view boundaries
public struct Bound
{
    public float top;
    public float left;
    public float bottom;
    public float right;
}

public class Movement : MonoBehaviour
{
    public float acceleration = 20f;            // How much velocity increases per second
    public float dampening = 20f;               // How much velocity decreases per second when idle
    public float maxSpeed = 5f;                 // Maximum velocity magnitude of the cube
    public float bounceCoefficient = 0.5f;      // Bounce energy retention when hiting walls

    private Rigidbody2D rb2d;
    private Bound screen;
    private bool screenInitialized = false;

    // Used in getDirection to set the magnitude for diagonal movements.
    // Need to add as a class member since C# doesn't support static locals (bruh).
    private static float diagMagnitude;
    
    void Start()
    {
        diagMagnitude = (float)Math.Cos(45 * (Math.PI / 180));
        rb2d = GetComponent<Rigidbody2D>();

        // Get main camera object
        GameObject mainCamera = GameObject.Find("Main Camera");

        // Null if main camera not found
        if (mainCamera == null)
        {
            return;
        }

        // Get camera component
        Camera cameraComp = mainCamera.GetComponent<Camera>();

        // Initialize screen camera bounds
        screen = new Bound();
        screen.top = cameraComp.orthographicSize;
        screen.bottom = -cameraComp.orthographicSize;
        screen.left = screen.bottom * 2;
        screen.right = screen.top * 2;
        screenInitialized = true;
    }

    void Update()
    {
        Vector2 velocity = rb2d.velocity;
        Vector2 direction = getDirection();

        // Apply accelerations
        applyAcceleration(ref velocity.x, direction.x);
        applyAcceleration(ref velocity.y, direction.y);

        // Bounds check
        checkCollision(ref velocity);

        // Update velocity
        rb2d.velocity = velocity;
    }

    // Accelerate or decelerate the cube given an input direction
    void applyAcceleration(ref float velocityComp, float directionComp)
    {
        // Accelerate
        if (directionComp != 0)
        {
            // Do nothing if already at max speed
            if (velocityComp == maxSpeed * directionComp)
            {
                return;
            }

            float maxSpeedComp = maxSpeed * Math.Abs(directionComp);
            if (Math.Abs(velocityComp) < maxSpeedComp || Math.Sign(velocityComp) != Math.Sign(directionComp))
            {
                // Increase velocity
                velocityComp += directionComp * acceleration * Time.deltaTime;
            }
            else
            {
                // Reduce from higher velocity
                float nextVelocity = velocityComp - (directionComp * acceleration * Time.deltaTime);
                if (Math.Abs(nextVelocity) < maxSpeedComp)
                {
                    velocityComp = maxSpeed * directionComp;
                }
                else
                {
                    velocityComp = nextVelocity;
                }
            }

            return;
        }

        // Decelerate
        if (velocityComp != 0)
        {
            // Decelerate
            bool prevSign = velocityComp >= 0;
            velocityComp -= dampening * Time.deltaTime * Math.Sign(velocityComp);

            // Check for overcorrection
            bool currSign = velocityComp >= 0;
            if (currSign != prevSign)
            {
                velocityComp = 0;
            }
        }
    }

    // Get the normalized vector directions of the user's WASD input
    Vector2 getDirection()
    {
        Vector2 direction = new Vector2(0f, 0f);

        // Get direction from inputs
        if (Input.GetKey(KeyCode.W))
        {
            direction.y += 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            direction.y -= 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            direction.x += 1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            direction.x -= 1;
        }

        // Normalize diagonal directions if necessary
        if (direction.x != 0 && direction.y != 0)
        {
            direction.x *= diagMagnitude;
            direction.y *= diagMagnitude;
        }

        return direction;
    }

    // Collision detection for the cube
    void checkCollision(ref Vector2 velocity)
    {
        if (!screenInitialized)
        {
            return;
        }

        float xSizeOffset = transform.lossyScale.x / 100 / 2;
        float ySizeOffset = transform.lossyScale.y / 100 / 2;
        Vector3 position = transform.position;

        // Top
        if (transform.position.y + ySizeOffset > screen.top)
        {
            position.y = screen.top - ySizeOffset;
            velocity.y *= -bounceCoefficient;
        }

        // Bottom
        if (transform.position.y - ySizeOffset < screen.bottom)
        {
            position.y = screen.bottom + ySizeOffset;
            velocity.y *= -bounceCoefficient;
        }

        // Right
        if (transform.position.x + xSizeOffset > screen.right)
        {
            position.x = screen.right - xSizeOffset;
            velocity.x *= -bounceCoefficient;
        }

        // Right
        if (transform.position.x - xSizeOffset < screen.left)
        {
            position.x = screen.left + xSizeOffset;
            velocity.x *= -bounceCoefficient;
        }

        transform.position = position;
    }
}
