using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGenerator : MonoBehaviour {
    SpriteRenderer nodePrefab;
    SpriteRenderer[] nodes;
    LineRenderer connectionPrefab;
    LineRenderer[] conns;

    private void Awake()
    {
        nodePrefab = transform.Find("NodePrefab").GetComponent<SpriteRenderer>();
        nodePrefab.enabled = false;
        connectionPrefab = transform.Find("ConnectionPrefab").GetComponent<LineRenderer>();
        connectionPrefab.enabled = false;
    }

    public void MakeNetwork(Learner learner, int n_nodes, int n_conns)
    {
        int x_size = learner.layer_sizes.Length*2 + 1;
        int y_size = 0;
        nodes = new SpriteRenderer[n_nodes];
        conns = new LineRenderer[n_conns];
        int node_index = -1;
        for (int x = 0; x < learner.layer_sizes.Length; ++x)
        {
            y_size = y_size > learner.layer_sizes[x]*2 + 1 ? y_size : learner.layer_sizes[x]*2 + 1;
            for (int y = 0; y < learner.layer_sizes[x]; ++y)
            {
                nodes[++node_index] = Instantiate(nodePrefab, transform);
                nodes[node_index].enabled = true;
                nodes[node_index].transform.Translate(new Vector3((x - learner.layer_sizes.Length/2f + 0.5f)*2, y*2 - learner.layer_sizes[x] + 1, 0));
            }
        }


        float scale = x_size > y_size ? x_size : y_size;
        transform.localScale = new Vector3(1f / scale, 1f / scale, 1f);

        if (x_size > y_size)
        {
            foreach (var node in nodes)
            {
                Vector3 position = node.transform.localPosition;
                position.y *= x_size / (float)y_size;
                node.transform.localPosition = position;
            }
        }
        else if (x_size < y_size)
        {
            foreach (var node in nodes)
            {
                Vector3 position = node.transform.localPosition;
                position.x *= y_size / (float)x_size;
                node.transform.localPosition = position;
            }
        }

        int node_offset = 0;
        int conn_index = -1;
        for (int dest_layer_index = 1; dest_layer_index < learner.layer_sizes.Length; ++dest_layer_index)
        {
            int n_source_nodes = learner.layer_sizes[dest_layer_index - 1];
            node_offset += n_source_nodes;
            for (int dest_index = node_offset; dest_index < learner.layer_sizes[dest_layer_index] + node_offset; ++dest_index)
            {
                for (int source_index = node_offset-n_source_nodes; source_index < node_offset; ++source_index)
                {
                    conns[++conn_index] = Instantiate(connectionPrefab, nodes[source_index].transform);
                    conns[conn_index].enabled = true;
                    conns[conn_index].SetPosition(1, nodes[dest_index].transform.localPosition - nodes[source_index].transform.localPosition - new Vector3(0.5f, 0, 0));
                    if(learner.weights[conn_index] > 0)
                    {
                        conns[conn_index].startColor = conns[conn_index].endColor = new Color(0, 1, 0, learner.weights[conn_index]);
                    }
                    else
                    {
                        conns[conn_index].startColor = conns[conn_index].endColor = new Color(1, 0, 0, -learner.weights[conn_index]);
                    }
                }
            }
        }
    }

    public void ShowMagic(Learner learner)
    {
        for(int i = 0; i < nodes.Length; ++i)
        {
            nodes[i].color = new Color(-learner.nodes[i], learner.nodes[i], 0);
        }
    }
}
