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

    private int Speed = 120;

    private Vector2 LeaderPosition = Vector2.Zero;

    private Sprite Highlight;
    private AnimatedSprite AnimatedSprite;

    private int FactionId;

    private Color ModulateColor;

    private State CurrentState = State.Default;
    enum State
    {
        Default,
        MouseHover,
        Selected,
        Resting,
        Following,
        Navigating,
    }

    private Random Random;

    private Vector2 DestinationPosition;
    private Boolean IsAutoNavigating = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Label1 = GetNode<Label>("Label1");
        Label2 = GetNode<Label>("Label2");
        Area2D = GetNode<Area2D>("Area2D");
        Highlight = GetNode<Sprite>("Highlight");
        AnimatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        Label1.Text = Power.ToString();
        Label1.RectPosition = new Vector2(-100, -100);
        Label1.RectSize = new Vector2(200, 200);

        Label2.RectPosition = new Vector2(-10, 0);
        Label2.RectSize = new Vector2(20, 20);

        Highlight.Hide();
        Random = new Random();

        SetIsCPU(IsCPU);
    }

    public Boolean IsSelected()
    {
        return CurrentState == State.Selected;
    }

    public void StartBorrowMode()
    {
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

    public void SetNavigating(Vector2 destinationPosition)
    {
        CurrentState = State.Navigating;
        DestinationPosition = destinationPosition;
    }

    public Boolean IsNavigating()
    {
        return CurrentState == State.Navigating;
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
        if (Area2D != null)
        {
            UpdateSignals();
        }
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

    public void SetFactionId(int id)
    {
        FactionId = id;
        Random = new Random(FactionId);
        ModulateColor = new Color(Random.Next(0, 255) / 256f, Random.Next(0, 255) / 256f, Random.Next(0, 255) / 256f, 1);
    }

    public int GetFactionId()
    {
        return FactionId;
    }

    private void OnMouseEntered()
    {
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

    private void NavigateTo(Vector2 position, int delta = 128)
    {
        Vector2 direction = position - GlobalPosition;
        if (direction.Length() > delta)
        {
            direction = direction.Normalized();
            MoveAndSlide(direction * Speed);
            AnimatedSprite.Play();
        }
        else
        {
            AnimatedSprite.Stop();
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        AnimatedSprite.Modulate = ModulateColor;
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
                NavigateTo(LeaderPosition, 64);
                break;
            case State.Navigating:
                Highlight.Hide();
                Label2.Hide();
                NavigateTo(DestinationPosition, 4);
                break;
        }
    }
}
