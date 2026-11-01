Unity 3D Side-View Flying Obstacle Game & iOS Simulator Build Guide

I. Project Setup & Material Fix
1. Open Unity and create a 3D (URP) project.
2. Import your vehicle model (e.g., CartoonCarFree package), obstacle model, and three audio clips (flying loop, crash sound, and pass obstacle sound) into the Assets folder.
3. Navigate to Assets -> CartoonCarFree -> Prefabs, drag the car prefab (FreeCar) into your Scene, and set its Transform Position and Rotation to (0, 0, 0) in the Inspector.
4. If the model appears bright purple/magenta (missing URP shader), go to the top menu: Window -> Rendering -> Render Pipeline Converter. Set the dropdown to Built-in to URP, check Material Upgrade, and click Initialize Converters followed by Convert Assets.

II. Player Components & Tag Configuration
1. Select the car (FreeCar) in the Hierarchy and verify or add the following components in the Inspector:
   - Box Collider: Ensure it fits the car body (leave Is Trigger unchecked).
   - Rigidbody: Uncheck Use Gravity, and under Constraints, check Freeze Position X and Freeze Rotation X, Y, Z.
   - Audio Source: Used to play the sound effects.
2. Click the Tag dropdown at the very top of the Inspector, select Add Tag..., and click the "+" button to create two custom tags: Obstacle and PassZone (match the exact capitalization).

III. Obstacle & PassZone Setup
1. Create the Obstacle: Add a 3D Cube to the scene, set its Tag to Obstacle, and ensure its Box Collider has Is Trigger unchecked. Set its Transform to Position (0, 0, 12.22), Rotation (0, 90, 0), and Scale (0.5, 5, 10).
2. Create the PassZone: Create an Empty GameObject (or invisible Cube), set its Tag to PassZone, add a Box Collider, and check Is Trigger. Set its Transform to Position (0, 5, 12.22), Rotation (0, 90, 0), and Scale (0.5, 5, 10) so it sits directly above the obstacle as a score/pass trigger gate.

IV. 2D Side-View Camera Setup
1. Drag the Main Camera onto the car object in the Hierarchy to make it a child object (or attach a smooth camera follow script).
2. Set the Main Camera's Transform to Position (12, 1, 0) and Rotation (0, -90, 0) so it views the car from the side as a 2D plane (optionally switch Camera Projection from Perspective to Orthographic).

V. Flight Control & Audio C# Script (With Annotations)
1. Attach a C# script named AircraftController to the car and paste the following code:

using UnityEngine;
using UnityEngine.InputSystem; // Uses the New Input System to avoid InvalidOperationException

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class AircraftController : MonoBehaviour
{
    [Header("2D Side-View Movement")]
    public float moveSpeed = 10f;              // Forward/backward speed along the Z-axis
    public float verticalSpeed = 8f;           // Up/down speed along the Y-axis
    public float touchSensitivity = 0.15f;     // Mobile touch drag sensitivity
    public bool invertForwardBackward = false; // Flips horizontal direction if controls feel inverted

    [Header("Audio Settings")]
    public AudioClip flySound;                 // Looping sound while flying
    public AudioClip crashSound;               // Sound played when hitting an obstacle
    public AudioClip passSound;                // Sound played when passing through a PassZone

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Disable gravity and lock X position and all rotations to keep movement strictly on the Y-Z plane
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;

        // Start playing the looping flight sound on launch
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

        // 1. Read desktop keyboard input (A/D or Left/Right for forward/back, W/S or Up/Down for vertical)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) forwardBackward -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) forwardBackward += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) upDown -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) upDown += 1f;
        }

        // 2. Read mobile touchscreen drag input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
            forwardBackward = Mathf.Clamp(touchDelta.x * touchSensitivity, -1f, 1f);
            upDown = Mathf.Clamp(touchDelta.y * touchSensitivity, -1f, 1f);
        }

        // Reverse forward/backward direction if toggled in the Inspector
        if (invertForwardBackward)
        {
            forwardBackward = -forwardBackward;
        }

        // Keep X velocity at 0; move only along Y (up/down) and Z (forward/backward)
        rb.linearVelocity = new Vector3(0f, upDown * verticalSpeed, forwardBackward * moveSpeed);
    }

    // Solid collision: Play crash sound when hitting an "Obstacle" without locking controls
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

    // Trigger detection: Play pass sound when flying through a "PassZone" without interrupting the flight loop
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

2. Back in the Unity Inspector on the car object, drag your three audio clips into the Fly Sound, Crash Sound, and Pass Sound slots on the AircraftController component. Check Invert Forward Backward if A and D move the car in the opposite direction of your camera view.

VI. Exporting to Xcode & Running on the iOS Simulator
1. Go to Edit -> Project Settings -> Player, select the iOS tab, and expand Other Settings:
   - Check Override Default Bundle Identifier and replace the default com.Unity-Technologies.com.unity.template.urp-blank with a unique reverse-domain string (e.g., com.yanceychen.flyingcar2026).
   - Locate Target SDK: Change it from Device SDK to Simulator SDK to run on the Xcode iOS Simulator (switch back to Device SDK when building for a physical iPhone).
2. Open File -> Build Settings (or Build Profiles), switch to the iOS platform, click Build, and export into a brand-new folder (do not overwrite or Append onto an existing Device SDK folder, or Xcode will fail with an [iOS, arm64] vs. iOS-simulator architecture mismatch).
3. Open the exported .xcodeproj file in Xcode:
   - Under Signing & Capabilities, check Automatically manage signing, select your Personal Team, and verify that the Bundle Identifier shows no yellow warning icons.
   - Press Shift + Cmd + K (Product -> Clean Build Folder) to clear any cached build artifacts.
   - Select any iPhone Simulator from the top device dropdown menu and press Cmd + R to build, launch, and test touch dragging with your mouse.
