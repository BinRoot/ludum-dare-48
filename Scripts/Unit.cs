using System;
using Godot;

public class Unit : KinematicBody2D
{
    [Signal]
    public delegate void UnitSelected(Unit unit);

    [Export]
    public int Power;

    [Export]
    private Boolean IsCPU = false;
    private Boolean IsBorrowMode = false;

    private Label Label1;
    private Label Label2;
    private Area2D Area2D;

    private Vector2 LeaderPosition = Vector2.Zero;

    private Sprite Highlight;

    private State CurrentState = State.Default;
    enum State
    {
        Default,
        MouseHover,
        Selected,
        Resting,
        Following
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Label1 = GetNode<Label>("Label1");
        Label2 = GetNode<Label>("Label2");
        Area2D = GetNode<Area2D>("Area2D");
        Highlight = GetNode<Sprite>("Highlight");

        Label1.Text = Power.ToString();
        Label1.AddColorOverride("font_color", new Color(0.1f, 0.2f, 0, 1));
        Label1.RectPosition = new Vector2(-100, -100);
        Label1.RectSize = new Vector2(200, 200);

        Label2.AddColorOverride("font_color", new Color(0.1f, 0.2f, 0, 1));
        Label2.RectPosition = new Vector2(-10, 0);
        Label2.RectSize = new Vector2(20, 20);

        Highlight.Hide();

        SetIsCPU(IsCPU);
    }

    public Boolean IsSelected()
    {
        return CurrentState == State.Selected;
    }

    public void StartBorrowMode()
    {
        GD.Print(Name, " starting borrow mode");
        IsBorrowMode = true;
        UpdateSignals();
    }

    public void StopBorrowMode()
    {
        IsBorrowMode = false;
        UpdateSignals();
    }

    public void SetSelected()
    {
        CurrentState = State.Selected;
        EmitSignal(nameof(UnitSelected), this);
    }

    public void SetResting()
    {
        CurrentState = State.Resting;
        EmitSignal(nameof(UnitSelected), this);
    }

    public Boolean IsResting()
    {
        return CurrentState == State.Resting;
    }

    public void SetDefault()
    {
        CurrentState = State.Default;
        EmitSignal(nameof(UnitSelected), this);
    }

    public void SetLeaderPosition(Vector2 position)
    {
        LeaderPosition = position;
    }

    public void SetFollowing()
    {
        CurrentState = State.Following;
    }

    private void UpdateSignals()
    {
        if (IsCPU && !IsBorrowMode)
        {
            GD.Print(Name, " disconnecting");
            if (Area2D.IsConnected("mouse_entered", this, nameof(OnMouseEntered)))
            {
                Area2D.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
            }
            if (Area2D.IsConnected("mouse_exited", this, nameof(OnMouseExited)))
            {
                Area2D.Disconnect("mouse_exited", this, nameof(OnMouseExited));
            }
            if (Area2D.IsConnected("input_event", this, nameof(OnInputEvent)))
            {
                Area2D.Disconnect("input_event", this, nameof(OnInputEvent));
            }
        }
        else
        {
            GD.Print(Name, " connecting");
            if (!Area2D.IsConnected("mouse_entered", this, nameof(OnMouseEntered)))
            {
                Area2D.Connect("mouse_entered", this, nameof(OnMouseEntered));
            }
            if (!Area2D.IsConnected("mouse_exited", this, nameof(OnMouseExited)))
            {
                Area2D.Connect("mouse_exited", this, nameof(OnMouseExited));
            }
            if (!Area2D.IsConnected("input_event", this, nameof(OnInputEvent)))
            {
                Area2D.Connect("input_event", this, nameof(OnInputEvent));
            }
        }
    }

    public void SetIsCPU(Boolean isCpu)
    {
        this.IsCPU = isCpu;
        UpdateSignals();
    }

    private void OnInputEvent(Node viewPort, InputEvent inputEvent, int shapeIdx)
    {
        if (inputEvent.IsPressed() && inputEvent is InputEventMouseButton)
        {
            if (CurrentState == State.Resting)
            {
                return;
            }
            if (CurrentState != State.Selected)
            {
                CurrentState = State.Selected;
            }
            else
            {
                CurrentState = State.Default;
            }
            EmitSignal(nameof(UnitSelected), this);
        }
    }

    private void OnMouseEntered()
    {
        GD.Print(Name, " mouse entered ", CurrentState);
        if (CurrentState == State.Default)
        {
            CurrentState = State.MouseHover;
        }
    }

    private void OnMouseExited()
    {
        if (CurrentState == State.MouseHover)
        {
            CurrentState = State.Default;
        }
    }

    private void FollowLeader()
    {
        Vector2 direction = LeaderPosition - GlobalPosition;
        if (direction.Length() > 64)
        {
            direction = direction.Normalized();
            MoveAndSlide(direction * 80);
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (CurrentState)
        {
            case State.Default:
                Highlight.Hide();
                Label2.Hide();
                break;
            case State.MouseHover:
                Highlight.Show();
                Label2.Hide();
                break;
            case State.Selected:
                Highlight.Show();
                Label2.Hide();
                break;
            case State.Resting:
                Highlight.Hide();
                Label2.Show();
                break;
            case State.Following:
                Highlight.Hide();
                Label2.Hide();
                FollowLeader();
                break;
        }
    }
}
