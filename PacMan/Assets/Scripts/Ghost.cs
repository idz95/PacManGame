﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

	public float moveSpeed = 3.9f;

	public int pinkyReleaseTimer=5;
	public float ghostReleaseTimer = 0;

	public bool isInGhostHouse = false;

	public Node startingPosition;

	public int scatterModeTimer1=7;
	public int chaseModeTimer1 = 20;
	public int scatterModeTimer2=7;
	public int chaseModeTimer2 = 20;
	public int scatterModeTimer3=5;
	public int chaseModeTimer3 = 20;
	public int scatterModeTimer4=5;


	private int modeChangeIteration = 1;
	private float modeChangeTimer=0;

	public enum Mode
	{
		Chase,
		Scatter,
		Frightened
	}

	Mode currentMode=Mode.Scatter;
	Mode previousMode;

	public enum GhostType{
		Red, 
		Pink,
		Blue,
		Orange

	}

	public GhostType ghostType=GhostType.Red;

	private GameObject trump;

	private Node currentNode, targetNode, previousNode;

	private Vector2 direction, nextDirection;






	// Use this for initialization
	void Start () {

		trump = GameObject.FindGameObjectWithTag("trump");
		Node node = GetNodeAtPosition (transform.localPosition);

		if (node != null) {

			currentNode = node;
		}

		if (isInGhostHouse) {

			direction = Vector2.up;
			targetNode = currentNode.neighbors [0];

		} else {
			direction = Vector2.left;
			targetNode = ChooseNextNode ();
		}
			
		previousNode = currentNode;

	}
	
	// Update is called once per frame
	void Update () {

		ModeUpdate ();

		Move ();

		ReleaseGhosts ();
	}


	void Move(){

		if (targetNode != currentNode && targetNode != null && !isInGhostHouse) {

			if (OverShotTarget ()) {

				currentNode = targetNode;

				transform.localPosition = currentNode.transform.position;

				GameObject otherPortal = GetPortal (currentNode.transform.position);

				if (otherPortal != null) {

					transform.localPosition = otherPortal.transform.position;

					currentNode = otherPortal.GetComponent<Node> ();
				}

				targetNode = ChooseNextNode ();

				previousNode = currentNode;

				currentNode = null;

			} else {

				Debug.Log (direction);

				transform.localPosition += (Vector3)direction * moveSpeed * Time.deltaTime;
			}

		}



	}


	void ModeUpdate(){

		if (currentMode != Mode.Frightened) {

			modeChangeTimer += Time.deltaTime;

			if (modeChangeIteration == 1) {

				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1) {

					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;

				}
				if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1) {

					modeChangeIteration = 2;
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;

				}
			} else if (modeChangeIteration == 2) {

				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2) {

					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;

				}
				if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2) {

					modeChangeIteration = 3;
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;

				}

			} else if (modeChangeIteration == 3) {

				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3) {

					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;

				}
				if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3) {

					modeChangeIteration = 4;
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;

				}

			} else if (modeChangeIteration == 4) {

				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4) {

					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;

				}

			}
		} else if (currentMode == Mode.Frightened) {

		}

	}



	void ChangeMode(Mode m){

		currentMode = m;
	}


	Vector2 GetRedGhostTargetTile(){

		Vector2 trumpPosition = trump.transform.localPosition;
		Vector2 targetTile = new Vector2 (Mathf.RoundToInt (trumpPosition.x), Mathf.RoundToInt (trumpPosition.y));

		return targetTile;
	}

	Vector2 GetPinkGhostTargetTile(){

		Vector2 trumpPosition = trump.transform.localPosition;
		Vector2 trumpOrientation = trump.GetComponent<trump> ().orientation;

		int trumpPositionX = Mathf.RoundToInt (trumpPosition.x);
		int trumpPositionY = Mathf.RoundToInt (trumpPosition.y);

		Vector2 trumpTile = new Vector2 (trumpPositionX, trumpPositionY);
		Vector2 targetTile = trumpTile + (4 * trumpOrientation);

		return targetTile;

	}

	Vector2 GetTargetTile(){

		Vector2 targetTile = Vector2.zero;

		if (ghostType == GhostType.Red) {
			targetTile = GetRedGhostTargetTile ();
		} else if (ghostType == GhostType.Pink) {
			targetTile = GetPinkGhostTargetTile();
		}

		return targetTile;

	}

	void ReleasePinkGhost(){
		if (ghostType == GhostType.Pink && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}
	void ReleaseGhosts(){
		ghostReleaseTimer += Time.deltaTime;

		if (ghostReleaseTimer > pinkyReleaseTimer)
			ReleasePinkGhost ();
	}



	Node ChooseNextNode(){

		Vector2 targetTile = Vector2.zero;


		targetTile = GetTargetTile();

		Node moveToNode=null;

		Node[] foundNodes = new Node[4];
		Vector2[] foundNodesDirection = new Vector2[4];

		int nodeCounter = 0;

		for (int i = 0; i < currentNode.neighbors.Length; i++) {

			if (currentNode.validDirections [i] != direction * -1) {

				foundNodes [nodeCounter] = currentNode.neighbors [i];
				foundNodesDirection [nodeCounter] = currentNode.validDirections [i];
				nodeCounter++;
			}
		}

		if (foundNodes.Length == 1) {



			moveToNode = foundNodes [0];
			direction = foundNodesDirection [0];

		}


		if (foundNodes.Length > 1) {

			float leastDistance = 100000f;

			for (int i = 0; i < foundNodes.Length; i++) {

				if (foundNodesDirection [i] != Vector2.zero) {

					float distance = GetDistance (foundNodes [i].transform.position, targetTile);

					if (distance < leastDistance ) {

						leastDistance = distance;
						moveToNode = foundNodes [i];
						direction = foundNodesDirection [i];


					}
				}
			}

		}

		return moveToNode;

	}







	Node GetNodeAtPosition(Vector2 pos){
		
		GameObject tile = GameObject.Find ("Game").GetComponent<GameBoard> ().board [(int)pos.x, (int)pos.y];

		if (tile != null) {

			if (tile.GetComponent<Node>() != null) {

				return tile.GetComponent<Node> ();
			}
		}

		return null; 
	}


	GameObject GetPortal (Vector2 pos){


		GameObject tile = GameObject.Find ("Game").GetComponent<GameBoard> ().board [(int)pos.x, (int)pos.y];

		if (tile != null) {

			if (tile.GetComponent<teleport> ().isPortal) {

				GameObject otherPortal = tile.GetComponent<teleport> ().portalReceiver;
				return otherPortal;
			}
		}

		return null;

	}


	float LenghtFromNode(Vector2 targetPosition){

		Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
		return vec.sqrMagnitude;
	}

	bool OverShotTarget(){

		float nodeToTarget = LenghtFromNode (targetNode.transform.position);
		float nodeToSelf = LenghtFromNode (transform.localPosition);

		return nodeToSelf > nodeToTarget;

	}

	float GetDistance (Vector2 posA, Vector2 posB){

		float dx = posA.x - posB.x;
		float dy = posA.y - posB.y;

		float distance = Mathf.Sqrt (dx * dx + dy * dy);

		return distance;

	}







}
