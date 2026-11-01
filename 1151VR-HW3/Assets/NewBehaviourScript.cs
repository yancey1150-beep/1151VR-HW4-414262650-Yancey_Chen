using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class AircraftController : MonoBehaviour
{
    [Header("2D Side-View Movement")]
    public float moveSpeed = 10f;
    public float verticalSpeed = 8f;
    public float touchSensitivity = 0.15f;
    public bool invertForwardBackward = false;

    [Header("Audio Settings")]
    public AudioClip flySound;
    public AudioClip crashSound;
    public AudioClip passSound;

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;

        if (flySound != null)
        {
            audioSource.clip = flySound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void FixedUpdate()
    {
        float forwardBackward = 0f;
        float upDown = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) forwardBackward -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) forwardBackward += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) upDown -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) upDown += 1f;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
            forwardBackward = Mathf.Clamp(touchDelta.x * touchSensitivity, -1f, 1f);
            upDown = Mathf.Clamp(touchDelta.y * touchSensitivity, -1f, 1f);
        }

        if (invertForwardBackward)
        {
            forwardBackward = -forwardBackward;
        }

        rb.linearVelocity = new Vector3(0f, upDown * verticalSpeed, forwardBackward * moveSpeed);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (crashSound != null)
            {
                audioSource.PlayOneShot(crashSound);
            }
            Debug.Log("Hit Obstacle!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PassZone"))
        {
            if (passSound != null)
            {
                audioSource.PlayOneShot(passSound);
            }
            Debug.Log("Passed Obstacle!");
        }
    }
}
