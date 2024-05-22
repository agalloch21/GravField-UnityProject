using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
using SplineMesh;
using Unity.Netcode;

public class EffectRope : MonoBehaviour
{
    // Basic
    public Performer performerStart;
    public Performer performerEnd;
    Transform ropeStart;
    Transform ropeEnd; 
    public Vector3 ropeOffset;
    int ropeIndex;

    // Path
    Spline spline;
    List<GameObject> wayPoints = new List<GameObject>();

    // Output to LIVE
    Transform centroidTransform;
    private Vector3 centroidPos;
    public Vector3 CentroidPos { get => centroidPos; }
    private Vector3 centroidVel;
    public Vector3 CentroidVel { get => centroidVel; }
    AutoSwitchedParameter<float> ropevel = new AutoSwitchedParameter<float>();

    // Parameters    
    float ropeMeshScale;
    float startThickness;
    float endThickness;
    float startMass;
    float endMass;

    void Awake()
    {
        ropeIndex = transform.GetSiblingIndex();

        ropeStart = transform.Find("Anchors").GetChild(0);
        ropeEnd = transform.Find("Anchors").GetChild(1);
    }

    void Start()
    {
        spline = GetComponent<Spline>();

        AssignWayPoints();

        AssignSplineNodes();

        RegisterNetworkVariableCallback_Client();

        RegisterPropertiesToLive_Server();
    }

    string FormatedOscAddress(string param)
    {
        return "/rope" + ropeIndex.ToString() + param;
    }

    void Update()
    {
        UpdateRopeAnchors();

        UpdateNodes();

        UpdateParamtersForLive();
    }

    void UpdateRopeAnchors()
    {
        //Vector3 nor = (performerEnd.position - performerStart.position).normalized;
        //ropeStart.localPosition = performerStart.position + nor* 0.3f;
        //ropeEnd.localPosition = performerEnd.position -nor*0.3f;

        ropeStart.localPosition = performerStart.transform.TransformPoint(ropeOffset);
        ropeEnd.localPosition = performerEnd.transform.TransformPoint(ropeOffset);

        ropeStart.localRotation = performerStart.transform.localRotation;
        ropeEnd.localRotation = performerEnd.transform.localRotation;
    }

    void UpdateNodes()
    {
        int i = 0;
        foreach (GameObject wayPoint in wayPoints)
        {
            var node = spline.nodes[i++];
            //if (Vector3.Distance(node.Position, transform.InverseTransformPoint(wayPoint.transform.position)) > 0.001f)
            //{
            //    node.Position = transform.InverseTransformPoint(wayPoint.transform.position);
            //    //node.Up = wayPoint.transform.up;
            //}
            node.Position = wayPoint.transform.position;
            //node.Up = wayPoint.transform.up;
        }
    }

    void UpdateParamtersForLive()
    {
        Vector3 last_pos = centroidPos;
        centroidPos = centroidTransform.localPosition;
        centroidVel = (centroidPos - last_pos) / Time.deltaTime;

        ropevel.OrginalValue = centroidVel.magnitude;
    }



    #region NetworkVariable
    void RegisterNetworkVariableCallback_Client()
    {
        performerStart.remoteThickness.OnValueChanged += (float prev, float cur) => { startThickness = cur; UpdateRopeThickness(); };
        performerEnd.remoteThickness.OnValueChanged += (float prev, float cur) => { endThickness = cur; UpdateRopeThickness(); };

        performerStart.remoteMass.OnValueChanged += (float prev, float cur) => { startMass = cur; UpdateRopeMass(); };
        performerEnd.remoteMass.OnValueChanged += (float prev, float cur) => { endMass = cur; UpdateRopeMass(); };

        GameManager.Instance.PerformerGroup.ropeMeshScale.OnValueChanged += (float prev, float cur) => { ropeMeshScale = cur; UpdateRopeMeshScale(); };

    }
    void UpdateRopeThickness()
    {
        float currentLength = 0;
        foreach (CubicBezierCurve curve in spline.GetCurves())
        {
            float startRate = currentLength / spline.Length;
            currentLength += curve.Length;
            float endRate = currentLength / spline.Length;

            curve.n1.Scale = Vector3.one * (startThickness + (endThickness - startThickness) * startRate);
            curve.n2.Scale = Vector3.one * (startThickness + (endThickness - startThickness) * endRate);
        }
    }

    void UpdateRopeMass()
    {
        Transform segment_root = transform.Find("Segments");
        for (int m = 0; m < segment_root.childCount; m++)
        {
            Rigidbody rigid = segment_root.GetChild(m).GetComponent<Rigidbody>();
            rigid.mass = Mathf.Lerp(startMass, endMass, m / segment_root.childCount - 1);
        }
    }

    void UpdateRopeMeshScale()
    {
        SplineMeshTiling meshTiling = GetComponent<SplineMeshTiling>();
        meshTiling.scale = Mathf.Max(0.05f, ropeMeshScale) * Vector3.one;
    }
    #endregion

    #region Parameter sent to Live
    void RegisterPropertiesToLive_Server()
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        SenderForLive.Instance.RegisterOscPropertyToSend(FormatedOscAddress("vel"), ropevel);
    }
    #endregion

    #region Paramters received from Coda

    #endregion




    public void BindPerformer(Performer performer_start, Performer performer_end)
    {
        performerStart = performer_start;
        performerEnd = performer_end;
    }

    //public void SetPerformerOffset(Vector3 offset)
    //{
    //    ropeOffset = offset;
    //}

    //public void BindRopeAnchors(Transform performer_start, Transform performer_end)
    //{
    //    ropeStart = performer_start;
    //}

    //public void SetThickness(float start_thickness, float end_thickness)
    //{
    //    startThickness = start_thickness;
    //    endThickness = end_thickness;
    //}






    void AssignWayPoints()
    {
        wayPoints.Clear();
        //Transform anchor_root = transform.Find("Anchors");
        //wayPoints.Add(anchor_root.GetChild(0).gameObject);

        Transform joint_root = transform.Find("Joints");
        for (int i = 0; i < joint_root.childCount; i++)
        {
            wayPoints.Add(joint_root.GetChild(i).gameObject);
        }

        //wayPoints.Add(anchor_root.GetChild(1).gameObject);

        centroidTransform = joint_root.GetChild(Mathf.FloorToInt(joint_root.childCount / 2f));
    }

    void AssignSplineNodes()
    {
        foreach (var penisNode in wayPoints.ToList())
        {
            if (penisNode == null) wayPoints.Remove(penisNode);
        }
        int nodeCount = wayPoints.Count;
        // adjust the number of nodes in the spline.
        while (spline.nodes.Count < nodeCount)
        {
            spline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
        }
        while (spline.nodes.Count > nodeCount && spline.nodes.Count > 2)
        {
            spline.RemoveNode(spline.nodes.Last());
        }
    }

    float CalculateRopeLength()
    {
        Vector3 last_pos = Vector3.zero;
        float dis_total = 0;

        foreach (GameObject wayPoint in wayPoints)
        {
            if (last_pos == Vector3.zero)
            {
                last_pos = wayPoint.transform.position;
            }
            else
            {
                dis_total += Vector3.Distance(wayPoint.transform.position, last_pos);
                last_pos = wayPoint.transform.position;
            }
        }
        //Debug.Log($"Rope{transform.GetSiblingIndex()} Length:{dis_total}");
        return dis_total;
    }
}