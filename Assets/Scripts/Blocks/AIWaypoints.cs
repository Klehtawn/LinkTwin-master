using UnityEngine;
using System.Collections;
using System;

public class AIWaypoints : Block {

    [NonSerialized]
    public Vector3[] points = null;

    [NonSerialized]
    public float[] delays = null;

	// Use this for initialization
	public override void Start () {

        base.Start();

        UpdatePointsFromChildren();

#if !UNITY_EDITOR
        for(int i = 0; i < transform.childCount; i++)
        {
            Transform c = transform.GetChild(i);
            c.gameObject.SetActive(false);
        }
#endif
	}
	
	// Update is called once per frame
	public override void Update () {

        base.Update();
	
	}

    public void UpdatePoints()
    {
        UpdatePointsFromChildren();
    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        UpdatePointsFromChildren();

        WriteSignificantValue(ref bw, "pointscount", (byte)points.Length);

        for(int i = 0; i < points.Length; i++)
        {
            WriteSignificantValue(ref bw, "p" + i.ToString() + "x", points[i].x);
            WriteSignificantValue(ref bw, "p" + i.ToString() + "y", points[i].y);
            WriteSignificantValue(ref bw, "p" + i.ToString() + "z", points[i].z);

            WriteSignificantValue(ref bw, "delay" + i.ToString(), delays[i]);
        }
    }

    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        int pc = ReadSignificantValueByte(ref br, "pointscount");
        points = new Vector3[pc];
        delays = new float[pc];

        for(int i = 0; i < pc; i++)
        {
            points[i].x = ReadSignificantValueFloat(ref br, "p" + i.ToString() + "x");
            points[i].y = ReadSignificantValueFloat(ref br, "p" + i.ToString() + "y");
            points[i].z = ReadSignificantValueFloat(ref br, "p" + i.ToString() + "z");

            delays[i] = ReadSignificantValueFloat(ref br, "delay" + i.ToString());
        }

        UpdateChildrenFromPoints();
    }

    public override void LinkSignificantValues(Block[] blocks)
    {
    }

    void UpdatePointsFromChildren()
    {
        int c = transform.childCount;
        points = new Vector3[c];
        delays = new float[c];
        for(int i = 0; i < c; i++)
        {
            points[i] = transform.GetChild(i).position;
            delays[i] = transform.GetChild(i).GetComponent<AIWaypointWait>().wait;
        }
    }

    void UpdateChildrenFromPoints()
    {
        Widget.DeleteAllChildren(transform);

        if(points == null || points.Length == 0) return;

        GameObject src = Resources.Load<GameObject>("WaypointSphere");

        for(int i = 0; i < points.Length; i++)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(src);
            obj.transform.SetParent(transform);
            obj.name = "Waypoint" + i.ToString();
            obj.transform.localScale = Vector3.one;
            obj.transform.position = points[i];

            obj.GetComponent<AIWaypointWait>().wait = delays[i];
        }
    }
}
