using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ThrowerDice : MonoBehaviour
{
    public Dice diceToThrow;
    public int amountOfDice = 2;
    public float throwForce = 5f;
    public float rollForce = 10f;

    private List<GameObject> _spawnDice = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) RollDice();
    }

    private async void RollDice()
    {
        if (diceToThrow == null) return;

        foreach (var dice in _spawnDice)
        {
            Destroy(dice);
        }

        for (int i = 0; i < amountOfDice; i++)
        {
            Dice dice = Instantiate(diceToThrow, transform.position, transform.rotation);
            _spawnDice.Add(dice.gameObject);
            dice.RollDice(throwForce, rollForce, i);
            await Task.Yield();
        }
    }
}
