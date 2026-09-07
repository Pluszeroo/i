using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementControls : MonoBehaviour
{
    [SerializeField] private GameObject head, tail;

    [field: SerializeField]
    private List<Tuple<Vector3, float>> _positionTimePairs = new();

    private Vector3 _newTailPosition;
    private float _time;

    // have starting position and ending position
    // every time the position from starting position reaches above a certain threshold, start movement of tail
    // once the tail reaches the finish position, you stop all movement of tail

    public void OnMove(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();

        head.transform.position += new Vector3(input.x, input.y, 0);
    }
}
