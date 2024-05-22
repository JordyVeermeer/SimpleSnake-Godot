using Godot;
using System;
using System.Collections.Generic;

public partial class Snake : Node2D
{
	private const float SEGMENT_SIZE = 16f;
	private const float MOVE_INTERVAL = 0.2f;

	private const float GRID_MAX_WIDTH = 496f;
	private const float GRID_MAX_HEIGHT = 248f;

	private Vector2 UP = new Vector2(0, -1);
	private Vector2 DOWN = new Vector2(0, 1);
	private Vector2 LEFT = new Vector2(-1, 0);
	private Vector2 RIGHT = new Vector2(1, 0);

	private Vector2 direction = Vector2.Right;
	private List<Sprite2D> Segments = new List<Sprite2D>();
	private float moveTimer = 0;
	private bool shouldGrow = false;

	private PackedScene appleScene;
	private Node2D apple;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < 3; i++)
		{
			AddSegment(new Vector2(-SEGMENT_SIZE * i, 0));
		}

		// load apple scene
		appleScene = (PackedScene)ResourceLoader.Load("res://Apple.tscn");
		SpawnApple();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		HandleInput();
		moveTimer += (float)delta;
		if (moveTimer >= MOVE_INTERVAL)
		{
			MoveSnake();
			moveTimer = 0;
		}
	}

	private void MoveSnake()
	{
		Vector2 headPos = Segments[0].Position;
		Vector2 newHeadPos = headPos + direction * SEGMENT_SIZE;

		// logic for checking boundaries

		if (newHeadPos.X < 0 || newHeadPos.Y < 0 || newHeadPos.X > GRID_MAX_WIDTH || newHeadPos.Y > GRID_MAX_HEIGHT)
		{
			if (direction == RIGHT)
			{
				newHeadPos.X = 0;
			}
			else if (direction == LEFT)
			{
				newHeadPos.X = 480;
			}
			else if (direction == UP)
			{
				newHeadPos.Y = 240;
			}
			else if (direction == DOWN)
			{
				newHeadPos.Y = 0;
			}

		}

		// check self collision
		foreach (var segment in Segments) 
		{
			if (segment.Position == newHeadPos)
			{
				GameOver();
				return;
			}
		}

		// check apple collision
		if (apple != null && apple.Position == newHeadPos)
		{
			shouldGrow = true;
			apple.QueueFree();
			SpawnApple();
		}

		Sprite2D newHead = CreateSegment(newHeadPos);
		Segments.Insert(0, newHead);
		AddChild(newHead);

		if (!shouldGrow)
		{
			// Remove last segment
			Sprite2D tail = Segments[Segments.Count - 1];
			Segments.RemoveAt(Segments.Count - 1);
			RemoveChild(tail);
			tail.QueueFree();
		}
		else
		{
			shouldGrow = false;
		}
	
	}

	private void HandleInput()
	{
		if (Input.IsActionPressed("ui_up") && direction != DOWN)
		{
			direction = UP;
			GD.Print("Up pressed");
		}
		else if (Input.IsActionPressed("ui_down") && direction != UP)
		{
			direction = DOWN;
			GD.Print("Down pressed");

		}
		else if (Input.IsActionPressed("ui_left") && direction != RIGHT)
		{
			direction = LEFT;
			GD.Print("Left pressed");

		}
		else if (Input.IsActionPressed("ui_right") && direction != LEFT)
		{
			direction = RIGHT;
			GD.Print("Right pressed");

		}
	}

	private void AddSegment(Vector2 pos)
	{
		Sprite2D segment = CreateSegment(pos);
		Segments.Add(segment);
		AddChild(segment);
	}

	private Sprite2D CreateSegment(Vector2 pos)
	{
		Sprite2D segment = new Sprite2D();
		segment.Position = pos;
		segment.Texture = (Texture2D)GD.Load("res://resources/redsquare.png");
		if (segment.Texture == null)
		{
			GD.PrintErr("Failed to load texture at: res://path_to_your_segment_texture.png");
		}
		return segment;
	}

	private void SpawnApple()
	{
		Vector2 applePos;
		bool isOnSnake;

		do
		{
			isOnSnake = false;
			applePos = new Vector2(GD.RandRange(0, 30) * SEGMENT_SIZE, GD.RandRange(0, 15) * SEGMENT_SIZE);

			// check if apple on snake
			foreach(var segment in Segments)
			{
				if (segment.Position == applePos)
				{
					isOnSnake = true;
					break;
				}
			}
		} while (isOnSnake);

		apple = (Node2D)appleScene.Instantiate();
		apple.Position = applePos;
		AddChild(apple);
	}

	private void GameOver()
	{
		GD.Print("The snake ate itself! Game over.");
	}
}
