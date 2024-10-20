using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectManager : MonoBehaviour
{
    public Volume volume;
    Bloom bloom;

    public EffectRopeController effectRope;

    public EffectSpringController effectSpring;

    public EffectMagneticField effectMagneticField;

    float effectMode = 0;

    void OnEnable()
    {
        GameManager.Instance.RoleManager.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);

        GameManager.Instance.PerformerGroup.OnPerformerFinishSpawn.AddListener(OnPerformerFinishSpawn);
    }

    void OnDisable()
    {
        //GameManager.Instance.RoleManager.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void Start()
    {
        GameManager.Instance.HolokitCameraManager.OnScreenRenderModeChanged += OnScreenRenderModeChanged;

        SetBloomState(GameManager.Instance.HolokitCameraManager.ScreenRenderMode);
    }

    void OnScreenRenderModeChanged(HoloKit.ScreenRenderMode mode)
    {
        SetBloomState(mode);
    }

    void SetBloomState(HoloKit.ScreenRenderMode mode)
    {
        VolumeProfile profile = volume.sharedProfile;
        profile.TryGet<Bloom>(out bloom);
#if UNITY_EDITOR
        //bloom.active = true;
        //bloom.intensity.value = 1.93f;
#else
        if (mode == HoloKit.ScreenRenderMode.Mono)
        {
            //bloom.int = false;
            bloom.intensity.value = 0.93f;
        }
        else
        {
            //bloom.active = true;
            bloom.intensity.value = 1.93f;
        }
#endif
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        ChangeEffectModeTo(GameManager.Instance.PerformerGroup.effectMode.Value);
    }

    void OnPerformerFinishSpawn()
    {
        AssignLocalVariable();

        RegisterNetworkVariableCallback_Client();
    }

    #region NetworkVariable / Clients also should execute
    void AssignLocalVariable()
    {
        effectMode = GameManager.Instance.PerformerGroup.effectMode.Value;
    }

    void RegisterNetworkVariableCallback_Client()
    {
        GameManager.Instance.PerformerGroup.effectMode.OnValueChanged += (float prev, float cur) => { ChangeEffectModeTo(cur); };
    }

    void ChangeEffectModeTo(float effect_index)
    {
        Debug.Log("ChangeEffectModeTo: " + effect_index);
        effectMode = Mathf.RoundToInt(effect_index);

        effectRope.SetEffectState(effectMode == 0);

        effectSpring.SetEffectState(effectMode == 1);

        effectMagneticField.SetEffectState(effectMode == 2);
    }
    #endregion

    

    #region Instance
    private static EffectManager _Instance;

    public static EffectManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<EffectManager>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<EffectManager>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
