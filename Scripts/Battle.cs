using System;
using System.Collections.Generic;
using Godot;

public class Battle : Node2D
{
    [Signal]
    public delegate void Retreated(Leader leader);

    private Army PlayerArmy;
    private Army EnemyArmy;

    private Button EngageButton;
    private Button RetreatButton;

    private Timer EngageTimer;

    private Leader EnemyLeader;

    enum State
    {
        PreBattle,
        InBattle,

    }

    private State CurrentState = State.PreBattle;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlayerArmy = GetNode<Army>("PlayerArmy");
        EnemyArmy = GetNode<Army>("EnemyArmy");
        EngageButton = GetNode<Button>("EngageButton");
        RetreatButton = GetNode<Button>("RetreatButton");
        EngageTimer = GetNode<Timer>("EngageTimer");

        EngageButton.Connect("pressed", this, nameof(OnEngagePressed));
        RetreatButton.Connect("pressed", this, nameof(OnRetreatPressed));
        EngageTimer.Connect("timeout", this, nameof(OnEngageTimeout));

        UpdateArmyPositions();
    }

    private void OnEngageTimeout()
    {
        PlayerArmy.WakeUnits();
        EnemyArmy.WakeUnits();

        List<Unit> enemySelectedUnits = EnemyArmy.GetSelectedUnits();
        List<Unit> enemyUnits = new List<Unit>(EnemyArmy.GetUnits());
        List<Unit> playerSelectedUnits = PlayerArmy.GetSelectedUnits();
        List<Unit> playerUnits = new List<Unit>(PlayerArmy.GetUnits());
        int playerPower = 0;
        int enemyPower = 0;
        foreach (Unit enemyUnit in enemySelectedUnits)
        {
            enemyPower += enemyUnit.Power;
        }
        foreach (Unit playerUnit in playerSelectedUnits)
        {
            playerPower += playerUnit.Power;
        }
        if (playerPower > enemyPower)
        {
            if (enemyPower == 0)
            {
                EnemyArmy.RemoveUnits();
                PlayerArmy.AddRestingUnits(enemyUnits);
            }
            else
            {
                EnemyArmy.RemoveSelectedUnits();
                PlayerArmy.AddRestingUnits(enemySelectedUnits);
            }
        }
        else if (enemyPower > playerPower)
        {

            if (playerPower == 0)
            {
                PlayerArmy.RemoveUnits();
                EnemyArmy.AddRestingUnits(playerUnits);
            }
            else
            {
                PlayerArmy.RemoveSelectedUnits();
                EnemyArmy.AddRestingUnits(playerSelectedUnits);
            }
        }
        PlayerArmy.UpdateUnitPositions();
        EnemyArmy.UpdateUnitPositions();
        PlayerArmy.DeselectAll();
        EnemyArmy.DeselectAll();

        if (EnemyArmy.GetArmyPower() <= 0)
        {
            EnemyLeader.SetDefeated();
        }

        CurrentState = State.PreBattle;
    }

    private void OnEngagePressed()
    {
        EnemyArmy.SetRandomSelection();
        List<Unit> units = new List<Unit>(EnemyArmy.GetSelectedUnits());
        units.AddRange(PlayerArmy.GetSelectedUnits());
        foreach (Unit unit in units.ToArray())
        {
            unit.SetNavigating(GetViewportRect().Size / 2);
        }

        EngageTimer.Start();
        CurrentState = State.InBattle;
    }

    public void SetEnemyLeader(Leader leader)
    {
        this.EnemyLeader = leader;
    }

    private void OnRetreatPressed()
    {
        EmitSignal(nameof(Retreated), EnemyLeader);
    }

    public void AddPlayerUnits(List<Unit> units)
    {
        PlayerArmy.AddUnits(units);
    }

    public List<Unit> GetPlayerUnits()
    {
        return PlayerArmy.GetUnits();
    }

    public void RemovePlayerUnits()
    {
        PlayerArmy.RemoveUnits();
    }

    public void RemoveEnemyUnits()
    {
        EnemyArmy.RemoveUnits();
    }

    public List<Unit> GetEnemyUnits()
    {
        return EnemyArmy.GetUnits();
    }

    public void AddEnemyUnits(List<Unit> units)
    {
        EnemyArmy.AddUnits(units);
    }

    private void UpdateArmyPositions()
    {
        Vector2 screenSize = GetViewportRect().Size;
        // screenSize = GetViewport().Size
        Vector2 NorthPoint = new Vector2(screenSize.x / 2, 0);
        Vector2 SouthPoint = new Vector2(screenSize.x / 2, screenSize.y);
        Vector2 MidPoint = screenSize / 2;
        PlayerArmy.Position = (MidPoint + SouthPoint) / 2;
        EnemyArmy.Position = (MidPoint + NorthPoint) / 2;
        EngageButton.RectPosition = MidPoint - EngageButton.RectSize;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (CurrentState)
        {
            case State.PreBattle:
                EngageButton.Show();
                break;
            case State.InBattle:
                EngageButton.Hide();
                break;
        }
    }
}
