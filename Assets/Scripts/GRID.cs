using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GRID : MonoBehaviour
{
    public int nodesAmountX, nodesAmountY;
    public Vector2 gridSize;

    [Tooltip("Изменяет размер сетки с заданного пользователем на размер выбранного участка.")]
    public bool setGridSizeToMeshSize = true;
    public MeshRenderer mesh;
    public LayerMask unwalkableMask;
    [Tooltip("Отключает отрисовку свободных и занятых полей, оставляя только отрисовку конечного пути.")]
    public bool onlyDisplayPathGizmos;
    private Vector2 gridWorldSize;
    private float nodeSizeX, nodeSizeY;
    Node[,] grid;

    private void Start()
    {
        if (!setGridSizeToMeshSize) gridWorldSize = gridSize;
        else gridWorldSize = new Vector2(mesh.bounds.size.x, mesh.bounds.size.z);

        nodeSizeX = gridWorldSize.x / nodesAmountX;
        nodeSizeY = gridWorldSize.y / nodesAmountY;
        CreateGrid();
    }

    public int MaxSize
    {
        get { return nodesAmountX * nodesAmountY; }
    }

    private void CreateGrid()
    {
        grid = new Node[nodesAmountX, nodesAmountY];

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < nodesAmountX; x++)
        {
            for (int y = 0; y < nodesAmountY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSizeX + nodeSizeX / 2) + Vector3.forward * (y * nodeSizeY + nodeSizeY / 2);
                float checkSphereRadius = (nodeSizeY > nodeSizeX) ? nodeSizeX : nodeSizeY;
                bool walkable = !(Physics.CheckBox(worldPoint, new Vector3(nodeSizeX / 2 - .1f, .5f, nodeSizeY / 2 - .1f), Quaternion.identity, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }

    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < nodesAmountX && checkY >= 0 && checkY < nodesAmountY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((nodesAmountX - 1) * percentX);
        int y = Mathf.RoundToInt((nodesAmountY - 1) * percentY);
        return grid[x, y];
    }


    public List<Node> path;
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (onlyDisplayPathGizmos)
        {
            if (path != null)
            {
                foreach (Node n in path)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.worldPosition, new Vector3(nodeSizeX - .1f, 1, nodeSizeY - .1f));
                }
            }
        }
        else
        {
            if (grid != null)
            {
                for (int x = 0; x < nodesAmountX; x++)
                {
                    for (int y = 0; y < nodesAmountY; y++)
                    {
                        Gizmos.color = (grid[x, y].walkable) ? Color.white : Color.red;
                        if (path != null)
                            if (path.Contains(grid[x, y]))
                                Gizmos.color = Color.black;
                        Gizmos.DrawCube(grid[x, y].worldPosition, new Vector3(nodeSizeX - .1f, 1, nodeSizeY - .1f));
                    }
                }
            }
        }
    }
}
