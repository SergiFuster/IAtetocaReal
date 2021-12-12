using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAGameMaster : MonoBehaviour
{
    public GameObject[] priorityShopUnitsEarlyGame;
    public GameObject[] priorityShopUnitsMidGame;
    public GameObject[] priorityShopUnitsLateGame;
    CharacterCreation shop;
    StateMachine stateMachine;
    List<Unit> IAUnits;
    Unit myKing;
    Unit targetUnit;
    public Unit defaultTarget;
    // Start is called before the first frame update
    void Start()
    {
        shop = FindObjectOfType<CharacterCreation>();
        stateMachine = new StateMachine();
        var waiting = stateMachine.CreateState("waiting");
        var movingUnits = stateMachine.CreateState("movingUnits");
        var shopping = stateMachine.CreateState("shopping");

        movingUnits.onEnter = delegate
        {
            StartCoroutine(MoveUnits());
        };
        var attacking = stateMachine.CreateState("attacking");

        attacking.onEnter = delegate
        {
            StartCoroutine(Attack());
        };

        shopping.onEnter = delegate
        {
            StartCoroutine(Shopping());
        };
    }

    IEnumerator Shopping()
    {
        if(GM.instance.gameState == GM.GameState.Early)
        {
            foreach(GameObject unit in priorityShopUnitsEarlyGame)
            {
                bool isVillage = unit.GetComponent<Village>() != null;
                if (isVillage)
                {
                    Village village = unit.GetComponent<Village>();
                    bool isBuyeable = GM.instance.player2Gold - village.cost >= 0;
                    while (isBuyeable)
                    {
                        shop.BuyVillage(village);
                        if (GM.instance.createdUnit != null || GM.instance.createdVillage)
                            putUnitOnTable();
                        isBuyeable = GM.instance.player2Gold - village.cost >= 0;
                        yield return new WaitForSeconds(2);
                    }
                }
                else
                {
                    Unit character = unit.GetComponent<Unit>();
                    shop.BuyUnit(character);
                }
                if(GM.instance.createdUnit != null || GM.instance.createdVillage)
                    putUnitOnTable();
                yield return new WaitForSeconds(1);
            }
            stateMachine.TransitionTo("movingUnits");
        }
        else if(GM.instance.gameState == GM.GameState.Mid)
        {
            foreach (GameObject unit in priorityShopUnitsMidGame)
            {
                bool isVillage = unit.GetComponent<Village>() != null;
                if (isVillage)
                {
                    Village village = unit.GetComponent<Village>();
                    shop.BuyVillage(village);
                }
                else
                {
                    Unit character = unit.GetComponent<Unit>();
                    bool isBuyeable = GM.instance.player2Gold - character.cost >= 0;
                    while (isBuyeable)
                    {
                        shop.BuyUnit(character);
                        if (GM.instance.createdUnit != null || GM.instance.createdVillage)
                            putUnitOnTable();
                        isBuyeable = GM.instance.player2Gold - character.cost >= 0;
                        yield return new WaitForSeconds(2);
                    }
                }
            }
            stateMachine.TransitionTo("movingUnits");
        }
        else
        {
            foreach (GameObject unit in priorityShopUnitsLateGame)
            {
                bool isVillage = unit.GetComponent<Village>() != null;
                if (isVillage)
                {
                    Village village = unit.GetComponent<Village>();
                    shop.BuyVillage(village);
                }
                else
                {
                    Unit character = unit.GetComponent<Unit>();
                    bool isBuyeable = GM.instance.player2Gold - character.cost >= 0;
                    while(isBuyeable)
                    {
                        shop.BuyUnit(character);
                        if (GM.instance.createdUnit != null || GM.instance.createdVillage)
                            putUnitOnTable();
                        isBuyeable = GM.instance.player2Gold - character.cost >= 0;
                        yield return new WaitForSeconds(2);
                    }
                }
                
            }
            stateMachine.TransitionTo("movingUnits");
        }

    }

    void putUnitOnTable()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        float maximum = -Mathf.Infinity;
        Tile choosedTile = null;
        foreach(Tile tile in tiles)
        {
            if(tile.isCreatable && tile.influenceValue > maximum)
            {
                choosedTile = tile;
                maximum = choosedTile.influenceValue;
            }
        }
        if(choosedTile != null)
        {
            if(GM.instance.createdUnit != null)
                choosedTile.createUnit();
            else if (GM.instance.createdVillage != null)
                choosedTile.createVillage();
        }
    }
    IEnumerator Attack()
    {
        if(IAUnits.Count != 0)
        {
            if(GM.instance.gameState != GM.GameState.End)
            {
                foreach(Unit unit in IAUnits)
                {
                    if (unit.hasAttacked || unit.isKing) continue;
                    Unit choosedEnemie = null;
                    unit.GetEnemies();
                    if(unit.enemiesInRange.Count > 0)
                    {
                        choosedEnemie = null;
                        float minDistance = Mathf.Infinity;
                        bool enemieKilleable = false;

                        for(int i = 0; i < unit.enemiesInRange.Count; i++)
                        {
                            float distance = Vector3.Distance(unit.enemiesInRange[i].transform.position, unit.transform.position);
                            if (unit.enemiesInRange[i].isKing)
                            {
                                choosedEnemie = unit.enemiesInRange[i];
                                break;
                            }
                            else if(distance < minDistance &&
                                    !enemieKilleable)
                            {
                                enemieKilleable = unit.attackDamage >= unit.enemiesInRange[i].health;
                                minDistance = distance;
                                choosedEnemie = unit.enemiesInRange[i];
                            }
                        }
                    }
                    if(choosedEnemie != null)
                    {
                        unit.Attack(choosedEnemie);
                        yield return new WaitForSeconds(2);
                    }
                }
            }
            else
            {
                foreach (Unit unit in IAUnits)
                {
                    if (unit.hasAttacked || unit.isKing) continue;
                    Unit choosedEnemie = null;
                    unit.GetEnemies();
                    if (unit.enemiesInRange.Count > 0)
                    {
                        choosedEnemie = null;
                        for (int i = 0; i < unit.enemiesInRange.Count; i++)
                        {
                            if (unit.enemiesInRange[i].isKing)
                            {
                                choosedEnemie = unit.enemiesInRange[i];
                                break;
                            }
                        }
                    }
                    if (choosedEnemie != null)
                    {
                        unit.Attack(choosedEnemie);
                        yield return new WaitForSeconds(2);
                    }
                }
            }

        }

        GM.instance.EndTurn();
        stateMachine.TransitionTo("waiting");
    }

    public void changeState(string nameState)
    {
        stateMachine.TransitionTo(nameState);
    }

    IEnumerator MoveUnits()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        IAUnits = new List<Unit>();
        targetUnit = null;
        foreach(Unit unit in allUnits)
        {
            if(unit.playerNumber == GM.instance.playerTurn)
            {
                IAUnits.Add(unit);
                if (unit.isKing) myKing = unit;
            }
            else //Aquí entran todos las unidades enemigas
            {
                if(GM.instance.IAVisibilityMap.GetNodeFromWorldPosition(unit.transform.position).visible) //Solo tiene en cuenta los enemigos que puede ver
                {
                    if (unit.isKing) //Y se queda con el nexo si lo ve, sino, con uno cualquiera
                        targetUnit = unit;
                    else if (targetUnit == null)
                        targetUnit = unit;
                }
            }
            yield return null;
        }

        foreach (Unit unit in IAUnits)
        {
            if (unit.hasMoved || unit.isKing) continue;
            List<Tile> walkableTiles = unit.IAGetWalkableTiles();
            Tile choosedTile = null;

            if (walkableTiles.Count > 0)
            {
                choosedTile = ChooseTile(walkableTiles, targetUnit);
            }
            

            if (choosedTile != null)
            {
                unit.Move(choosedTile.transform.position);
                yield return new WaitForSeconds(1.5f);
            }
        }
        stateMachine.TransitionTo("attacking");
    }

    Tile ChooseTile(List<Tile> reachableTiles, Unit target)
    {

        Tile choosedTile = reachableTiles[0];
        Vector3 targetPos;
        if(GM.instance.gameState == GM.GameState.Early) //Defienden la base
        {

            if (target != null)
                targetPos = target.transform.position;
            else
                targetPos = myKing.transform.position;
        }
        else if(GM.instance.gameState == GM.GameState.Mid)
        {
            if (target != null)
                targetPos = target.transform.position;
            else
                targetPos = defaultTarget.transform.position;
        }
        else
        {
            targetPos = defaultTarget.transform.position;
        }
        if (reachableTiles.Count > 1)
        {
            for (int i = 1; i < reachableTiles.Count; i++)
            {
                if (Vector3.Distance(reachableTiles[i].transform.position, targetPos) < Vector3.Distance(choosedTile.transform.position, targetPos))
                {
                    choosedTile = reachableTiles[i];
                }
            }
        }

        return choosedTile;
    }
}
