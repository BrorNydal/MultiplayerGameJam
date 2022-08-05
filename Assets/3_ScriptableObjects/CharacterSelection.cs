using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

[CreateAssetMenu(fileName = "Data", menuName = "3_ScriptableObjects/CharacterScriptableObject", order = 1)]
public class CharacterSelection : ScriptableObject
{
    public Color color;
    public AnimatorController animatorController
        ;
}
