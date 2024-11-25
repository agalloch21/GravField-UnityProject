using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using SplineMesh;
using UnityEngine.Events;
using Unity.Netcode;

public class EffectRopeController : MonoBehaviour
{
    List<Performer> performerList = new List<Performer>();
    List<bool> ropeStateList = new List<bool>();
    List<EffectRope> ropeList = new List<EffectRope>();
    bool effectEnabled = false;

    void Awake()
    {
        Transform performerTransformRoot = GameManager.Instance.PlayerManager.PerformerTransformRoot;
        for (int i=0; i<performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
        }

        for(int i=0; i< transform.childCount; i++)
        {
            ropeStateList.Add(false);
            EffectRope rope = transform.GetChild(i).GetComponent<EffectRope>();
            Vector2Int performer_index = GetPerformerIndexOfRope(i);
            rope.BindPerformer(performerList[performer_index.x], performerList[performer_index.y]);
            ropeList.Add(rope);
        }
    }

    void Start()
    {
        
    }

    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);

        GameManager.Instance.PlayerManager.OnStartPerformingEvent.AddListener(OnStartPerforming);
        GameManager.Instance.PlayerManager.OnStopPerformingEvent.AddListener(OnStopPerforming);
    }

    void OnDisable()
    {
        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);

        GameManager.Instance.PlayerManager.OnStartPerformingEvent.RemoveListener(OnStartPerforming);
        GameManager.Instance.PlayerManager.OnStopPerformingEvent.RemoveListener(OnStopPerforming);
    }

    #region Start / Stop game 
    void OnStartGame(PlayerRole player_role)
    {

    }

    void OnStopGame(PlayerRole player_role)
    {

    }
    #endregion

    

    void OnStartPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }

    void OnStopPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }

    void UpdateAllRopeState()
    {
        for (int i = 0; i < ropeStateList.Count; i++)
        {
            bool cur_rope_state = GetRopeState(i);
            // Just active rope, generate a new one
            if (cur_rope_state == true && ropeStateList[i] == false)
            {

            }
            // Just deactive rope
            if (cur_rope_state == false && ropeStateList[i] == true)
            {

            }
            ropeStateList[i] = cur_rope_state;

            SetRopeState(i, cur_rope_state);
        }
    }

    bool GetRopeState(int rope_index)
    {
        if (effectEnabled == false)
            return false;

        Vector2Int performer_index = GetPerformerIndexOfRope(rope_index);
        return performerList[performer_index.x].isPerforming.Value == true && performerList[performer_index.y].isPerforming.Value == true;
    }

    Vector2Int GetPerformerIndexOfRope(int rope_index)
    {
        // Rope 0: Performer 0 , 1
        // Rope 1: Performer 0 , 2
        // Rope 2: Performer 1 , 2
        int start_index = rope_index == 2 ? 1 : 0;
        int end_index = rope_index == 0 ? 1 : 2;
        return new Vector2Int(start_index, end_index);
    }

    void SetRopeState(int index, bool state)
    {
        ropeList[index].SetRopeState(state);

        //transform.GetChild(index).gameObject.SetActive(state);
        
        //SetSplineMeshVisible(index, state);
    }

    void SetSplineMeshVisible(int index, bool visible)
    {
        Transform root_transform = transform.GetChild(index).Find("generated by SplineMeshTiling");
        root_transform?.gameObject.SetActive(visible);
    }

    public void SetEffectState(bool state)
    {
        effectEnabled = state;
        UpdateAllRopeState();
    }    
}
