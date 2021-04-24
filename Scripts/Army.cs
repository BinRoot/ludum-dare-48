using System;
using System.Collections.Generic;
using Godot;

public class Army : Node2D
{
    private List<Unit> Units = new List<Unit>();
    private List<Unit> SelectedUnits = new List<Unit>();

    private Label TotalCountLabel;

    [Export]
    private Boolean IsCPU = false;

    private int TotalPower = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TotalCountLabel = GetNode<Label>("TotalCountLabel");
        UpdateUnitPositions();
        Randomize();
    }

    public void Randomize()
    {
        UpdateUnitPositions();
        foreach (Unit unit in Units.ToArray())
        {
            unit.QueueFree();
        }
        Random random = new Random();
        var unitScene = GD.Load<PackedScene>("res://Scenes/Unit.tscn");
        for (int i = 0; i < random.Next(3, 6); i++)
        {
            var unitInstance = unitScene.Instance();
            ((Unit)unitInstance).Power = random.Next(1, 4);
            AddChild(unitInstance);
        }
        UpdateUnitPositions();
    }

    private void OnUnitSelected(Unit unit)
    {
        GD.Print(unit.Name, " selected");
        UpdateUnitPositions();
    }

    public void SetRandomSelection()
    {
        Random random = new Random();
        if (Units.Count > 0)
        {
            int idx1 = random.Next(0, Units.Count);
            if (!Units[idx1].IsResting())
            {
                Units[idx1].SetSelected();
            }
            int idx2 = random.Next(0, Units.Count);
            if (!Units[idx2].IsResting())
            {
                Units[idx2].SetSelected();
            }
        }
    }

    public List<Unit> GetSelectedUnits()
    {
        return new List<Unit>(SelectedUnits);
    }

    public void AddRestingUnits(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            AddChild(unit);
            unit.SetResting();
        }
    }

    public void RemoveSelectedUnits()
    {
        foreach (Unit unit in SelectedUnits.ToArray())
        {
            RemoveChild(unit);
        }
    }

    public int GetTotalPower()
    {
        return TotalPower;
    }

    public void UpdateUnitPositions()
    {
        int totalPowerCount = 0;
        Units.Clear();
        SelectedUnits.Clear();
        foreach (Node node in GetChildren())
        {
            if (node is Unit)
            {
                Units.Add((Unit)node);
            }
        }
        int xSpacing = 66;
        int xOffset = xSpacing * (Units.Count - 1) / 2;
        for (int unitIdx = 0; unitIdx < Units.Count; unitIdx++)
        {
            Unit unit = (Unit)Units[unitIdx];
            unit.Position = new Vector2(xSpacing * unitIdx - xOffset, 0);
            unit.SetIsCPU(IsCPU);
            if (!unit.IsConnected("UnitSelected", this, nameof(OnUnitSelected)))
            {
                unit.Connect("UnitSelected", this, nameof(OnUnitSelected));
            }
            if (unit.IsSelected())
            {
                SelectedUnits.Add(unit);
                totalPowerCount += unit.Power;
            }
        }
        TotalPower = totalPowerCount;
        TotalCountLabel.Text = totalPowerCount.ToString();
        TotalCountLabel.RectPosition = new Vector2(-100, -100 + 50 * (IsCPU ? 1 : -1));
        TotalCountLabel.RectSize = new Vector2(200, 200);
    }

    public void WakeUnits()
    {
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsResting())
            {
                unit.SetDefault();
            }
        }
    }

    public void DeselectAll()
    {
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsSelected())
            {
                unit.SetResting();
            }
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
