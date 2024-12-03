using UnityEngine;


[RequireComponent(typeof(EffectSpring))]
public class EffectSpring_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    EffectSpring effectSpring;


    // sender for Live
    AutoSwitchedParameter<float> ropevel = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> ropeacc = new AutoSwitchedParameter<float>();


    // sender for Coda

    void Awake()
    {
        effectSpring = transform.GetComponent<EffectSpring>();
        if (effectSpring == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectSpring.");
        }
    }


    //void Update()
    //{
    //    if (isDispatching == false)
    //        return;
    //}

    #region Receiver
    protected override void RegisterReceiver()
    {
        string base_name = "/spring";
        string rope_index = effectSpring.SpringIndex.ToString();
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
    {
        
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion
}
