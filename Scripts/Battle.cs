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
    private Timer WinnerTimer;

    private Leader EnemyLeader;

    private Label WinnerLabel;
    private Tween WinnerTween;

    private Boolean IsAllPlayerUnitsResting;

    private AudioStreamPlayer WinAudio;
    private AudioStreamPlayer LoseAudio;


    private Label HintLabel;

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
        WinnerLabel = GetNode<Label>("WinnerLabel");
        WinnerTween = GetNode<Tween>("WinnerTween");
        HintLabel = GetNode<Label>("HintLabel");
        WinAudio = GetNode<AudioStreamPlayer>("WinAudio");
        LoseAudio = GetNode<AudioStreamPlayer>("LoseAudio");

        EngageButton.Connect("pressed", this, nameof(OnEngagePressed));
        RetreatButton.Connect("pressed", this, nameof(OnRetreatPressed));
        EngageTimer.Connect("timeout", this, nameof(OnEngageTimeout));

        UpdateArmyPositions();
    }

    private void ShowWinnerLabel(String text, Army army)
    {
        WinnerLabel.Text = text;
        WinnerLabel.RectPosition = army.Position - WinnerLabel.RectSize / 2;
        WinnerTween.InterpolateProperty(
            WinnerLabel, "modulate",
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f, 0.0f),
            2f, Tween.TransitionType.Quad,
            Tween.EaseType.Out
        );
        WinnerTween.Start();
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
            ShowWinnerLabel("You win!", PlayerArmy);
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
            ShowWinnerLabel("Enemy wins!", EnemyArmy);
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

        if (EnemyArmy.GetUnits().Count == 0 || PlayerArmy.GetUnits().Count == 0)
        {
            if (EnemyArmy.GetUnits().Count == 0)
            {
                WinAudio.Play();
            }
            else
            {
                LoseAudio.Play();
            }
            OnRetreatPressed();
        }
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
        EngageButton.RectPosition = MidPoint - EngageButton.RectSize / 2;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (CurrentState)
        {
            case State.PreBattle:
                if (PlayerArmy.GetSelectedUnits().Count > 0)
                {
                    EngageButton.Show();
                }
                else
                {
                    EngageButton.Hide();
                }
                break;
            case State.InBattle:
                EngageButton.Hide();
                break;
        }

        IsAllPlayerUnitsResting = true;
        foreach (Unit unit in PlayerArmy.GetUnits().ToArray())
        {
            if (!unit.IsResting())
            {
                IsAllPlayerUnitsResting = false;
                break;
            }
        }
        if (IsAllPlayerUnitsResting)
        {
            HintLabel.Show();
        }
        else
        {
            HintLabel.Hide();
        }
    }
}
