﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    enum State
    {
        Alive,
        Dying,
        Transcending
    };

    State state = State.Alive;

    // Debug Mode
    private bool isCollisionDisabled = false;

    [SerializeField] float levelLoadDelay = 2.31f;

    // Rocket Thrust
    [SerializeField] float rcsThrust = 250f;
    [SerializeField] float mainThrust = 50f;

    // Audio
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    // Particles
    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    Rigidbody rigidbody;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCollisionDisabled = !isCollisionDisabled; //toggle
            print("Collision enabled: " + isCollisionDisabled);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !isCollisionDisabled)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;

        successParticles.Play();

        audioSource.Stop();
        audioSource.PlayOneShot(success);

        Invoke(nameof(LoadNextLevel), levelLoadDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;

        deathParticles.Play();

        audioSource.Stop();
        audioSource.PlayOneShot(death);
        // Loads current level
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Invoke(nameof(LoadFirstLevel), levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void RespondToRotateInput()
    {
        rigidbody.freezeRotation = true; // Takes manual control of the rotation
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidbody.freezeRotation = false; // Resume physics control of rotation
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidbody.AddRelativeForce(Vector3.up * (mainThrust * Time.deltaTime));
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }

        mainEngineParticles.Play();
    }
}