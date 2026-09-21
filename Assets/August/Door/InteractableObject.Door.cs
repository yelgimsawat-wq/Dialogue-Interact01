using System.Collections;
using UnityEngine;

public partial class InteractableObject
{
    [SerializeField]
    private GameObject door;

    [SerializeField]
    [Tooltip("จุดบานพับ ใช้แกน Y ของ Pivot เป็นแกนหมุน หากไม่ใส่จะหมุนรอบจุดกำเนิดของ Door")]
    private Transform doorPivot;

    public bool IsOpen = false;

    [SerializeField]
    private bool IsRotatingDoor = true;

    [SerializeField]
    [Min(0.01f)]
    private float Speed = 1f;

    [Header("Rotation Config")]
    [SerializeField]
    private float RotationAmount = 90f;

    [SerializeField]
    private float ForwardDirection = 0f;

    private Transform doorTransform;
    private Vector3 closedDoorPosition;
    private Quaternion closedDoorRotation;
    private Vector3 pivotPosition;
    private Vector3 pivotAxis;
    private Vector3 closedDoorRight;
    private float currentDoorAngle;
    private Coroutine AnimationCoroutine;

    private void Awake()
    {
        doorTransform = door != null ? door.transform : transform;
        closedDoorPosition = doorTransform.localPosition;
        closedDoorRotation = doorTransform.localRotation;

        Vector3 worldPivot = doorPivot != null ? doorPivot.position : doorTransform.position;
        Vector3 worldAxis = doorPivot != null ? doorPivot.up : doorTransform.up;
        Transform parent = doorTransform.parent;
        pivotPosition = parent != null ? parent.InverseTransformPoint(worldPivot) : worldPivot;
        pivotAxis = parent != null ? parent.InverseTransformDirection(worldAxis).normalized : worldAxis.normalized;
        closedDoorRight = closedDoorRotation * Vector3.right;
    }

    partial void DoorUpdate()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open(transform.position);
        }
    }

    partial void Open(Vector3 UserPosition)
    {
        if (!IsRotatingDoor) return;

        Transform parent = doorTransform.parent;
        Vector3 worldPivot = parent != null ? parent.TransformPoint(pivotPosition) : pivotPosition;
        Vector3 worldRight = parent != null ? parent.TransformDirection(closedDoorRight) : closedDoorRight;
        float dot = Vector3.Dot(worldRight, (UserPosition - worldPivot).normalized);
        float targetAngle = dot >= ForwardDirection ? -RotationAmount : RotationAmount;
        IsOpen = true;
        StartDoorRotation(targetAngle);
    }

    partial void Close()
    {
        if (!IsOpen || !IsRotatingDoor) return;

        IsOpen = false;
        StartDoorRotation(0f);
    }

    private void StartDoorRotation(float targetAngle)
    {
        if (AnimationCoroutine != null)
        {
            StopCoroutine(AnimationCoroutine);
        }

        AnimationCoroutine = StartCoroutine(RotateDoor(targetAngle));
    }

    private IEnumerator RotateDoor(float targetAngle)
    {
        float startAngle = currentDoorAngle;
        float time = 0f;
        while (time < 1f)
        {
            time = Mathf.Min(1f, time + Time.deltaTime * Mathf.Max(0.01f, Speed));
            ApplyDoorAngle(Mathf.Lerp(startAngle, targetAngle, time));
            yield return null;
        }

        ApplyDoorAngle(targetAngle);
        AnimationCoroutine = null;
    }

    private void ApplyDoorAngle(float angle)
    {
        currentDoorAngle = angle;
        Quaternion rotation = Quaternion.AngleAxis(angle, pivotAxis);
        // Keep the hinge in the parent's coordinates, even when Pivot is a child
        // of the moving door. Re-reading it each frame would move the rotation center.
        doorTransform.localPosition = pivotPosition + rotation * (closedDoorPosition - pivotPosition);
        doorTransform.localRotation = rotation * closedDoorRotation;
    }
}
