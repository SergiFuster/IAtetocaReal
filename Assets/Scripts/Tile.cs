using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer rend;
    public Color highlightedColor;
    public Color creatableColor;
    private Color defaultColor;
    public float influenceValue = 0;

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

    }

    public bool isClear() // does this tile have an obstacle on it. Yes or No?
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 0.2f, obstacles);
        if (col == null)
        {
            return true;
        }
        else {
            return false;
        }
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
            Unit unit = Instantiate(gm.createdUnit, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            unit.hasMoved = true;
            unit.hasAttacked = true;
            gm.ResetTiles();
            gm.createdUnit = null;
        } else if (isCreatable == true && gm.createdVillage != null) {
            Instantiate(gm.createdVillage, new Vector3(transform.position.x, transform.position.y, 0) , Quaternion.identity);
            gm.ResetTiles();
            gm.createdVillage = null;
        }
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

    private void OnDrawGizmos()
    {
        if(influenceValue < 0)
        {
            Gizmos.color = new Color(0, 0, Mathf.Abs(influenceValue));
        }
        else if(influenceValue > 0)
        {
            Gizmos.color = new Color(influenceValue, 0, 0);
        }
        else
        {
            Gizmos.color = Color.black;
        }

        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
