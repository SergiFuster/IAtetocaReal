using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public bool isSelected;
    public bool hasMoved;

    public int tileSpeed;
    public float moveSpeed;

    private Vector3[] path;

    private GM gm;
    private InfluenceMap map;

    private GridNode grid;

    public int attackRadius;
    public bool hasAttacked;
    public List<Unit> enemiesInRange = new List<Unit>();

    public int playerNumber;

    public GameObject weaponIcon;

    // Attack Stats
    public int health;
    public int attackDamage;
    public int defenseDamage;
    public int armor;
    public DamageIcon damageIcon;

    public int cost;
    Tile[] tiles;
    public GameObject deathEffect;

	private Animator camAnim;

    public bool isKing;

	private AudioSource source;

    public Text displayedText; 

    private void Start()
    {
        map = FindObjectOfType<InfluenceMap>();
		source = GetComponent<AudioSource>();
		camAnim = Camera.main.GetComponent<Animator>();
        gm = FindObjectOfType<GM>();
        
        grid = FindObjectOfType<GridNode>();
        UpdateHealthDisplay();
        tiles = FindObjectsOfType<Tile>();
        if(playerNumber == 1)
        {
            gm.playerVisibilityMap.InsertViewPoint(this.GetComponent<Unit>());
        }
        else
        {
            gm.IAVisibilityMap.InsertViewPoint(this.GetComponent<Unit>());
        }
    }

    private void UpdateHealthDisplay ()
    {
        if (isKing)
        {
            displayedText.text = health.ToString();
        }
    }

    private void OnMouseDown() // select character or deselect if already selected
    {
        
        ResetWeaponIcon();

        if (isSelected)
        {
            
            isSelected = false;
            gm.selectedUnit = null;
            gm.ResetTiles();

        }
        else {
            if (playerNumber == gm.playerTurn) { // select unit only if it's his turn
                if (gm.selectedUnit != null)
                { // deselect the unit that is currently selected, so there's only one isSelected unit at a time
                    gm.selectedUnit.isSelected = false;
                }
                gm.ResetTiles();

                gm.selectedUnit = this;

                isSelected = true;
				if(source != null){
					source.Play();
				}
				
                GetWalkableTiles();
                GetEnemies();
            }

        }



        Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);
        if (col != null)
        {
            Unit unit = col.GetComponent<Unit>(); // double check that what we clicked on is a unit
            if (unit != null && gm.selectedUnit != null)
            {
                if (gm.selectedUnit.enemiesInRange.Contains(unit) && !gm.selectedUnit.hasAttacked)
                { // does the currently selected unit have in his list the enemy we just clicked on
                    gm.selectedUnit.Attack(unit);

                }
            }
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            gm.UpdateInfoPanel(this);
        }
    }



    void GetWalkableTiles() { // Looks for the tiles the unit can walk on
        if (hasMoved == true) {
            return;
        }

        grid.CreateGrid();
        foreach (Tile tile in tiles) {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed && tile.walkable)
            { // how far he can move
                if (tile.isClear() == true)
                { // is the tile clear from any obstacles
                    tile.Highlight();
                }

            }          
        }
    }

    void GetEnemies() {
    
        enemiesInRange.Clear();

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= attackRadius) // check is the enemy is near enough to attack
            {
                if (enemy.playerNumber != gm.playerTurn && !hasAttacked) { // make sure you don't attack your allies
                    enemiesInRange.Add(enemy);
                    enemy.weaponIcon.SetActive(true);
                }

            }
        }
    }

    public void Move(Vector3 movePos)
    {
        gm.ResetTiles();
        PathRequestManager.RequestPath(transform.position, movePos, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesfull)
    {
        if (pathSuccesfull)
        {
            path = newPath;
            StopCoroutine("StartMovement");
            StartCoroutine("StartMovement");
        }
    }

    void Attack(Unit enemy) {
        hasAttacked = true;

        int enemyDamege = attackDamage - enemy.armor;
        int unitDamage = enemy.defenseDamage - armor;

        if (enemyDamege >= 1)
        {
            enemy.health -= enemyDamege;
            enemy.UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            d.Setup(enemyDamege);
        }

        if (transform.tag == "Archer" && enemy.tag != "Archer")
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= 1) // check is the enemy is near enough to attack
            {
                if (unitDamage >= 1)
                {
                    health -= unitDamage;
                    UpdateHealthDisplay();
                    DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                    d.Setup(unitDamage);
                }
            }
        } else {
            if (unitDamage >= 1)
            {
                health -= unitDamage;
                UpdateHealthDisplay();
                DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                d.Setup(unitDamage);
            }
        }

        if (enemy.health <= 0)
        {
         
            if (deathEffect != null){
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

            if (enemy.isKing)
            {
                gm.ShowVictoryPanel(enemy.playerNumber);
            }

            GetWalkableTiles(); // check for new walkable tiles (if enemy has died we can now walk on his tile)
            gm.RemoveInfoPanel(enemy);
            enemy.DestroyMe();
        }

        if (health <= 0)
        {

            if (deathEffect != null)
			{
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

			if (isKing)
            {
                gm.ShowVictoryPanel(playerNumber);
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            DestroyMe();
        }

        gm.UpdateInfoStats();
  

    }

    public void DestroyMe()
    {
        if(playerNumber == 1)
            gm.playerVisibilityMap.DeleteViewPoint(this.GetComponent<Unit>());
        else
            gm.IAVisibilityMap.DeleteViewPoint(this.GetComponent<Unit>());
        Destroy(gameObject);
    }

    public void ResetWeaponIcon() {
        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            enemy.weaponIcon.SetActive(false);
        }
    }

    IEnumerator StartMovement() { // Moves the character to his new position.

        int index = 0;
        while (index < path.Length)
        {
            while(transform.position != path[index])
            {
                transform.position = Vector2.MoveTowards(transform.position, path[index], moveSpeed * Time.deltaTime);
                yield return null;
            }
            index += 1;
        }

        hasMoved = true;
        ResetWeaponIcon();
        GetEnemies();
        gm.MoveInfoPanel(this);
    }






}
