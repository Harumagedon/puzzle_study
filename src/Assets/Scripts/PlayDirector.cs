using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController playerController = null;
    LogicalInput logicalInput = new();

    NextQueue nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };// ��next�̃Q�[���I�u�W�F�N�g�̐���

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        logicalInput.Clear();
        playerController.SetLogicalInput(logicalInput);

        nextQueue.Initialize();
        Spawn(nextQueue.Update());
        UpdateNextsView();
    }

    void UpdateNextsView()
    {
        nextQueue.Each( (int idx, Vector2Int n) => {
            nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
        });
    }

    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)LogicalInput.Key.MAX]{
        KeyCode.RightArrow, // Right
        KeyCode.LeftArrow,  // Left
        KeyCode.X,          // RotR
        KeyCode.Z,          // RotL
        KeyCode.UpArrow,    // QuickDrop
        KeyCode.DownArrow,  // Down
    };

    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;

        for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (LogicalInput.Key)(1 << i);
            }
        }

        logicalInput.Update(inputDev);
    }


    void FixedUpdate()
    {
        UpdateInput();

        if (!player.activeSelf)
        {
            Spawn(nextQueue.Update());
            UpdateNextsView();
        }
    }

    bool Spawn(Vector2Int next) => playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}