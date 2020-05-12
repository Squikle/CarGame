using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelsClass
{
    public Transform wheel;
    public Transform wheelModel;
    public Transform raySource;

    [NonSerialized] public GameObject surface;

    [NonSerialized] public List<GameObject> emitters = new List<GameObject>();
}
