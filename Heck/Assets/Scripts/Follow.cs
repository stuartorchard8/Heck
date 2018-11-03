using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {
    [SerializeField]
    float speed = 1;
    [SerializeField]
    Transform target;
    Vector3 offset;
    // Use this for initialization
    void Start () {
		if(!target)
        {
            throw new System.Exception("Follow needs its target assigned in editor.");
        }
        offset = target.transform.position - transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 displacement = target.transform.position - transform.position - offset;
        Vector3 position = transform.position;
        Vector3 translation = (displacement * speed + position) / (speed + 1);
        transform.Translate(displacement);
	}
}
