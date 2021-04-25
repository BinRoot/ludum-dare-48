using System;
using System.Collections.Generic;
using Godot;

public class Leader : Node2D
{

    [Signal]
    public delegate void LeaderSelected(Leader leader);
    [Signal]
    public delegate void LeaderDeselected();

    [Export]
    public Boolean IsCPU = false;

    [Export]
    public Boolean IsBorrowOnly = false;
    private KinematicBody2D KinematicBody;
    private Vector2 DirectionFacing = Vector2.Right;
    private List<Unit> Units = new List<Unit>();

    private Node2D FollowersNode;

    private Area2D Area2D;

    private Sprite Highlight;

    private AnimatedSprite AnimatedSprite;
    private AnimatedSprite AnimatedSprite2;

    private int MovementSpeed = 200;

    private Boolean IsAutoNavigating;
    private Vector2 DestinationPosition;

    private Label DebtLabel;
    private Label DebugLabel;
    private Label DialogLabel;

    private int Debt = 0;

    private int TutorialStep = 0;

    private float Duration = 0;

    public int FactionId
    {
        get
        {
            return (int)Name.Hash();
        }
    }
    enum State
    {
        Idle,
        Hover,
        Borrow,
        Defeated,
    }

    private Camera2D Camera;
    private Leader PlayerLeader;

    private State CurrentState = State.Idle;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        KinematicBody = GetNode<KinematicBody2D>("KinematicBody2D");
        FollowersNode = GetNode<Node2D>("Followers");
        Area2D = GetNode<Area2D>("KinematicBody2D/Area2D");
        Highlight = GetNode<Sprite>("KinematicBody2D/Highlight");
        AnimatedSprite = GetNode<AnimatedSprite>("KinematicBody2D/AnimatedSprite");
        AnimatedSprite2 = GetNode<AnimatedSprite>("KinematicBody2D/AnimatedSprite2");
        DebtLabel = GetNode<Label>("KinematicBody2D/DebtLabel");
        DebugLabel = GetNode<Label>("KinematicBody2D/DebugLabel");
        DialogLabel = GetNode<Label>("KinematicBody2D/DialogLabel");

        if (!IsCPU)
        {
            Area2D.Connect("area_entered", this, nameof(OnAreaEntered));
            Area2D.Connect("area_exited", this, nameof(OnAreaExited));
        }

        Random random = new Random(FactionId);
        if (IsCPU)
        {
            AnimatedSprite2.Modulate = new Color(random.Next(0, 200) / 256f, random.Next(0, 200) / 256f, random.Next(0, 200) / 256f, 1);
            AnimatedSprite2.Show();
            AnimatedSprite2.Frame = random.Next(0, 4);
            AnimatedSprite.Hide();
        }
        else
        {
            AnimatedSprite.Show();
            AnimatedSprite2.Hide();
        }

