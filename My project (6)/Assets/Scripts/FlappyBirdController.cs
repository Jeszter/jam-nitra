using UnityEngine;
using UnityEngine.InputSystem;

public class FlappyBirdController : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float flapForce = 5f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float maxFallSpeed = -10f;
    [SerializeField] private float maxRiseSpeed = 10f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxUpRotation = 30f;
    [SerializeField] private float maxDownRotation = -60f;
    
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 3f;
    
    private float verticalVelocity = 0f;
    private bool isGameActive = true;
    private Keyboard keyboard;
    
    void Start()
    {
        keyboard = Keyboard.current;
    }
    
    void Update()
    {
        if (!isGameActive) return;
        
        // Check for flap input (Space key) using new Input System
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            Flap();
        }
        
        // Apply gravity
        verticalVelocity -= gravity * Time.deltaTime;
        
        // Clamp vertical velocity
        verticalVelocity = Mathf.Clamp(verticalVelocity, maxFallSpeed, maxRiseSpeed);
        
        // Move the bird
        Vector3 movement = new Vector3(forwardSpeed * Time.deltaTime, verticalVelocity * Time.deltaTime, 0f);
        transform.position += movement;
        
        // Rotate based on velocity
        float targetRotation = Mathf.Lerp(maxDownRotation, maxUpRotation, (verticalVelocity - maxFallSpeed) / (maxRiseSpeed - maxFallSpeed));
        float currentZRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentZRotation);
    }
    
    private void Flap()
    {
        verticalVelocity = flapForce;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Game over on collision with pipes or ground
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Ground"))
        {
            GameOver();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Score point when passing through pipe gap
        if (other.CompareTag("ScoreZone"))
        {
            Debug.Log("Score!");
        }
    }
    
    private void GameOver()
    {
        isGameActive = false;
        Debug.Log("Game Over!");
    }
    
    public void RestartGame()
    {
        isGameActive = true;
        verticalVelocity = 0f;
        transform.position = new Vector3(-5f, 0f, 0f);
        transform.rotation = Quaternion.identity;
    }
}
