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

    private Camera2D FollowCam;
    private Camera2D BattleCam;

    PackedScene MarkerScene = GD.Load<PackedScene>("res://Scenes/Marker.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Kingdom = GetNode<Kingdom>("Kingdom");
        Battle = GetNode<Battle>("Battle");
        FollowCam = (Camera2D)GetTree().GetNodesInGroup("camera")[0];
        BattleCam = GetNode<Camera2D>("BattleCam");

        Kingdom.Connect("BattleInitiated", this, nameof(OnBattleInitiated));
        Battle.Connect("Retreated", this, nameof(OnRetreated));
    }

    private void OnRetreated(Leader leader)
    {
        FollowCam.MakeCurrent();
        // BattleCam.ClearCurrent();
        CurrentState = State.Kingdom;

        Unit[] playerUnits = Battle.GetPlayerUnits().ToArray();
        foreach (Unit playerUnit in playerUnits)
        {
            playerUnit.CollisionMask = 4;
            playerUnit.CollisionLayer = 4;
        }
        Battle.RemovePlayerUnits();
        Kingdom.AddPlayerUnits(new List<Unit>(playerUnits), true);
        Unit[] enemyUnits = Battle.GetEnemyUnits().ToArray();
        foreach (Unit enemyUnit in enemyUnits)
        {
            enemyUnit.CollisionMask = 4;
            enemyUnit.CollisionLayer = 4;
        }
        Battle.RemoveEnemyUnits();
        leader.AddUnits(new List<Unit>(enemyUnits));
        Kingdom.RaiseDebt();
    }

    private void OnBattleInitiated(Leader leader)
    {
        BattleCam.MakeCurrent();
        // FollowCam.ClearCurrent();
        CurrentState = State.Battle;
        Battle.SetEnemyLeader(leader);

        Unit[] playerUnits = Kingdom.GetPlayerUnits().ToArray();
        foreach (Unit playerUnit in playerUnits)
        {
            playerUnit.CollisionMask = 16;
            playerUnit.CollisionLayer = 16;
        }
        Kingdom.RemovePlayerUnits();
        Battle.AddPlayerUnits(new List<Unit>(playerUnits));
        Unit[] enemyUnits = leader.GetUnits().ToArray();
        foreach (Unit enemyUnit in enemyUnits)
        {
            enemyUnit.CollisionMask = 16;
            enemyUnit.CollisionLayer = 16;
        }
        leader.RemoveUnits();
        Battle.AddEnemyUnits(new List<Unit>(enemyUnits));
    }


    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (CurrentState == State.Kingdom && @event.IsPressed() && @event is InputEventMouseButton)
        {
            Vector2 pos = GetGlobalMousePosition();
            Kingdom.SetPlayerDestination(pos);
            Marker marker = (Marker)MarkerScene.Instance();
            AddChild(marker);
            marker.GlobalPosition = pos;
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
