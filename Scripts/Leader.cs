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
    private Boolean IsCPU = false;
    private KinematicBody2D KinematicBody;
    private Vector2 DirectionFacing = Vector2.Right;
    private List<Unit> Units = new List<Unit>();

    private Node2D FollowersNode;

    private Area2D Area2D;

    private Sprite Highlight;

    private int MovementSpeed = 200;

    private Boolean IsAutoNavigating;
    private Vector2 DestinationPosition;

    enum State
    {
        Idle,
        Hover,
        Borrow,
    }

    private State CurrentState = State.Idle;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        KinematicBody = GetNode<KinematicBody2D>("KinematicBody2D");
        FollowersNode = GetNode<Node2D>("Followers");
        Area2D = GetNode<Area2D>("KinematicBody2D/Area2D");
        Highlight = GetNode<Sprite>("KinematicBody2D/Highlight");

        if (!IsCPU)
        {
            Area2D.Connect("area_entered", this, nameof(OnAreaEntered));
            Area2D.Connect("area_exited", this, nameof(OnAreaExited));
        }
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

    private void OnAreaEntered(Area2D area)
    {
        Leader leader = (Leader)area.GetParent().GetParent();
        leader.SetHover();
        EmitSignal(nameof(LeaderSelected), leader);
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

    private void OnAreaExited(Area2D area)
    {
        Leader leader = (Leader)area.GetParent().GetParent();
        leader.SetIdle();
        leader.StopBorrowMode();
        EmitSignal(nameof(LeaderDeselected));
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
            }
        }
    }

    public void AddUnits(List<Unit> units)
    {
        Units.Clear();
        Units.AddRange(units);
        foreach (Unit unit in units.ToArray())
        {
            FollowersNode.AddChild(unit);
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
            }
            else
            {
                IsAutoNavigating = false;
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
                break;
            case State.Hover:
                Highlight.Show();
                break;
        }
    }
}
