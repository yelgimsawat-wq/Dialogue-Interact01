using System.Collections;
using UnityEngine;

public abstract class BaseDoor : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField]
    protected bool isRotatingDoor = true;

    [SerializeField]
    protected float speed = 1f;

    [Header("Rotation Config")]
    [SerializeField]
    protected float rotationAmount = 90f;

    [SerializeField]
    protected float forwardDirection = 0f;

    public bool IsOpen { get; protected set; }

    protected Vector3 startRotation;
    protected Vector3 forward;
    protected Coroutine animationCoroutine;

    protected virtual void Awake()
    {
        startRotation = transform.rotation.eulerAngles;
        forward = transform.right;
    }

    public virtual bool CanInteract => true;

    public virtual string InteractionText
    {
        get
        {
            if (!CanInteract) return "Door is locked.";
            return IsOpen ? "Press E to close door." : "Press E to open door.";
        }
    }

    public virtual void Interact()
    {
        if (!CanInteract)
        {
            Debug.Log($"[{gameObject.name}] Cannot interact (Door is locked).");
            return;
        }

        Vector3 userPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open(userPosition);
        }
    }

    public virtual void Open(Vector3 userPosition)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (isRotatingDoor)
        {
            float dot = Vector3.Dot(forward, (userPosition - transform.position).normalized);
            animationCoroutine = StartCoroutine(DoRotationOpen(dot));
        }
    }

    protected virtual IEnumerator DoRotationOpen(float forwardAmount)
    {
        Quaternion currentRot = transform.rotation;
        Quaternion endRotation;

        if (forwardAmount >= forwardDirection)
        {
            endRotation = Quaternion.Euler(new Vector3(0, currentRot.eulerAngles.y - rotationAmount, 0));
        }
        else
        {
            endRotation = Quaternion.Euler(new Vector3(0, currentRot.eulerAngles.y + rotationAmount, 0));
        }

        IsOpen = true;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(currentRot, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    public virtual void Close()
    {
        if (!IsOpen) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (isRotatingDoor)
        {
            animationCoroutine = StartCoroutine(DoRotationClose());
        }
    }

    protected virtual IEnumerator DoRotationClose()
    {
        Quaternion currentRot = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(startRotation);

        IsOpen = false;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(currentRot, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }
}
