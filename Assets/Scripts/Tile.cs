using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer rend;
    public Color highlightedColor;
    public Color creatableColor;
    private Color defaultColor;
    public bool isLake = false;
    public int playerNumber = 0;
    public float influenceValue = 0;
    public GameObject Fog;
    VisibilityNode playerFogNode;
    VisibilityNode IAFogNode;

    public LayerMask obstacles;

    public bool walkable;
    private bool isWalkable;
    public bool isCreatable;

    private GM gm;

    public float amount;
    private bool sizeIncrease;

	private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        gm = FindObjectOfType<GM>();
        rend = GetComponent<SpriteRenderer>();
        defaultColor = rend.color;
        playerFogNode = gm.playerVisibilityMap.GetNodeFromWorldPosition(this.transform.position);
        IAFogNode = gm.IAVisibilityMap.GetNodeFromWorldPosition(this.transform.position);

    }

    public bool isClear() // does this tile have an obstacle on it. Yes or No?
    {
        return Physics2D.OverlapCircle(transform.position, 0.2f, obstacles) == null;
    }

    private void FixedUpdate()
    {
        if (GM.instance.playerTurn == 1)
            Fog.SetActive(!playerFogNode.visible);
        else
            Fog.SetActive(!IAFogNode.visible);
    }

    public void Highlight() {
		
        rend.color = highlightedColor;
        isWalkable = true;
    }

    public void Reset()
    {
        rend.color = defaultColor;
        isWalkable = false;
        isCreatable = false;
    }

    public void SetCreatable() {
        rend.color = creatableColor;
        isCreatable = true;
    }

    private void OnMouseDown()
    {
        if (isWalkable == true) {
            gm.selectedUnit.Move(this.transform.position);
        } else if (isCreatable == true && gm.createdUnit != null) {
            createUnit();
        } else if (isCreatable == true && gm.createdVillage != null) {
            createVillage();
        }
    }

    public void createUnit()
    {
        Unit unit = Instantiate(gm.createdUnit, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        unit.hasMoved = true;
        unit.hasAttacked = true;
        gm.ResetTiles();
        gm.createdUnit = null;
    }

    public void createVillage()
    {
        Instantiate(gm.createdVillage, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        gm.ResetTiles();
        gm.createdVillage = null;
    }


    private void OnMouseEnter()
    {
        if (isClear() == true) {
			source.Play();
			sizeIncrease = true;
            transform.localScale += new Vector3(amount, amount, amount);
        }
        
    }

    private void OnMouseExit()
    {
        if (isClear() == true)
        {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }

        if (isClear() == false && sizeIncrease == true) {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(influenceValue < 0)
        {
            Gizmos.color = new Color(0, 0, Mathf.Abs(influenceValue), 0.5f);
        }
        else if(influenceValue > 0)
        {
            Gizmos.color = new Color(influenceValue, 0, 0, 0.5f);
        }
        else
        {
            Gizmos.color = new Color(0, 0, 0, 0);
        }

        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
