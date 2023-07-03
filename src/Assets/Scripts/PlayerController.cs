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
    LogicalInput logicalInput = new();

    private const int TRANS_TIME = 3;
    private const int ROT_TIME = 3;

    const int FALL_COUNT_UNIT = 120;
    const int FALL_COUNT_SPD = 10;
    const int FALL_COUNT_FAST_SPD = 20;
    const int GROUND_FRAMES = 50;

    Vector2Int position = new Vector2Int(2, 12);
    private RotState rotate = RotState.Up;

    private AnimationController animationController = new AnimationController();

    private Vector2Int lastPosition;
    private RotState lastRotate = RotState.Up;

    int _fallCount = 0;
    int _groundFrame = GROUND_FRAMES;

    // Start is called before the first frame update
    private void Start()
    {
        logicalInput.Clear();

        puyoControllers[0].SetPuyoType(PuyoType.Green);
        puyoControllers[1].SetPuyoType(PuyoType.Red);

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

    private void SetTransition(Vector2Int pos, RotState rotState, int transTime)
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

    void Settle()
    {
        // 直接接地
        bool isSet0 = boardController.Settle(position,
            (int)puyoControllers[0].GetPuyoType());
        Debug.Assert(isSet0); // 置いたのは空いていた場所のはず

        bool isSet1 = boardController.Settle(CalcChildPuyoPos(position, rotate),
            (int)puyoControllers[1].GetPuyoType());
        Debug.Assert(isSet1); // 置いたのは空いていた場所のはず

        gameObject.SetActive(false);
    }

    void QuickDrop()
    {
        // 落ちれる一番下まで落ちる
        Vector2Int pos = position;
        do
        {
            pos += Vector2Int.down;
        }
        while (CanMove(pos, rotate));

        pos -= Vector2Int.down; // 一つ上の場所（最後に置けた場所）に戻す

        position = pos;

        Settle();
    }


    static readonly KeyCode[] KeyCodeTbl = new KeyCode[(int)LogicalInput.Key.MAX]
    {
        KeyCode.RightArrow, // Right
        KeyCode.LeftArrow, // Left
        KeyCode.X, // RotR
        KeyCode.Z, // RotL
        KeyCode.UpArrow, // QuickDrop
        KeyCode.DownArrow, // Down
    };

    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;

        for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
        {
            if (Input.GetKey(KeyCodeTbl[i]))
            {
                inputDev |= (LogicalInput.Key)(1 << i);
            }
        }

        logicalInput.Update(inputDev);
    }

    bool Fall(bool isFast)
    {
        _fallCount -= isFast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

        while (_fallCount < 0)
        {
            if (!CanMove(position + Vector2Int.down, rotate))
            {
                _fallCount = 0;
                if (0 < --_groundFrame) return true;

                Settle();
                return false;
            }

            position += Vector2Int.down;
            lastPosition += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }

        return true;
    }

    void Control()
    {
        if (!Fall(logicalInput.IsRaw(LogicalInput.Key.Down))) return;

        if (animationController.Update()) return;

        // 平行移動のキー入力取得
        if (logicalInput.IsRepeat(LogicalInput.Key.Right))
        {
            if (Translate(true)) return;
        }

        if (logicalInput.IsRepeat(LogicalInput.Key.Left))
        {
            if (Translate(false)) return;
        }

        // 回転のキー入力取得
        if (logicalInput.IsTrigger(LogicalInput.Key.RotR)) // 右回転
        {
            if (Rotate(true)) return;
        }

        if (logicalInput.IsTrigger(LogicalInput.Key.RotL)) // 左回転
        {
            if (Rotate(false)) return;
        }

        // クイックドロップのキー入力取得
        if (logicalInput.IsRelease(LogicalInput.Key.QuickDrop))
        {
            QuickDrop();
        }
    }


    private void FixedUpdate()
    {
        UpdateInput();

        Control();

        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;
        float animRate = animationController.GetNormalized();
        puyoControllers[0].SetPos(dy + Interpolate(position, RotState.Invalid, lastPosition, RotState.Invalid, animRate));
        puyoControllers[1].SetPos(dy + Interpolate(position, rotate, lastPosition, lastRotate, animRate));
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