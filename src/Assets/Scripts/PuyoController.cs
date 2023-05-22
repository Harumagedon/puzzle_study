using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuyoType
{
    Blank,
    Green,
    Red,
    Yellow,
    Blue,
    Purple,
    Cyan,
    Invalid,
}

[RequireComponent(typeof(Renderer))]
public class PuyoController : MonoBehaviour
{
    private static readonly Color[] color_table = new Color[]
    {
        Color.black,
        Color.green,
        Color.red,
        Color.yellow,
        Color.blue,
        Color.magenta,
        Color.cyan,
        Color.gray
    };

    [SerializeField] private Renderer my_renderer = default!;
    private PuyoType type = PuyoType.Invalid;

    public void SetPuyoType(PuyoType type)
    {
        this.type = type;

        my_renderer.material.color = color_table[(int)type];
    }

    public PuyoType GetPuyoType()
    {
        return type;
    }

    public void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }
}