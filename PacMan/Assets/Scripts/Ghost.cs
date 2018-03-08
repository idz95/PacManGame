﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

	public float moveSpeed = 5.9f;
	public float frightenedModeMoveSpeed = 2.9f;
	public float normalMovespeed=5.9f;

	public bool canMove = true;
	public bool active=false;
	public bool noActive = false;

	public int pinkyReleaseTimer=5;
	public int inkyReleaseTimer=14;
	public int clydeReleaseTimer = 21;
	public float ghostReleaseTimer = 0;


	public float disabledTimer=0;
	public float WaitForActive;

	public int frightenedModeDuration = 2;
	public int startBlinkingAt = 1;

	public bool isInGhostHouse = false;
	public static bool disabled=false;

	public Node startingPosition;
	public Node homeNode;

	public int scatterModeTimer1=7;
	public int chaseModeTimer1 = 20;
	public int scatterModeTimer2=7;
	public int chaseModeTimer2 = 20;
	public int scatterModeTimer3=5;
	public int chaseModeTimer3 = 20;
	public int scatterModeTimer4=5;

	public RuntimeAnimatorController ghostUp;
	public RuntimeAnimatorController ghostDown;
	public RuntimeAnimatorController ghostLeft;
	public RuntimeAnimatorController ghostRight;
	public RuntimeAnimatorController ghostWhite;
	public RuntimeAnimatorController ghostBlue;

	private int modeChangeIteration = 1;
	private float modeChangeTimer=0;

	private float frightenedModeTimer=0;
	private float blinkTimer = 0;

	private bool frightenedModeIsWhite = false;

	private float previousMoveSpeed;

	private AudioSource backgroundAudio;

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


		backgroundAudio = GameObject.Find ("Game").transform.GetComponent<AudioSource> ();

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

		UpdateAnimatorController ();

	}


	public void MoveToStartingPosition(){


		if (transform.name != "Ghost") {
			isInGhostHouse = true;
		}

		transform.position = startingPosition.transform.position;

		if (isInGhostHouse) {
			direction = Vector2.up;
		} else {
			direction = Vector2.left;
		}
		UpdateAnimatorController ();
	}

	public void Restart(){

		canMove = true;

		currentMode = Mode.Scatter;

		moveSpeed = normalMovespeed;

		previousMoveSpeed = 0;


		ghostReleaseTimer = 0;
		modeChangeIteration = 1;
		modeChangeTimer = 0;



		currentNode = startingPosition;

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

		if (canMove) {

			ModeUpdate ();

			Move ();

			ReleaseGhosts ();

			CheckCollision ();

		}
	}

	void CheckCollision(){
		
		Rect ghostRect = new Rect (transform.position, transform.GetComponent<SpriteRenderer> ().sprite.bounds.size / 4);
		Rect trumpRect = new Rect (trump.transform.position, trump.transform.GetComponent<SpriteRenderer> ().sprite.bounds.size / 4);

		if (ghostRect.Overlaps (trumpRect)) {
			
			if (currentMode == Mode.Frightened) {
				disabled = true;
				Consumed ();
			} else {



				GameObject.Find ("Game").transform.GetComponent<GameBoard> ().StartDeath ();
			}
		}
	}
		

	void Consumed(){


		if (GameMenu.isOnePlayerGame) {
			GameObject.Find ("Game").GetComponent<GameBoard> ().playerOneScore += 200;
		} else {
			if (GameObject.Find ("Game").GetComponent<GameBoard> ().isPlayerOneUp) {
				GameObject.Find ("Game").GetComponent<GameBoard> ().playerOneScore += 200;
			} else {
				GameObject.Find ("Game").GetComponent<GameBoard> ().playerTwoScore += 200;
			}
		}

		if (disabled == true) {
			
			DisableGameObject ();
		}

		GameObject.Find ("Game").transform.GetComponent<GameBoard> ().StartConsumed (this.GetComponent<Ghost> ());
	}

	public void DisableGameObject(){


		active = true;
		StartCoroutine (GhostDeath (2));

	}

	public void EnableGameObject(){


			gameObject.SetActive (true);

	}

	IEnumerator GhostDeath(float delay){
		gameObject.SetActive (false);
		yield return new WaitForSeconds (delay);
		gameObject.SetActive (true);
	}

	void UpdateAnimatorController(){

		if (currentMode != Mode.Frightened) {

			if (direction == Vector2.up) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostUp;
			} else if (direction == Vector2.down) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostDown;
			} else if (direction == Vector2.left) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostLeft;
			} else if (direction == Vector2.right) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostRight;
			}
		} else {

			transform.GetComponent<Animator> ().runtimeAnimatorController = ghostBlue;
		}
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

				UpdateAnimatorController ();

			} else {

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

			frightenedModeTimer += Time.deltaTime;

			if (frightenedModeTimer >= frightenedModeDuration) {

				backgroundAudio.clip = GameObject.Find ("Game").transform.GetComponent<GameBoard> ().backgroundAudioNormal;
				backgroundAudio.Play ();

				frightenedModeTimer = 0;
				ChangeMode (previousMode);
			}

			if (frightenedModeTimer >= startBlinkingAt) {

				blinkTimer += Time.deltaTime;

				if (blinkTimer >= 0.1f) {
					blinkTimer = 0f;

					if (frightenedModeIsWhite) {
						transform.GetComponent<Animator> ().runtimeAnimatorController = ghostBlue;
						frightenedModeIsWhite = false;
					} else {
						transform.GetComponent<Animator> ().runtimeAnimatorController = ghostWhite;
						frightenedModeIsWhite = true;
					}
				}
			}
		}

	}



	void ChangeMode(Mode m){

		if (currentMode == Mode.Frightened) {
			moveSpeed = previousMoveSpeed;
		}

		if (m==Mode.Frightened){
			previousMoveSpeed = moveSpeed;
			moveSpeed = frightenedModeMoveSpeed;
		}

		if (currentMode != m) {
			previousMode = currentMode;
			currentMode = m;
		}

		UpdateAnimatorController ();
	}


	public void StartFrightenedMode(){

		frightenedModeTimer = 0;
		backgroundAudio.clip = GameObject.Find ("Game").transform.GetComponent<GameBoard> ().backgroundAudioFrightened;
		backgroundAudio.Play ();
		ChangeMode (Mode.Frightened);
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

	Vector2 GetBlueGhostTargetTile(){

		//- Odabiremo dva tilea u blizini Trumpa
		Vector2 trumpPosition=trump.transform.localPosition;
		Vector2 trumpOrientatiom = trump.GetComponent<trump> ().orientation;

		int trumpPositionX = Mathf.RoundToInt (trumpPosition.x);
		int trumpPositionY = Mathf.RoundToInt (trumpPosition.y);

		Vector2 trumpTile = new Vector2 (trumpPositionX, trumpPositionY);

		Vector2 targetTile = trumpTile + (2 * trumpOrientatiom);

		Vector2 tempBlinkyPosition = GameObject.Find ("trump").transform.localPosition;

		int blinkyPositionX = Mathf.RoundToInt (tempBlinkyPosition.x);
		int blinkyPositionY = Mathf.RoundToInt (tempBlinkyPosition.y);

		tempBlinkyPosition = new Vector2 (blinkyPositionX, blinkyPositionY);

		float distance = GetDistance (tempBlinkyPosition, targetTile);
		distance *= 2;

		targetTile = new Vector2 (tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);

		return targetTile;


	}

	Vector2 GetOrangeGhostTargetTile(){

		Vector2 trumpPosition = trump.transform.localPosition;

		float distance = GetDistance (transform.localPosition, trumpPosition);
		Vector2 targetTile = Vector2.zero;

		if (distance > 8) {
			targetTile = new Vector2 (Mathf.RoundToInt (trumpPosition.x), (trumpPosition.y));
		} else if (distance < 8) {
			targetTile = homeNode.transform.position;
		}

		return targetTile;
	}

	Vector2 GetTargetTile(){

		Vector2 targetTile = Vector2.zero;

		if (ghostType == GhostType.Red) {
			targetTile = GetRedGhostTargetTile ();
		} else if (ghostType == GhostType.Pink) {
			targetTile = GetPinkGhostTargetTile ();
		} else if (ghostType == GhostType.Blue) {
			targetTile = GetBlueGhostTargetTile ();
		} else if (ghostType == GhostType.Orange) {
			targetTile = GetOrangeGhostTargetTile ();
		}

		return targetTile;

	}

	void ReleasePinkGhost(){
		if (ghostType == GhostType.Pink && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}

	void ReleaseBlueGhost(){
		if (ghostType == GhostType.Blue && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}

	void ReleaseOrangeGhost(){
		if (ghostType == GhostType.Orange && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}


	void ReleaseGhosts(){
		ghostReleaseTimer += Time.deltaTime;

		if (ghostReleaseTimer > pinkyReleaseTimer)
			ReleasePinkGhost ();
		if (ghostReleaseTimer > inkyReleaseTimer)
			ReleaseBlueGhost ();
		if (ghostReleaseTimer > clydeReleaseTimer)
			ReleaseOrangeGhost ();
	}



	Node ChooseNextNode(){

		Vector2 targetTile = Vector2.zero;

		if (currentMode == Mode.Chase) {
			targetTile = GetTargetTile ();
		} else if (currentMode == Mode.Scatter) {
			targetTile = homeNode.transform.position;
		}


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

