using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EffectSpringGroup : MonoBehaviour
{
    Performer performerStart;
    Performer performerEnd;

    int springIndex;
    public int SpringIndex { get => springIndex; }

    bool springEnabled = false;

    [HideInInspector]
    public List<EffectSpring> springList = new List<EffectSpring>();


    void Awake()
    {
        springIndex = transform.GetSiblingIndex();
    }

    public void InitializeSpringGroup(Performer start, Performer end, Vector3 offset)
    {
        performerStart = start;
        performerEnd = end;

        for (int i = 0; i < transform.childCount; i++)
        {
            EffectSpring spring = transform.GetChild(i).GetComponent<EffectSpring>();
            spring.BindPerformer(start, end);
            springList.Add(spring);
        }
    }

    public void SetSpringState(bool state)
    {
        springEnabled = state;

        for (int i = 0; i < springList.Count; i++)
        {
            springList[i].SetSpringState(state);
        }
    }
}
