using System;
using System.Collections.Generic;
using Godot;

public class Kingdom : Node2D
{

    [Signal]
    public delegate void BattleInitiated(Leader leader);
    private Leader PlayerLeader;
    private Leader EnemyLeader1;
    private Leader EnemyLeader2;
    private Leader EnemyLeader3;
    private Leader EnemyLeader4;
    private Leader EnemyLeader5;

    private Button ChallengeButton;
    private Button BorrowButton;
    private Button BorrowDoneButton;

    enum State
    {
        Default,
        Selection,
        Borrow,
    }

    private Leader SelectedLeader = null;

    private State CurrentState = State.Default;

    PackedScene UnitScene = GD.Load<PackedScene>("res://Scenes/Unit.tscn");

    public override void _Ready()
    {
        PlayerLeader = GetNode<Leader>("PlayerLeader");
        EnemyLeader1 = GetNode<Leader>("EnemyLeaders/EnemyLeader1");
        EnemyLeader2 = GetNode<Leader>("EnemyLeaders/EnemyLeader2");
        EnemyLeader3 = GetNode<Leader>("EnemyLeaders/EnemyLeader3");
        EnemyLeader4 = GetNode<Leader>("EnemyLeaders/EnemyLeader4");
        EnemyLeader5 = GetNode<Leader>("EnemyLeaders/EnemyLeader5");
        ChallengeButton = GetNode<Button>("ChallengeButton");
        BorrowButton = GetNode<Button>("BorrowButton");
        BorrowDoneButton = GetNode<Button>("BorrowDoneButton");

        Vector2 MidPoint = GetViewport().Size / 2;
        ChallengeButton.RectPosition = MidPoint - ChallengeButton.RectSize;

        PopulateUnits(PlayerLeader, new List<int> { 1, });
        PopulateUnits(EnemyLeader1, new List<int> { 1, 1, 4, 4, 5 });
        PopulateUnits(EnemyLeader2, new List<int> { 1, 2, 2, 3, 3 });
        PopulateUnits(EnemyLeader3, new List<int> { 1, 1, 1, 2, 3, 4 });
        PopulateUnits(EnemyLeader4, new List<int> { 5, 5, 5, 5 });
        PopulateUnits(EnemyLeader5, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 });

        PlayerLeader.Connect("LeaderSelected", this, nameof(OnLeaderSelected));
        PlayerLeader.Connect("LeaderDeselected", this, nameof(OnLeaderDeselected));
        ChallengeButton.Connect("pressed", this, nameof(OnChallengeButtonPressed));
        BorrowButton.Connect("pressed", this, nameof(OnBorrowButtonPressed));
        BorrowDoneButton.Connect("pressed", this, nameof(OnBorrowDoneButtonPressed));
    }

    public List<Unit> GetPlayerUnits()
    {
        return PlayerLeader.GetUnits();
    }

    public void SetPlayerDestination(Vector2 destination)
    {
        if (CurrentState != State.Borrow)
        {
            PlayerLeader.SetDestination(destination);
        }
    }

    public void RemovePlayerUnits()
    {
        PlayerLeader.RemoveUnits();
    }

    public void AddPlayerUnits(List<Unit> units)
    {
        PlayerLeader.AddUnits(units);
    }

    private void OnLeaderSelected(Leader leader)
    {
        SelectedLeader = leader;
        CurrentState = State.Selection;

        // TODO: compare leader with player units
        foreach (Unit leaderUnit in SelectedLeader.GetUnits())
        {

        }
    }

    private void OnLeaderDeselected()
    {
        SelectedLeader = null;
        CurrentState = State.Default;
    }

    private void OnChallengeButtonPressed()
    {
        CurrentState = State.Default;
        EmitSignal(nameof(BattleInitiated), SelectedLeader);
    }

    private void OnBorrowButtonPressed()
    {
        CurrentState = State.Borrow;
        PlayerLeader.CancelNavigation();
        SelectedLeader.StartBorrowMode();
    }

    private void OnBorrowDoneButtonPressed()
    {
        Unit[] leaderSelectedUnits = SelectedLeader.GetSelectedUnits().ToArray();
        foreach (Unit unit in leaderSelectedUnits)
        {
            Vector2 offset = SelectedLeader.GetBodyGlobalPosition() - PlayerLeader.GetBodyGlobalPosition();
            unit.Position += PlayerLeader.GetBodyPosition() - SelectedLeader.GetBodyPosition() + offset;
        }
        Unit[] playerUnits = PlayerLeader.GetUnits().ToArray();
        SelectedLeader.RemoveSelectedUnits();
        List<Unit> totalSelectedUnits = new List<Unit>(leaderSelectedUnits);
        totalSelectedUnits.AddRange(playerUnits);
        PlayerLeader.AddUnits(totalSelectedUnits);
        CurrentState = State.Default;
        SelectedLeader.StopBorrowMode();
    }

    private void PopulateUnits(Leader leader, List<int> unitPowers)
    {
        List<Unit> followers = new List<Unit>();
        Random random = new Random();
        for (int unitIdx = 0; unitIdx < unitPowers.Count; unitIdx++)
        {
            Unit unitInstance = (Unit)UnitScene.Instance();
            unitInstance.Power = unitPowers[unitIdx];
            unitInstance.Position = new Vector2(unitIdx * 64 - unitPowers.Count * 64 / 2, random.Next(-100, 100));
            unitInstance.SetFactionId((int)leader.Name.Hash());
            followers.Add(unitInstance);
        }
        leader.AddUnits(followers);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (CurrentState)
        {
            case State.Default:
                ChallengeButton.Hide();
                BorrowButton.Hide();
                BorrowDoneButton.Hide();
                break;
            case State.Selection:
                ChallengeButton.Show();
                BorrowButton.Show();
                BorrowDoneButton.Hide();
                ChallengeButton.RectPosition = SelectedLeader.Position - ChallengeButton.RectSize / 2 + Vector2.Up * 20;
                BorrowButton.RectPosition = SelectedLeader.Position - BorrowButton.RectSize / 2 + Vector2.Down * ChallengeButton.RectSize.y;
                break;
            case State.Borrow:
                ChallengeButton.Hide();
                BorrowButton.Hide();
                BorrowDoneButton.Show();
                BorrowDoneButton.RectPosition = SelectedLeader.Position - BorrowDoneButton.RectSize / 2 + Vector2.Down * ChallengeButton.RectSize.y;
                break;
        }
    }
}
