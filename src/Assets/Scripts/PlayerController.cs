using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PuyoController[] puyoControllers = new PuyoController[2] { default!, default! };
    [SerializeField] private BoardController boardController = default!;

    private Vector2Int position;

    // Start is called before the first frame update
    private void Start()
    {
        puyoControllers[0].SetPuyoType(PuyoType.Green);
        puyoControllers[1].SetPuyoType(PuyoType.Red);

        position = new Vector2Int(2, 12);

        puyoControllers[0].SetPos(new Vector3((float)position.x, (float)position.y, 0.0f));
        puyoControllers[1].SetPos(new Vector3((float)position.x, (float)position.y + 1.0f, 0.0f));
    }

    private bool CanMove(Vector2Int pos)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(pos + Vector2Int.up)) return false;

        return true;
    }

    private bool Translate(bool is_right)
    {
        Vector2Int pos = position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos)) return false;

        position = pos;

        puyoControllers[0].SetPos(new Vector3((float)position.x, (float)position.y, 0.0f));
        puyoControllers[1].SetPos(new Vector3((float)position.x, (float)position.y + 1.0f, 0.0f));

        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Translate(true);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Translate(false);
        }
    }
}