        Camera = (Camera2D)GetTree().GetNodesInGroup("camera")[0];
        PlayerLeader = (Leader)GetTree().GetNodesInGroup("player_leader")[0];
    }

    public int GetDebt()
    {
        return Debt;
    }

    public void SetDestination(Vector2 destination)
    {
        DestinationPosition = destination;
        IsAutoNavigating = true;
    }

    public void SetHover()
    {
        CurrentState = State.Hover;
    }

    public void SetIdle()
    {
        CurrentState = State.Idle;
    }

    public Vector2 GetBodyPosition()
    {
        return KinematicBody.Position;
    }

    public Vector2 GetBodyGlobalPosition()
    {
        return KinematicBody.GlobalPosition;
    }

    public void CancelNavigation()
    {
        IsAutoNavigating = false;
    }

    public Boolean IsDefeated()
    {
        return CurrentState == State.Defeated;
    }

    public void SetDefeated()
    {
        CurrentState = State.Defeated;
    }

    private void OnAreaEntered(Area2D area)
    {
        Leader leader = (Leader)area.GetParent().GetParent();
        if (!leader.IsDefeated())
        {
            leader.SetHover();
            EmitSignal(nameof(LeaderSelected), leader);
        }
    }

    private void OnAreaExited(Area2D area)
    {
        Leader leader = (Leader)area.GetParent().GetParent();
        if (!leader.IsDefeated())
        {
            leader.SetIdle();
            leader.StopBorrowMode();
        }
        EmitSignal(nameof(LeaderDeselected));
    }


    public void StartBorrowMode()
    {
        CurrentState = State.Borrow;
        foreach (Unit unit in Units.ToArray())
        {
            unit.SetDefault();
            unit.StartBorrowMode();
        }
    }

    public void StopBorrowMode()
    {
        CurrentState = State.Idle;
        foreach (Unit unit in Units.ToArray())
        {
            unit.SetFollowing();
            unit.StopBorrowMode();
        }
    }

    public void RemoveUnits()
    {
        foreach (Unit unit in Units.ToArray())
        {
            FollowersNode.RemoveChild(unit);
        }
        Units.Clear();
    }

    public void RemoveSelectedUnits()
    {
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsSelected())
            {
                FollowersNode.RemoveChild(unit);
                Units.Remove(unit);
                if (!IsBorrowOnly)
                {
                    Debt += unit.Power;
                }
            }
        }
        UpdateDebtLabel();
    }


    public void AddUnits(List<Unit> units, Boolean forcePosition = false)
    {
        Units.Clear();
        Units.AddRange(units);
        Random random = new Random();
        foreach (Unit unit in units.ToArray())
        {
            FollowersNode.AddChild(unit);
            if (forcePosition)
            {
                unit.GlobalPosition = KinematicBody.GlobalPosition + new Vector2(random.Next(-100, 100), random.Next(-100, 100));
            }
        }
    }

    public Vector2 GetKinematicGlobalPosition()
    {
        return KinematicBody.GlobalPosition;
    }

    public void RaiseDebt()
    {
        if (Debt > 0)
        {
            Debt = (int)(Debt * 1.2);
        }
        UpdateDebtLabel();
    }

    public void Credit(int amount)
    {
        Debt -= amount;
        UpdateDebtLabel();
    }

    private void UpdateDebtLabel()
    {
        if (IsCPU)
        {
            DebtLabel.Text = $"-{Debt} DEBT";
            if (Debt > 0)
            {
                DebtLabel.Show();
            }
            else
            {
                DebtLabel.Hide();
            }
        }
    }

    public List<Unit> GetUnits()
    {
        return Units;
    }

    public List<Unit> GetSelectedUnits()
    {
        List<Unit> selectedUnits = new List<Unit>();
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsSelected())
            {
                selectedUnits.Add(unit);
            }
        }
        return selectedUnits;
    }

    public override void _Process(float delta)
    {
        if (!IsCPU)
        {
            Camera.GlobalPosition = KinematicBody.GlobalPosition;
            Vector2 vector = Vector2.Zero;

            if (Input.IsActionPressed("ui_up"))
            {
                vector.y -= 1;
            }
            if (Input.IsActionPressed("ui_down"))
            {
                vector.y += 1;
            }
            if (Input.IsActionPressed("ui_left"))
            {
                vector.x -= 1;
            }
            if (Input.IsActionPressed("ui_right"))
            {
                vector.x += 1;
            }

            vector = vector.Normalized();
            if (vector.Length() > 0)
            {
                DirectionFacing = new Vector2(vector);
                IsAutoNavigating = false;
                AnimatedSprite.Play();
            }
            else if (!IsAutoNavigating)
            {
                AnimatedSprite.Stop();
            }
            KinematicBody.MoveAndSlide(vector * MovementSpeed);
        }

        if (IsAutoNavigating)
        {
            Vector2 vector = DestinationPosition - KinematicBody.GlobalPosition;
            if (vector.Length() > 50)
            {
                vector = vector.Normalized();
                KinematicBody.MoveAndSlide(vector * MovementSpeed);
                AnimatedSprite.Play();
            }
            else
            {
                IsAutoNavigating = false;
                AnimatedSprite.Stop();
            }
        }

        if (CurrentState != State.Borrow)
        {
            foreach (Unit unit in Units.ToArray())
            {
                unit.SetLeaderPosition(KinematicBody.GlobalPosition);
                unit.SetFollowing();
            }
        }

        switch (CurrentState)
        {
            case State.Idle:
                Highlight.Hide();
                DebugLabel.Hide();
                break;
            case State.Hover:
                Highlight.Show();
                DebugLabel.Hide();
                break;
            case State.Defeated:
                DebugLabel.Show();
                break;
        }

        Duration += delta;
        if (Name == "EnemyLeader1")
        {
            if (TutorialStep == 0 && Duration > 0.5)
            {
                DialogLabel.Text = "Prince, is that really you?";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 1 && CurrentState == State.Borrow && Duration > 0.2)
            {
                DialogLabel.Text = "Go ahead, you'll need them!";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 2 && CurrentState == State.Borrow && Duration > 3)
            {
                DialogLabel.Text = "Pick my units, then press done.";
            }
            else if (TutorialStep == 2 && CurrentState == State.Idle && Duration > 0.2 && GetUnits().Count == 0)
            {
                DialogLabel.Text = "Excellent!";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 2 && CurrentState == State.Idle && Duration > 0.2 && GetUnits().Count > 0)
            {
                DialogLabel.Text = "Borrow my units, you'll need them.";
                Duration = 0;
            }
            else if (TutorialStep == 3 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 4 && Duration > 0.5)
            {
                DialogLabel.Text = "Be wise borrowing units...";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 5 && Duration > 3)
            {
                DialogLabel.Text = "... or you'll be deep in debt!";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 6 && Duration > 3)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 7 && Duration > 0.5)
            {
                DialogLabel.Text = "Interest rates are high around here...";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 8 && Duration > 4)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
        }
        else if (Name == "EnemyLeader2")
        {
            if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300)
            {
                DialogLabel.Text = "Hey, look it's the prince!";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 1 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (CurrentState == State.Hover && Debt > 0)
            {
                DialogLabel.Text = "Repay my debt, to fight with honor!";
                TutorialStep = 20;
                Duration = 0;
            }
            else if (TutorialStep == 20 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
        }
        else if (Name == "EnemyLeader3")
        {
            if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300)
            {
                DialogLabel.Text = "If you borrow my men...";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 1 && Duration > 2)
            {
                DialogLabel.Text = "... then you better pay me back";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 2 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (CurrentState == State.Borrow && TutorialStep < 10)
            {
                DialogLabel.Text = "Take what you need.";
                TutorialStep = 10;
                Duration = 0;
            }
            else if (TutorialStep == 10 && Duration > 3)
            {
                DialogLabel.Text = "I expect them back later.";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 11 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (CurrentState == State.Hover && Debt > 0)
            {
                DialogLabel.Text = "Repay my debt, to fight with honor!";
                TutorialStep = 20;
                Duration = 0;
            }
            else if (TutorialStep == 20 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
        }
        else if (Name == "EnemyLeader4")
        {
            if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300)
            {
                DialogLabel.Text = "This isn't amateur hour!";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 1 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
        }
        else if (Name == "EnemyLeader5")
        {
            if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300 && PlayerLeader.GetUnits().Count <= 4)
            {
                DialogLabel.Text = "That's all you got?";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300 && PlayerLeader.GetUnits().Count > 4)
            {
                DialogLabel.Text = "You seem like a worthy opponent!";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 1 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
        }

        else if (Name == "EnemyLeader6")
        {
            if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300 && PlayerLeader.GetUnits().Count <= 4)
            {
                DialogLabel.Text = "I'll lend you some.";
                TutorialStep += 1;
                Duration = 0;
            }
            else if (TutorialStep == 0 && PlayerLeader.GetKinematicGlobalPosition().DistanceTo(GetKinematicGlobalPosition()) < 300 && PlayerLeader.GetUnits().Count > 4)
            {
                DialogLabel.Text = "I've been waiting to fight you!";
                TutorialStep = 1;
                Duration = 0;
            }
            else if (TutorialStep == 1 && Duration > 2)
            {
                DialogLabel.Text = "";
                TutorialStep += 1;
                Duration = 0;
            }
        }
    }
}
