using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DogController : MonoBehaviour
{
    public Transform cameraPivot;
    public Transform dogModel;

    public float moveSpeed = 3.5f;
    public float jumpForce = 1.2f;
    public float gravity = -9.8f;
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 75f;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Look();
        Move();
    }

    private void Look()
    {
        if (Mouse.current == null) return;

        float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
        float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Поворот строго вокруг мировой оси Y
        transform.Rotate(Vector3.up, mouseX, Space.World);

        // Модель смотрит туда же, куда камера по горизонтали
        if (dogModel != null)
        {
            Vector3 rot = dogModel.eulerAngles;
            rot.y = transform.eulerAngles.y;
            dogModel.eulerAngles = rot;
        }
    }

    private void Move()
    {
        if (Keyboard.current == null || cameraPivot == null) return;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed) z += 1;
        if (Keyboard.current.sKey.isPressed) z -= 1;
        if (Keyboard.current.aKey.isPressed) x -= 1;
        if (Keyboard.current.dKey.isPressed) x += 1;

        // Берём направление камеры, но без наклона вверх/вниз
        Vector3 forward = cameraPivot.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraPivot.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * z + right * x;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveSpeed * move;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}
