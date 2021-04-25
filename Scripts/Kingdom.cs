using System;
using System.Collections.Generic;
using Godot;

public class Kingdom : Node2D
{

    [Signal]
    public delegate void BattleInitiated(Leader leader);

    private Node2D EnemyLeadersNode;
    private Leader PlayerLeader;
    private Leader EnemyLeader1;
    private Leader EnemyLeader2;
    private Leader EnemyLeader3;
    private Leader EnemyLeader4;
    private Leader EnemyLeader5;

    private Button ChallengeButton;
    private Button BorrowButton;
    private Button BorrowDoneButton;
    private Button SettleDebtButton;
    private Button SettleDebtDoneButton;

    private Boolean IsFactionWillingToFight;

    enum State
    {
        Default,
        Selection,
        Borrow,
        SettleDebt,
    }

    private Leader SelectedLeader = null;

    private State CurrentState = State.Default;

    PackedScene UnitScene = GD.Load<PackedScene>("res://Scenes/Unit.tscn");

    public override void _Ready()
    {
        PlayerLeader = GetNode<Leader>("PlayerLeader");
        EnemyLeadersNode = GetNode<Node2D>("EnemyLeaders");
        EnemyLeader1 = GetNode<Leader>("EnemyLeaders/EnemyLeader1");
        EnemyLeader2 = GetNode<Leader>("EnemyLeaders/EnemyLeader2");
        EnemyLeader3 = GetNode<Leader>("EnemyLeaders/EnemyLeader3");
        EnemyLeader4 = GetNode<Leader>("EnemyLeaders/EnemyLeader4");
        EnemyLeader5 = GetNode<Leader>("EnemyLeaders/EnemyLeader5");
        ChallengeButton = GetNode<Button>("ChallengeButton");
        BorrowButton = GetNode<Button>("BorrowButton");
        BorrowDoneButton = GetNode<Button>("BorrowDoneButton");
        SettleDebtButton = GetNode<Button>("SettleDebtButton");
        SettleDebtDoneButton = GetNode<Button>("SettleDebtDoneButton");

        Vector2 MidPoint = GetViewport().Size / 2;
        ChallengeButton.RectPosition = MidPoint - ChallengeButton.RectSize;

        PopulateUnits(PlayerLeader, new List<int> { });
        PopulateUnits(EnemyLeader1, new List<int> { 1, 1 });
        PopulateUnits(EnemyLeader2, new List<int> { 1, 2, 2, 3 });
        PopulateUnits(EnemyLeader3, new List<int> { 1, 1, 3, 3, 4 });
        PopulateUnits(EnemyLeader4, new List<int> { 3, 3, 5, 5 });
        PopulateUnits(EnemyLeader5, new List<int> { 4, 4, 4, 5, 5, 5 });

        PlayerLeader.Connect("LeaderSelected", this, nameof(OnLeaderSelected));
        PlayerLeader.Connect("LeaderDeselected", this, nameof(OnLeaderDeselected));
        ChallengeButton.Connect("pressed", this, nameof(OnChallengeButtonPressed));
        BorrowButton.Connect("pressed", this, nameof(OnBorrowButtonPressed));
        BorrowDoneButton.Connect("pressed", this, nameof(OnBorrowDoneButtonPressed));
        SettleDebtButton.Connect("pressed", this, nameof(OnSettleDebtButtonPressed));
        SettleDebtDoneButton.Connect("pressed", this, nameof(OnSettleDebtDoneButtonPressed));
    }

    public List<Unit> GetPlayerUnits()
    {
        return PlayerLeader.GetUnits();
    }

    public void SetPlayerDestination(Vector2 destination)
    {
        if (CurrentState != State.Borrow && CurrentState != State.SettleDebt)
        {
            PlayerLeader.SetDestination(destination);
        }
    }

    public void RemovePlayerUnits()
    {
        PlayerLeader.RemoveUnits();
    }

    public void AddPlayerUnits(List<Unit> units, Boolean forcePosition = false)
    {
        PlayerLeader.AddUnits(units, forcePosition);
    }

    public Vector2 GetPlayerGlobalPosition()
    {
        return PlayerLeader.GetKinematicGlobalPosition();
    }

