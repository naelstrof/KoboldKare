using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameEventResponseConditionalAnimatorInt : GameEventResponse
{
    [SerializeField] public Animator animator;
    [SerializeField] public string parameterName;
    [SerializeField] public int targetInteger;
    [SerializeField, Range(0, 5), Tooltip("0 is =, 1 is ≠, 2 is <, 3 is >, 4 is ≤, 5 is ≥")] public int Operator;
    [SerializeField, SubclassSelector, SerializeReference] private List<GameEventResponse> requirementMet = new List<GameEventResponse>();
    [SerializeField, SubclassSelector, SerializeReference] private List<GameEventResponse> requirementFailed = new List<GameEventResponse>();


    public override void Invoke(MonoBehaviour owner)
    {
        int aniParamator = animator.GetInteger(parameterName);

        switch (Operator)
        {
            case 0:
                if (aniParamator == targetInteger)
                {
                    foreach (var response in requirementMet)
                    {
                        response?.Invoke(owner);
                    }
                }
                else
                {
                    foreach (var response in requirementFailed)
                    {
                        response?.Invoke(owner);
                    }
                }
                break;
            case 1:
                if (aniParamator != targetInteger)
                {
                    foreach (var response in requirementMet)
                    {
                        response?.Invoke(owner);
                    }
                }
                else
                {
                    foreach (var response in requirementFailed)
                    {
                        response?.Invoke(owner);
                    }
                }
                break;
            case 2:
                if (aniParamator < targetInteger)
                {
                    foreach (var response in requirementMet)
                    {
                        response?.Invoke(owner);
                    }
                }
                else
                {
                    foreach (var response in requirementFailed)
                    {
                        response?.Invoke(owner);
                    }
                }
                break;
            case 3:
                if (aniParamator > targetInteger)
                {
                    foreach (var response in requirementMet)
                    {
                        response?.Invoke(owner);
                    }
                }
                else
                {
                    foreach (var response in requirementFailed)
                    {
                        response?.Invoke(owner);
                    }
                }
                break;
            case 4:
                if (aniParamator <= targetInteger)
                {
                    foreach (var response in requirementMet)
                    {
                        response?.Invoke(owner);
                    }
                }
                else
                {
                    foreach (var response in requirementFailed)
                    {
                        response?.Invoke(owner);
                    }
                }
                break;
            case 5:
                if (aniParamator >= targetInteger)
                {
                    foreach (var response in requirementMet)
                    {
                        response?.Invoke(owner);
                    }
                }
                else
                {
                    foreach (var response in requirementFailed)
                    {
                        response?.Invoke(owner);
                    }
                }
                break;
        }
    }
}