using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum RotState
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        Invalid = -1
    }

    [SerializeField] private PuyoController[] puyoControllers = new PuyoController[2] { default!, default! };
    [SerializeField] private BoardController boardController = default!;

    private const float TRANS_TIME = 0.05f;
    private const float ROT_TIME = 0.05f;

    enum RotState
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        Invalid = -1,
    }

    private Vector2Int position;
    private RotState rotate = RotState.Up;

    private AnimationController animationController = new AnimationController();
    private Vector2Int lastPosition;
    private RotState lastRotate = RotState.Up;

    // Start is called before the first frame update
    private void Start()
    {
        puyoControllers[0].SetPuyoType(PuyoType.Green);
        puyoControllers[1].SetPuyoType(PuyoType.Red);

        position = new Vector2Int(2, 12);

        puyoControllers[0].SetPos(new Vector3((float)position.x, (float)position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(position, rotate);
        puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));
    }

    static readonly Vector2Int[] RotateTbl = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos, RotState rot)
    {
        return pos + RotateTbl[(int)rot];
    }

    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;
        
        return true;
    }

    private bool Translate(bool isRight)
    {
        Vector2Int pos = position + (isRight ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, rotate)) return false;

        SetTransition(pos, rotate, TRANS_TIME);

        return true;
    }

    private void SetTransition(Vector2Int pos, RotState rotState, float transTime)
    {
        lastPosition = position;
        lastRotate = rotate;

        position = pos;
        rotate = rotState;

        animationController.Set(transTime);
    }

    bool Rotate(bool isRight)
    {
        RotState rot = (RotState)(((int)rotate + (isRight ? +1 : +3)) & 3);
        Vector2Int pos = position;

        switch (rot)
        {
            case RotState.Down:
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                    !boardController.CanSettle(pos + new Vector2Int(isRight ? 1 : -1, -1)))
                {
                    pos += Vector2Int.up;
                }

                break;
            case RotState.Right:
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;
            case RotState.Left:
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }

        if (!CanMove(pos, rot)) return false;

        SetTransition(pos, rot, ROT_TIME);

        return true;
    }

    void QuickDrop()
    {
        Vector2Int pos = position;
        do
        {
            pos += Vector2Int.down;
        }
        while (CanMove(pos, rotate));

        pos -= Vector2Int.down;

        position = pos;

        bool isSet0 = boardController.Settle(position,
            (int)puyoControllers[0].GetPuyoType());
        Debug.Assert(isSet0);

        bool isSet1 = boardController.Settle(CalcChildPuyoPos(position, rotate),
            (int)puyoControllers[1].GetPuyoType());
        Debug.Assert(isSet1);

        gameObject.SetActive(false);
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Translate(true)) return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Translate(false)) return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (Rotate(true)) return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (Rotate(false)) return;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            QuickDrop();
        }
    }

    private void Update()
    {
        if (!animationController.Update(Time.deltaTime)) // アニメ中はキー入力を受け付けない
        {
            Control();
        }

        float animRate = animationController.GetNormalized();
        puyoControllers[0].SetPos(Interpolate(position, RotState.Invalid, lastPosition, RotState.Invalid, animRate));
        puyoControllers[1].SetPos(Interpolate(position, rotate, lastPosition, lastRotate, animRate));
    }

    static Vector3 Interpolate(Vector2Int pos, RotState rot, Vector2Int posLast, RotState rotLast, float rate)
    {
        Vector3 p = Vector3.Lerp(
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)posLast.x, (float)posLast.y, 0.0f), rate);

        if (rot == RotState.Invalid) return p;

        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rotLast;
        float theta = theta1 - theta0;

        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;

        theta = theta0 + rate * theta;

        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }
}