﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
	public static GameManager Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<GameManager> ();
			}
			if (instance == null) {
				instance = new GameObject ().AddComponent<GameManager> ();
			}
			return instance;
		}
	}

	public static readonly Color[] COLORS = new Color[] {
		Color.gray,
		Color.red,
		Color.green,
		Color.blue,
		Color.magenta,
	};

	[SerializeField]
	private GameObject playerPrefab;
	private BoardManager board;
	private Image buttonOverlay;
	public IList<Vector2> PlayerStarts { get; set; }
	private int currentPlayer;
	private bool playerMoved;

	/// <summary>
	/// All the players in the game
	/// </summary>
	List<Player> players = new List<Player>();

	void Awake () {
		board = GameObject.FindObjectOfType<BoardManager> ();
		buttonOverlay = GameObject.Find ("ReadyTint").GetComponent<Image>();
	}

	void Start () {
		MakePlayer (Vector2.left);
		foreach (var start in PlayerStarts)
		{
			MakePlayer (start);
		}
		SetButtonColor (COLORS[currentPlayer + 1]);
		TestCase1 (); //!!
	}

	private Player MakePlayer(Vector2 start)
	{
		var playerObject = Instantiate(playerPrefab);
		var player = playerObject.GetComponent<Player>();
		player.Color = COLORS[players.Count];
		players.Add(player);

        // deal with prophets
        Prophet prophet = player.GetComponentInChildren<Prophet>();
        prophet.Color = player.Color;
        if (start.x >= 0)
        {
            var node = board.GetNodeAt(start);
            prophet.CurrentNode = node;
            player.Prophets.Add(prophet);
        }
        else
        {
            prophet.gameObject.SetActive(false);
        }

        return player;
	}

	void Update () {
		if (Input.GetMouseButtonUp (0)) {
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if (hit.transform) {
				Node node = hit.transform.GetComponent<Node> ();
				var canMove = !playerMoved && players [currentPlayer + 1].Prophets [0].MoveProphet (node);
				if (canMove) {
					playerMoved = true;
				}
			}
		}
	}

	public void OnReadyPressed()
	{
		currentPlayer = (currentPlayer + 1) % (players.Count - 1);
		playerMoved = false;
		SetButtonColor (COLORS[currentPlayer + 1]);
		// if we haven't cylced back to red's turn, don't compute
		if (currentPlayer != 0)
			return;
        foreach (var nodePair in board.nodes)
        {
            nodePair.Value.Evangelize();
        }
        foreach (var nodePair in board.nodes)
        {
            nodePair.Value.UpdateHealth();
		}

    }

	private void SetButtonColor(Color color)
	{
		buttonOverlay.color = color.WithAlpha(0.5f);
	}

	void OnDestroy() {
		instance = null;
	}

	public void NodeChangingInfluence(Node node) {
		// Check the Nodes current health - if 0 set it into the neutralList
		// If current health > 0, move from the neutralList into the Node.Player.Nodes list.
	}

	public void TestCase1()
	{
		board.GetNodeAt (new Vector2(0, 0)).TestInteraction(players[1], 10);
		board.GetNodeAt (new Vector2(9, 9)).TestInteraction(players[2], 10);
		board.GetNodeAt (new Vector2(0, 9)).TestInteraction(players[3], 10);
		board.GetNodeAt (new Vector2(9, 0)).TestInteraction(players[4], 10);
	}
}

static class Extensions
{
	public static Color WithAlpha(this Color color, float alpha) {
					return new Color (color.r, color.g, color.b, alpha);
	}
}