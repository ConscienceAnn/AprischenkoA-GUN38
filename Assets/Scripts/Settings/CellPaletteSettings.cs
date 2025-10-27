using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CellPalette", menuName = "Settings/Cell Palette", order = 0)]
public class CellPaletteSettings : ScriptableObject
{
    [field: SerializeField, Space(20f)]
    [field: Tooltip("Клетка под выбранным юнитом")]

    public Material SelectCell { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Клетка доступная для передвижения")]

    public Material MoveCell { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Клетка доступная для атаки")]

    public Material AttackCell { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Клетка доступная и для атаки и для движения")]

    public Material MoveAndAttackCell { get; private set; }    

}
