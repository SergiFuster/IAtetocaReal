using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceMap : MonoBehaviour
{
    public LayerMask UnitMask;
    public int propagationRadius;
    public List<Unit> generators;
    Tile[] tiles;

    private void Start()
    {
        tiles = FindObjectsOfType<Tile>();
        generators = new List<Unit>();

    }

    public void InsertToGeneratorsList(Unit unit)
    {
        generators.Add(unit);
    }

    public void DeleteFromGeneratorsList(Unit unit)
    {
        generators.Remove(unit);
    }

    private void LateUpdate()
    {
        UpdateInfluenceMap();
    }
    public void UpdateInfluenceMap()
    {
        foreach(Tile tile in tiles)
        {
            Collider2D[] nearestUnits = Physics2D.OverlapBoxAll(tile.transform.position, Vector2.one * propagationRadius, 0, UnitMask);
            tile.influenceValue = 0;
            if (nearestUnits != null)
            {
                foreach(Collider2D unit in nearestUnits)
                {
                    float distance = (unit.transform.position - tile.transform.position).magnitude;
                    if(unit.GetComponent<Unit>().playerNumber == 1)
                    {
                        tile.influenceValue += 1 / distance;
                    }
                    else
                    {
                        tile.influenceValue -= 1 / distance;
                    }
                }
            }
        }
    }
}