    private void OnLeaderSelected(Leader leader)
    {
        SelectedLeader = leader;
        CurrentState = State.Selection;

        IsFactionWillingToFight = leader.GetDebt() <= 0;
        // HashSet<int> enemyFactions = new HashSet<int>();
        // enemyFactions.Add(SelectedLeader.FactionId);
        // foreach (Unit enemyUnit in SelectedLeader.GetUnits())
        // {
        //     enemyFactions.Add(enemyUnit.GetFactionId());
        // }

        // IsFactionWillingToFight = true;
        // foreach (Unit playerUnit in PlayerLeader.GetUnits())
        // {
        //     if (enemyFactions.Contains(playerUnit.GetFactionId()))
        //     {
        //         IsFactionWillingToFight = false;
        //     }
        // }
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
        CurrentState = State.Selection;
        SelectedLeader.StopBorrowMode();
    }

    public void RaiseDebt()
    {
        foreach (Node node in EnemyLeadersNode.GetChildren())
        {
            if (node is Leader)
            {
                Leader leader = (Leader)node;
                leader.RaiseDebt();
            }
        }
    }

    private void OnSettleDebtButtonPressed()
    {
        CurrentState = State.SettleDebt;
        PlayerLeader.CancelNavigation();
        PlayerLeader.StartBorrowMode();
    }

    private void OnSettleDebtDoneButtonPressed()
    {
        Unit[] playerSelectedUnits = PlayerLeader.GetSelectedUnits().ToArray();
        foreach (Unit unit in playerSelectedUnits)
        {
            Vector2 offset = PlayerLeader.GetBodyGlobalPosition() - SelectedLeader.GetBodyGlobalPosition();
            unit.Position += SelectedLeader.GetBodyPosition() + offset - PlayerLeader.GetBodyPosition();
        }
        Unit[] enemyUnits = SelectedLeader.GetUnits().ToArray();
        PlayerLeader.RemoveSelectedUnits();
        List<Unit> totalSelectedUnits = new List<Unit>(playerSelectedUnits);
        int credit = 0;
        foreach (Unit playerUnit in playerSelectedUnits)
        {
            credit += playerUnit.Power;
        }
        SelectedLeader.Credit(credit);
        totalSelectedUnits.AddRange(enemyUnits);
        SelectedLeader.AddUnits(totalSelectedUnits);
        CurrentState = State.Selection;
        PlayerLeader.StopBorrowMode();
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
            unitInstance.SetFactionId(leader.FactionId);
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
                SettleDebtButton.Hide();
                SettleDebtDoneButton.Hide();
                break;
            case State.Selection:
                if (!SelectedLeader.IsBorrowOnly)
                {
                    ChallengeButton.Show();
                    ChallengeButton.Disabled = !IsFactionWillingToFight;
                }
                if (SelectedLeader.GetUnits().Count > 0)
                {
                    BorrowButton.Show();
                }
                BorrowDoneButton.Hide();
                SettleDebtDoneButton.Hide();
                if (SelectedLeader.GetDebt() > 0)
                {
                    SettleDebtButton.Show();
                    SettleDebtButton.Text = $"Settle Debt ({SelectedLeader.GetDebt()})";
                }
                else
                {
                    SettleDebtButton.Hide();
                }
                ChallengeButton.RectPosition = SelectedLeader.Position - ChallengeButton.RectSize / 2;
                BorrowButton.RectPosition = SelectedLeader.Position - BorrowButton.RectSize / 2 + Vector2.Down * ChallengeButton.RectSize.y;
                SettleDebtButton.RectPosition = SelectedLeader.Position - SettleDebtButton.RectSize / 2 + Vector2.Down * ChallengeButton.RectSize.y * 2;
                break;
            case State.Borrow:
                ChallengeButton.Hide();
                BorrowButton.Hide();
                BorrowDoneButton.Show();
                SettleDebtButton.Hide();
                SettleDebtDoneButton.Hide();
                BorrowDoneButton.RectPosition = SelectedLeader.Position - BorrowDoneButton.RectSize / 2 + Vector2.Down * ChallengeButton.RectSize.y;
                break;
            case State.SettleDebt:
                ChallengeButton.Hide();
                BorrowButton.Hide();
                BorrowDoneButton.Hide();
                SettleDebtButton.Hide();
                SettleDebtDoneButton.Show();
                SettleDebtDoneButton.RectPosition = SelectedLeader.Position - SettleDebtButton.RectSize / 2 + Vector2.Down * ChallengeButton.RectSize.y * 2;
                break;
        }
    }
}
