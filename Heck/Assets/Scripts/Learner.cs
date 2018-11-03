using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Learner : MonoBehaviour
{
    [SerializeField]
    NetworkGenerator networkGenerator;
    [SerializeField]
    private HingeJoint2D[] joints;
    Transform bodyTransform;
    
    public int[] layer_sizes;

    public float[] nodes;      // Nodes store information to be propegated through the network
    public float[] biases;     // Biases reset the nodes before new data is applied to them
    public float[] weights;    // Weights multiply the values of nodes before they are passed to the next layer

    // Use this for initialization
    void Start ()
    {
        bodyTransform = transform.Find("Body");
        joints = GetComponentsInChildren<HingeJoint2D>();
        // Set up layer sizes
        layer_sizes = new int[4];
        layer_sizes[0] = joints.Length; // Input
        layer_sizes[1] = 8;                 // Hidden Layers
        layer_sizes[2] = 6;                 // Hidden Layers
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
        biases = new float[n_nodes - layer_sizes[0]];
        for (int i = 0; i < biases.Length; ++i)
        {
            biases[i] = (Random.value - 0.5f) * 2.0f;
        }
        weights = new float[n_conns];
        for (int i = 0; i < n_conns; ++i)
        {
            weights[i] = (Random.value - 0.5f)*2.0f;
        }

        if(networkGenerator)
        {
            networkGenerator.MakeNetwork(this, n_nodes, n_conns);
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
            nodes[i] = joints[i].jointAngle / 180f;    // Each joint rotation (0 - 1)
        }
        
        int node_offset = 0;
        int conn_index = 0;
        for (int dest_layer_index = 1; dest_layer_index < layer_sizes.Length; ++dest_layer_index)
        {
            int n_source_nodes = layer_sizes[dest_layer_index - 1];
            node_offset += n_source_nodes;
            for (int dest_index = node_offset; dest_index < layer_sizes[dest_layer_index] + node_offset; ++dest_index)
            {
                nodes[dest_index] = biases[dest_index - layer_sizes[0]];    // Reset node before we add to it
                for (int source_index = node_offset - n_source_nodes; source_index < node_offset; ++source_index)
                {
                    nodes[dest_index] += nodes[source_index] * weights[conn_index++];
                }
                // Apply ReLU, but not on the output layer
                if(dest_layer_index < layer_sizes.Length - 1 && nodes[dest_index] < 0)
                {
                    nodes[dest_index] = 0;
                }
            }
        }

        networkGenerator.ShowMagic(this);

        // Move motors according the the output layer
        for (int i = 0; i < joints.Length; ++i)
        {
            var motor = joints[i].motor;
            motor.motorSpeed = nodes[nodes.Length - i - 1] * 100.0f;
            joints[i].motor = motor;
        }
    }
}
