using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class componentValidMoveTile : MonoBehaviour
{
    protected int _x, _y;

	void Start () {
	}
	
	void Update () {
	}

    public virtual void setLocation(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public Vector2 getLocation()
    {
        return new Vector2(_x, _y);
    }
}
