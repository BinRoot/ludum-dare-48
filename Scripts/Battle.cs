using System;
using System.Collections.Generic;
using Godot;

public class Battle : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private Army PlayerArmy;
    private Army EnemyArmy;

    private Button EngageButton;

    private Timer EngageTimer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlayerArmy = GetNode<Army>("PlayerArmy");
        EnemyArmy = GetNode<Army>("EnemyArmy");
        EngageButton = GetNode<Button>("EngageButton");
        EngageTimer = GetNode<Timer>("EngageTimer");

        EngageButton.Connect("pressed", this, nameof(OnEngagePressed));
        EngageTimer.Connect("timeout", this, nameof(OnEngageTimeout));

        UpdateArmyPositions();
    }

    private void OnEngageTimeout()
    {
        PlayerArmy.WakeUnits();
        EnemyArmy.WakeUnits();
        int playerPower = PlayerArmy.GetTotalPower();
        int enemyPower = EnemyArmy.GetTotalPower();
        GD.Print("player ", playerPower, " enemy ", enemyPower);
        List<Unit> enemySelectedUnits = EnemyArmy.GetSelectedUnits();
        List<Unit> playerSelectedUnits = PlayerArmy.GetSelectedUnits();
        if (playerPower > enemyPower)
        {
            EnemyArmy.RemoveSelectedUnits();
            PlayerArmy.AddRestingUnits(enemySelectedUnits);
        }
        else if (enemyPower > playerPower)
        {
            PlayerArmy.RemoveSelectedUnits();
            EnemyArmy.AddRestingUnits(playerSelectedUnits);
        }
        PlayerArmy.UpdateUnitPositions();
        EnemyArmy.UpdateUnitPositions();
        PlayerArmy.DeselectAll();
        EnemyArmy.DeselectAll();
    }

    private void OnEngagePressed()
    {
        EnemyArmy.SetRandomSelection();
        EngageTimer.Start();
    }

    private void UpdateArmyPositions()
    {
        Vector2 NorthPoint = new Vector2(GetViewport().Size.x / 2, 0);
        Vector2 SouthPoint = new Vector2(GetViewport().Size.x / 2, GetViewport().Size.y);
        Vector2 MidPoint = GetViewport().Size / 2;
        PlayerArmy.Position = (MidPoint + SouthPoint) / 2;
        EnemyArmy.Position = (MidPoint + NorthPoint) / 2;

        EngageButton.RectPosition = MidPoint - EngageButton.RectSize;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
