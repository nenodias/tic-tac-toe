using Godot;
using System;
using System.Collections.Generic;

public partial class BoardGame : Node
{
	private Texture2D xTexture;
	private Texture2D oTexture;
	private Node2D positionsNode;
	private List<Sprite2D> positions = new();
	private TileMapLayer circleLayer;
	private TileMapLayer crossLayer;

	private int currentPosition = 0; // 1-9 for board positions, 0 for none
	private int playerTurn = 1;      // 1 for X, 2 for O
	private int[,] board = new int[3, 3]; // 0 empty, 1 X, 2 O
	private int moves = 0;
	private bool gameOver = false;

	private Label resultLabel;
	private Button restartButton;

	public override void _Ready()
	{
		xTexture = GD.Load<Texture2D>("res://assets/x.png");
		oTexture = GD.Load<Texture2D>("res://assets/o.png");
		positionsNode = GetNode<Node2D>("Posicoes");
		circleLayer = GetNode<TileMapLayer>("CircleLayer");
		crossLayer = GetNode<TileMapLayer>("CrossLayer");

		for (int i = 1; i <= 9; i++)
		{
			var pos = positionsNode.GetNode<Sprite2D>(i.ToString());
			pos.Texture = null;
			positions.Add(pos);

			var staticBody = pos.GetNode<StaticBody2D>("StaticBody2D");
			int idx = i;
			staticBody.MouseEntered += () => MouseHoverOption(idx);
			staticBody.MouseExited += () => { if (currentPosition == idx) MouseHoverOption(0); };
		}

		resultLabel = GetNode<Label>("ResultLabel");
		restartButton = GetNode<Button>("RestartButton");
		resultLabel.Text = "";
		restartButton.Visible = false;
		restartButton.Pressed += RestartGame;

		UpdateTurn();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsPressed() && currentPosition > 0 && !gameOver)
		{
			int row = (currentPosition - 1) / 3;
			int col = (currentPosition - 1) % 3;

			if (board[row, col] == 0)
			{
				board[row, col] = playerTurn;
				positions[currentPosition - 1].Texture = playerTurn == 1 ? xTexture : oTexture;
				playerTurn = 3 - playerTurn; // Switch between 1 and 2
				moves++;
				UpdateTurn();
				CheckGameOver();
			}
		}
	}

	public void MouseHoverOption(int option)
	{
		currentPosition = option;
	}

	private void UpdateTurn()
	{
		circleLayer.Visible = playerTurn == 2;
		crossLayer.Visible = playerTurn == 1;
	}

	private void CheckGameOver()
	{
		// Check rows and columns
		for (int i = 0; i < 3; i++)
		{
			if (board[i, 0] != 0 && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
				EndGame(board[i, 0]);
			if (board[0, i] != 0 && board[0, i] == board[1, i] && board[1, i] == board[2, i])
				EndGame(board[0, i]);
		}
		// Check diagonals
		if (board[1, 1] != 0 &&
			((board[0, 0] == board[1, 1] && board[2, 2] == board[1, 1]) ||
			 (board[0, 2] == board[1, 1] && board[2, 0] == board[1, 1])))
			EndGame(board[1, 1]);

		if (!gameOver && moves >= 9)
		{
			EndGame(0); // Draw
		}
	}

	private void EndGame(int winner)
	{
		gameOver = true;
		string message = winner == 0 ? "It's a draw!" : $"Winner: Player {winner}";
		GD.Print($"Game Over! {message}");
		resultLabel.Text = message;
		restartButton.Visible = true;
	}

	private void RestartGame()
	{
		// Reset board state
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
				board[i, j] = 0;
		foreach (var pos in positions)
			pos.Texture = null;
		playerTurn = 1;
		moves = 0;
		gameOver = false;
		currentPosition = 0;
		resultLabel.Text = "";
		restartButton.Visible = false;
		UpdateTurn();
	}
}
