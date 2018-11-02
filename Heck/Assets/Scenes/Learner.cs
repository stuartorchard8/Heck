using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Learner : MonoBehaviour
{
    [SerializeField]
    private HingeJoint2D[] joints;
    Transform bodyTransform;

    const float decay = 0.001f; // 0.1% decay per second
    const float base_weight = 0f; // The value the weights will decay to.
    int[] layer_sizes;
    [SerializeField]
    float[] nodes;
    float[] conns;

	// Use this for initialization
	void Start ()
    {
        bodyTransform = transform.Find("Body");
        joints = GetComponentsInChildren<HingeJoint2D>();
        // Set up layer sizes
        layer_sizes = new int[4];
        layer_sizes[0] = joints.Length + 1; // Input
        layer_sizes[1] = 8;                 // Hidden layer
        layer_sizes[2] = 6;                 // Hidden layer
        layer_sizes[3] = joints.Length;     // Output

        foreach(var joint in joints)
        {
           joint.useMotor = true;
        }

        int n_nodes = layer_sizes[0];
        int n_conns = 0;
        for(int i = 1; i < layer_sizes.Length; ++i)
        {
            n_nodes += layer_sizes[i];
            n_conns += layer_sizes[i - 1] * layer_sizes[i];
        }
        nodes = new float[n_nodes];
        conns = new float[n_conns];
        
        //Random.InitState(0);
        for(int i = 0; i < n_conns; ++i)
        {
            conns[i] = (Random.value - 0.5f)*2.0f;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        //////////////
        // Thinking //
        //////////////
        // Set up input nodes
        for(int i = 0; i < joints.Length; ++i)
        {
            nodes[i] = joints[i].transform.localRotation.eulerAngles.z / 360.0f;    // Each joint rotation
        }
        nodes[joints.Length] = bodyTransform.localRotation.eulerAngles.z / 360.0f;  // Body rotation

        /*
        for (int i = 0; i < conns.Length; ++i)
        {
            float frame_decay = decay * Time.deltaTime;
            conns[i] = conns[i] * (1f - frame_decay) + base_weight * frame_decay;
        }
        */
        
        int node_offset = 0;
        int conn_index = 0;
        for (int dest_layer_index = 1; dest_layer_index < layer_sizes.Length; ++dest_layer_index)
        {
            int n_source_nodes = layer_sizes[dest_layer_index - 1];
            node_offset += n_source_nodes;
            for (int dest_index = 0; dest_index < layer_sizes[dest_layer_index]; ++dest_index)
            {
                nodes[dest_index + node_offset] = 0;    // Reset node before we add to it
                for (int source_index = -n_source_nodes; source_index < 0; ++source_index)
                {
                    nodes[dest_index + node_offset] += nodes[source_index + node_offset] * conns[conn_index++];
                }
            }
        }

        // Move motors according the the output layer
        for (int i = 0; i < joints.Length; ++i)
        {
            var motor = joints[i].motor;
            motor.motorSpeed = nodes[nodes.Length - i - 1] * 100.0f;
            joints[i].motor = motor;
        }
    }
}
