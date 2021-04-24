using System;
using System.Collections.Generic;
using Godot;

public class Main : Node2D
{
    Kingdom Kingdom;
    Battle Battle;

    enum State
    {
        Kingdom,
        Battle
    }

    private State CurrentState = State.Kingdom;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Kingdom = GetNode<Kingdom>("Kingdom");
        Battle = GetNode<Battle>("Battle");
        Kingdom.Connect("BattleInitiated", this, nameof(OnBattleInitiated));
        Battle.Connect("Retreated", this, nameof(OnRetreated));
    }

    private void OnRetreated(Leader leader)
    {
        CurrentState = State.Kingdom;

        Unit[] playerUnits = Battle.GetPlayerUnits().ToArray();
        Battle.RemovePlayerUnits();
        Kingdom.AddPlayerUnits(new List<Unit>(playerUnits));
        Unit[] enemyUnits = Battle.GetEnemyUnits().ToArray();
        Battle.RemoveEnemyUnits();
        leader.AddUnits(new List<Unit>(enemyUnits));
    }

    private void OnBattleInitiated(Leader leader)
    {
        CurrentState = State.Battle;
        Battle.SetEnemyLeader(leader);

        Unit[] playerUnits = Kingdom.GetPlayerUnits().ToArray();
        Kingdom.RemovePlayerUnits();
        Battle.AddPlayerUnits(new List<Unit>(playerUnits));
        Unit[] enemyUnits = leader.GetUnits().ToArray();
        leader.RemoveUnits();
        Battle.AddEnemyUnits(new List<Unit>(enemyUnits));
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (CurrentState == State.Kingdom && @event.IsPressed() && @event is InputEventMouseButton)
        {
            Kingdom.SetPlayerDestination(((InputEventMouseButton)@event).Position);
        }
    }

    public override void _Process(float delta)
    {
        switch (CurrentState)
        {
            case State.Battle:
                Battle.Show();
                Kingdom.Hide();
                break;
            case State.Kingdom:
                Kingdom.Show();
                Battle.Hide();
                break;
        }
    }
}
