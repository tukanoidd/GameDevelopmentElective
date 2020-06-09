using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipStatsManager : MonoBehaviour
{
    public bool isDead = false;

    [SerializeField] private TextMeshProUGUI shipName;
    [SerializeField] private TextMeshProUGUI lifeStatus;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI powerUpText;
    [SerializeField] private TextMeshProUGUI burningText;

    private Ship _assignedShip;

    // Update is called once per frame
    void Update()
    {
        if (!CompetitionManager.current.gameOver && _assignedShip && !isDead)
        {
            lifeStatus.color = _assignedShip.dying ? Color.yellow : Color.green;
            lifeStatus.text = _assignedShip.dying ? "Dying" : "Alive";

            healthBarFill.color = _assignedShip.health > (healthBar.maxValue * 2 / 3)
                ? Color.green
                : (_assignedShip.health > (healthBar.maxValue / 3)
                    ? Color.yellow
                    : Color.red);
            healthBar.value = _assignedShip.health;

            powerUpText.text = _assignedShip.GetPowerUp(this).ToString();

            bool isBurning = _assignedShip.IsBurning(this);
            burningText.color = isBurning ? Color.yellow : Color.green;
            burningText.text = isBurning ? "Yes" : "No";
        }
        else if (!CompetitionManager.current.gameOver && isDead)
        {
            lifeStatus.color = Color.red;
            lifeStatus.text = "Dead";
        }
    }

    public Ship GetShip(object caller)
    {
        if (caller is Ship || caller is CompetitionManager) return _assignedShip;
        return null;
    }

    public void SetShip(object caller, Ship ship)
    {
        if (caller is CompetitionManager)
        {
            _assignedShip = ship;
            shipName.text = _assignedShip.name.Replace("(Clone)", "");
        }
    }
}