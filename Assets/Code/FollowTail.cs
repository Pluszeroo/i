using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(SpriteRenderer))]
public class FollowTail : MonoBehaviour
{
    public Transform leader;
    public Movement movement;
    public GameObject reverseMessage;

    public float pointSpacing = 0.15f;
    public float restLength = 5f;
    public float minLength = 1f;
    public float lengthRecoverSpeed = 2f;

    public float fleeTriggerDistance = 3f;
    public float fleeSpeedMultiplier = 1.6f;

    public float onPathTolerance = 0.4f;
    [Range(90f, 180f)] public float backAngle = 120f;

    public float unlockDistance = 40f;
    public float mergeApproachSpeed = 0.5f;
    public float mergeThreshold = 0.1f;
    public UnityEvent onMerge;
    public float stillTimeToMerge = 5f;

    public Color pathColor = new Color(1, 1, 1, 0.25f);
    [Range(0f, 1f)] public float whiteAlpha = 0.4f;

    private List<Vector3> path = new List<Vector3>();
    private LineRenderer line;
    private SpriteRenderer sr;
    private Vector3 lastLeaderPos;
    private float totalDistance = 0f;
    private float currentLength;
    private bool canMerge, merged, reverseShown, revealed;
    private float stillTimer = 0f;

    public Vector3 messageOffset = new Vector3(0.6f, 0.8f, 0f);

    public float hitDistance = 0.6f;

    [Range(0f, 1f)] public float mergeViewportX = 0.5f;
    [Range(0f, 1f)] public float mergeViewportY = 0.6f;
    public float cameraMoveDuration = 1.5f;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();
        currentLength = restLength;
        sr.enabled = false;
        SetAlpha(whiteAlpha);
        if (leader != null)
        {
            lastLeaderPos = leader.position;
            transform.position = leader.position;
            path.Add(leader.position);
        }
        if (reverseMessage != null) reverseMessage.SetActive(false);
    }

    void Update()
    {
        if (leader == null || merged) return;

        totalDistance += Vector3.Distance(leader.position, lastLeaderPos);
        lastLeaderPos = leader.position;
        if (!canMerge && totalDistance >= unlockDistance) canMerge = true;

        if (movement == null || !movement.IsKnocking) RecordHead();
        UpdateLength();

        bool stopped = movement == null || !movement.IsMoving;
        if (stopped) stillTimer += Time.deltaTime;
        else stillTimer = 0f;

        if (canMerge && stopped && revealed)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, leader.position, mergeApproachSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, leader.position) <= mergeThreshold)
            { DoMerge(); return; }
        }
        else
        {
            transform.position = ComputeTailAndTrim();
            CheckCollision();
        }

        if (!revealed && PathLength() >= currentLength - 0.01f)
        { revealed = true; sr.enabled = true; }

        if (reverseShown && reverseMessage != null)
            reverseMessage.transform.position = leader.position + messageOffset;
        DrawLine();
    }

    public void ResetPathAfterKnockback()
    {
        path.Clear();
        path.Add(leader.position);
        currentLength = restLength;
        transform.position = leader.position;
        revealed = false;
        sr.enabled = false;
    }

    void CheckCollision()
    {
        if (!revealed || movement == null || movement.IsKnocking) return;
        if (Vector3.Distance(transform.position, leader.position) <= hitDistance)
            movement.Knockback(transform.position);
    }

    void UpdateLength()
    {
        float d = Vector3.Distance(transform.position, leader.position);
        float speed = movement != null ? movement.MoveSpeed : 5f;

        if (d < fleeTriggerDistance && movement != null && movement.IsMoving)
            currentLength -= speed * fleeSpeedMultiplier * Time.deltaTime;
        else
            currentLength += lengthRecoverSpeed * Time.deltaTime;

        currentLength = Mathf.Clamp(currentLength, minLength, restLength);
    }

    void RecordHead()
    {
        if (path.Count == 0) { path.Add(leader.position); return; }
        if (Vector3.Distance(leader.position, path[path.Count - 1]) >= pointSpacing)
            path.Add(leader.position);
    }

    float PathLength()
    {
        float t = 0f;
        for (int i = 1; i < path.Count; i++) t += Vector3.Distance(path[i], path[i - 1]);
        return t;
    }

    Vector3 ComputeTailAndTrim()
    {
        float remaining = currentLength;
        for (int i = path.Count - 1; i >= 1; i--)
        {
            float seg = Vector3.Distance(path[i], path[i - 1]);
            if (seg >= remaining)
            {
                Vector3 tail = Vector3.Lerp(path[i], path[i - 1], remaining / seg);
                if (i - 1 > 0) path.RemoveRange(0, i - 1);
                return tail;
            }
            remaining -= seg;
        }
        return path.Count > 0 ? path[0] : transform.position;
    }

    void DrawLine()
    {
        line.positionCount = path.Count + 1;
        line.SetPosition(0, transform.position);
        for (int i = 0; i < path.Count; i++) line.SetPosition(i + 1, path[i]);
        line.startColor = pathColor;
        line.endColor = pathColor;
    }

    public bool IsBacktracking(Vector2 pos, Vector2 dir)
    {
        if (merged || path.Count < 3) return false;

        int nearest = -1; float best = float.MaxValue;
        for (int i = 0; i < path.Count; i++)
        {
            float d = Vector2.Distance(pos, path[i]);
            if (d < best) { best = d; nearest = i; }
        }
        if (best > onPathTolerance) return false;
        if (nearest >= path.Count - 2) return false;

        Vector2 forward = (Vector2)(path[nearest + 1] - path[nearest]);
        if (forward.sqrMagnitude < 0.0001f) return false;
        return Vector2.Angle(forward.normalized, dir) > backAngle;
    }

    public void SetReverseMessage(bool show)
    {
        if (show == reverseShown) return;
        reverseShown = show;
        if (reverseMessage != null) reverseMessage.SetActive(show);
    }

    void DoMerge()
    {
        merged = true;
        transform.position = leader.position;
        line.enabled = false;
        SetReverseMessage(false);
        if (movement != null) movement.frozen = true;

        StartCoroutine(MoveCameraToFrame());
    }

    System.Collections.IEnumerator MoveCameraToFrame()
    {
        Camera cam = Camera.main;

        var brain = cam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        Vector3 from = cam.transform.position;

        Vector3 viewportOffset = cam.ViewportToWorldPoint(new Vector3(mergeViewportX, mergeViewportY, 0f))
                               - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        Vector3 to = leader.position - viewportOffset;
        to.z = from.z;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / cameraMoveDuration;
            cam.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        onMerge?.Invoke(); 
        Debug.Log("Merged");
    }

    void SetAlpha(float a) { Color c = sr.color; c.a = a; sr.color = c; }
}