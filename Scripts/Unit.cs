using System;
using Godot;

public class Unit : Node2D
{
    [Export]
    private int Power = 1;

    private Label Label1;
    private Area2D Area2D;

    private Sprite Highlight;

    private State CurrentState = State.Default;
    enum State
    {
        Default,
        MouseHover,
        Selected
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Label1 = GetNode<Label>("Label1");
        Area2D = GetNode<Area2D>("Area2D");
        Highlight = GetNode<Sprite>("Highlight");

        Label1.Text = Power.ToString();
        Label1.AddColorOverride("font_color", new Color(0.1f, 0.2f, 0, 1));
        Label1.RectPosition = new Vector2(-100, -100);
        Label1.RectSize = new Vector2(200, 200);

        Highlight.Hide();

        Area2D.Connect("mouse_entered", this, nameof(OnMouseEntered));
        Area2D.Connect("mouse_exited", this, nameof(OnMouseExited));
        Area2D.Connect("input_event", this, nameof(OnInputEvent));
    }

    private void OnInputEvent(Node viewPort, InputEvent inputEvent, int shapeIdx)
    {
        if (inputEvent.IsPressed() && inputEvent is InputEventMouseButton)
        {
            if (CurrentState != State.Selected)
            {
                CurrentState = State.Selected;
            }
            else
            {
                CurrentState = State.Default;
            }
        }
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

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        switch (CurrentState)
        {
            case State.Default:
                Highlight.Hide();
                break;
            case State.MouseHover:
                Highlight.Show();
                break;
            case State.Selected:
                Highlight.Show();
                break;
        }
    }
}
