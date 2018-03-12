using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour {

	private static int boardWidth = 28;
	private static int boardHeight=36;

	private bool didStartDeath=false;
	private bool didStartConsumed = false;

	public static int playerOneLevel=1;
	public static int playerTwoLevel=1;

	public int playerOneCoinsConsumed = 0;
	public int playerTwoCoinsConsumed = 0;

	public int totalCoins = 0;
	public int score = 0;
	public static int playerOneScore = 0;
	public static int playerTwoScore=0;
	public static int trumpLives = 3;

	public static bool isPlayerOneUp=true;
	public bool shouldBlink=false;

	public float blinkIntervalTime=0.1f;

	private float blinkIntervalTimer = 0;

	public AudioClip backgroundAudioNormal;
	public AudioClip backgroundAudioFrightened;
	public AudioClip backgroundAudioDead;
	public AudioClip consumedGhostAudioClip;

	public Sprite mapaPlava;
	public Sprite mapaBijela;

	public Text playerText;
	public Text readyText;

	public Text highscoretext;
	public Text playerOneUp;
	public Text playerTwoUp;
	public Text playerOneScoreText;
	public Text playerTwoScoreText;
	public Image playerLives2;
	public Image playerLives3;

	public Text consumedGhostScoretext;

	public GameObject[,] board = new GameObject[boardWidth,boardHeight];

	private bool didIncrementLevel = false;
	// Use this for initialization
	void Start () {

		Object[] objects = GameObject.FindObjectsOfType (typeof(GameObject));

		foreach (GameObject o in objects) {
		
			Vector2 pos = o.transform.position;

			if (o.name != "trump" && o.name != "Nodes" && o.name != "NonNodes" && o.name != "Mapa" && o.name != "Coin" && o.tag != "Ghost" && o.tag !="ghostHome"  && o.name!= "Canvas" && o.tag!="UIElements" ) {

				if (o.GetComponent<teleport>() != null) {

					if (o.GetComponent<teleport> ().isCoin || o.GetComponent<teleport> ().isExtraCoin) {

						totalCoins++;
					}
				}
			
				
				board [(int)pos.x, (int)pos.y] = o;
			} else {

				Debug.Log ("Found trump at: " + pos);
			
			}

		}

		if (isPlayerOneUp) {
			if (playerOneLevel == 1) {
				GetComponent<AudioSource> ().Play ();
			}
		} else {
			if (playerTwoLevel == 1) {
				GetComponent<AudioSource> ().Play ();
			}
		}

		StartGame ();
	}

	void Update(){

		UpdateUI ();

		CheckCoinsConsumed ();

		CheckShouldBlink ();
	}

	void UpdateUI(){

		playerOneScoreText.text = playerOneScore.ToString ();


		if (trumpLives == 3) {

			playerLives3.enabled = true;
			playerLives2.enabled = true;

		} else if (trumpLives == 2) {

			playerLives3.enabled = false;
			playerLives2.enabled = true;

		} else if (trumpLives == 1) {

			playerLives3.enabled = false;
			playerLives2.enabled = false;
		}

	}

	void CheckCoinsConsumed(){

		if (playerOneUp) {
			//player 1. playing
			if (totalCoins == playerOneCoinsConsumed) {
				PlayerWin (1);
			}

		} else {
			//player2 play


		}
	}

	void PlayerWin(int playerNum){

		if (playerNum==1) {
			if (!didIncrementLevel) {
				didIncrementLevel = true;
				playerOneLevel++;
			}
		} else {
			if (!didIncrementLevel) {
				didIncrementLevel = true;
				playerTwoLevel++;
			}
		}

		StartCoroutine (processWin (2));
	}

	IEnumerator processWin(float delay){

		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<trump> ().canMove = false;
		trump.transform.GetComponent<Animator> ().enabled = false;

		transform.GetComponent<AudioSource> ().Stop ();

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<Ghost> ().canMove = false;
			ghost.transform.GetComponent<Animator> ().enabled = false;
		}

		yield return new WaitForSeconds (delay);

		StartCoroutine (BlinkBoard (2));
	}

	IEnumerator BlinkBoard(float delay){

		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<SpriteRenderer> ().enabled = false;

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<SpriteRenderer> ().enabled = false;
		}
		//blink board
		shouldBlink = true;
		yield return new WaitForSeconds (2);

		shouldBlink = false;
		//restart game
		StartNextLevel();
	}

	private void StartNextLevel(){

		SceneManager.LoadScene ("Level1");
	}

	private void CheckShouldBlink(){

		if (shouldBlink) {

			if (blinkIntervalTimer < blinkIntervalTime) {

				blinkIntervalTimer += Time.deltaTime;
			} else {
				blinkIntervalTimer = 0;
				if (GameObject.Find ("Mapa").transform.GetComponent<SpriteRenderer> ().sprite == mapaPlava) {
					
					GameObject.Find("Mapa").transform.GetComponent<SpriteRenderer>().sprite=mapaBijela;
				} else {
					GameObject.Find ("Mapa").transform.GetComponent<SpriteRenderer> ().sprite = mapaPlava;
				}
				}
			}

	}

	public void StartGame(){



		if (isPlayerOneUp) {
			StartCoroutine (StartBlinking (playerOneUp));
		} else {
			StartCoroutine (StartBlinking (playerTwoUp));
		}

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<SpriteRenderer> ().enabled = false;
			ghost.transform.GetComponent<Ghost> ().canMove = false;
		}

		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<SpriteRenderer> ().enabled = false;
		trump.transform.GetComponent<trump> ().canMove = false;

		StartCoroutine (ShowObjectAfter (2.25f));
	}


	public void StartConsumed(Ghost consumedGhost){

		if (!didStartConsumed) {
			didStartConsumed = true;

			GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

			foreach (GameObject ghost in o) {

				ghost.transform.GetComponent<Ghost> ().canMove = false;
			}

			GameObject trump = GameObject.Find ("trump");
			trump.GetComponent<trump> ().canMove = false;

			//-Hide trump
			trump.transform.GetComponent<SpriteRenderer>().enabled=false;

			consumedGhost.transform.GetComponent<SpriteRenderer> ().enabled = false;

			transform.GetComponent<AudioSource> ().Stop ();

			Vector2 pos = consumedGhost.transform.position;

			Vector2 viewPortPoint = Camera.main.WorldToViewportPoint (pos);

			consumedGhostScoretext.GetComponent<RectTransform> ().anchorMin = viewPortPoint;
			consumedGhostScoretext.GetComponent<RectTransform> ().anchorMax = viewPortPoint;

			consumedGhostScoretext.GetComponent<Text> ().enabled = true;

			transform.GetComponent<AudioSource> ().PlayOneShot (consumedGhostAudioClip);

			StartCoroutine (ProcessConsumedAfter (0.75f, consumedGhost));
		}
	}


	IEnumerator StartBlinking(Text blinkText){
		yield return new WaitForSeconds(0.25f);

		blinkText.GetComponent<Text>().enabled=!blinkText.GetComponent<Text>().enabled;
		StartCoroutine(StartBlinking(blinkText));
	}




	IEnumerator ProcessConsumedAfter (float delay, Ghost consumedGhost){
		yield return new WaitForSeconds (delay);

		consumedGhostScoretext.GetComponent<Text> ().enabled = false;

		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<SpriteRenderer> ().enabled = true;

		consumedGhost.transform.GetComponent<SpriteRenderer> ().enabled = true;

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<Ghost> ().canMove = true;
		}

		trump.transform.GetComponent<trump> ().canMove = true;
		transform.GetComponent<AudioSource> ().Play ();

		didStartConsumed = false;

	}


	IEnumerator ShowObjectAfter(float delay){
		yield return new WaitForSeconds (delay);

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<SpriteRenderer> ().enabled = true;
		}

		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<SpriteRenderer> ().enabled = true;

		playerText.transform.GetComponent<Text> ().enabled = false;

		StartCoroutine (StartGameAfter (2));
	}

	IEnumerator StartGameAfter(float delay){

		yield return new WaitForSeconds (delay);

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<Ghost> ().canMove = true;
		}

		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<trump> ().canMove = true;

		readyText.transform.GetComponent<Text> ().enabled = false;


		transform.GetComponent<AudioSource> ().clip = backgroundAudioNormal;
		transform.GetComponent<AudioSource> ().Play ();
	}

	public void StartDeath(){

		if (!didStartDeath) {

			StopAllCoroutines ();

			if (GameMenu.isOnePlayerGame) {
				playerOneUp.GetComponent<Text> ().enabled = true;
			} else {
				playerOneUp.GetComponent<Text> ().enabled = true;
				playerTwoUp.GetComponent<Text> ().enabled = true;
			}

			didStartDeath = true;

			GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

			foreach (GameObject ghost in o) {
				ghost.transform.GetComponent<Ghost> ().canMove = false;
			}

			GameObject trump = GameObject.Find ("trump");
			trump.transform.GetComponent<trump> ().canMove = false;

			trump.transform.GetComponent<Animator> ().enabled = false;

			transform.GetComponent<AudioSource> ().Stop();

			StartCoroutine (ProcessDeathAfter (1));


		}
	}

	IEnumerator ProcessDeathAfter(float delay){

		yield return new WaitForSeconds (delay);

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {
			ghost.transform.GetComponent<SpriteRenderer> ().enabled = false;
		}

		StartCoroutine (ProcessDeathAnimator (1f));
	}

	IEnumerator ProcessDeathAnimator(float delay){

		GameObject trump = GameObject.Find ("trump");
		trump.transform.localScale = new Vector3 (1, 1, 1);
		trump.transform.localRotation = Quaternion.Euler (0, 0, 0);

		trump.transform.GetComponent<Animator> ().runtimeAnimatorController = trump.transform.GetComponent<trump> ().deathAnimation;
		trump.transform.GetComponent<Animator> ().enabled = true;

		transform.GetComponent<AudioSource> ().clip = backgroundAudioDead;
		transform.GetComponent<AudioSource> ().Play ();

		yield return new WaitForSeconds (delay);

		StartCoroutine (ProcessRestart (1));
	}



	IEnumerator ProcessRestart(float delay){

		trumpLives -= 1;


		if (trumpLives == 0) {

			playerText.transform.GetComponent<Text> ().enabled = true;

			readyText.transform.GetComponent<Text> ().text = "GAME OVER";
			readyText.transform.GetComponent<Text>().color=Color.red;

			readyText.transform.GetComponent<Text> ().enabled = true;

			GameObject trump = GameObject.Find ("trump");
			trump.transform.GetComponent<SpriteRenderer> ().enabled = false;
			transform.GetComponent<AudioSource> ().Stop ();

			StartCoroutine(ProcessGameOver(4));

		}
			else {

				playerText.transform.GetComponent<Text> ().enabled = true;
				readyText.transform.GetComponent<Text> ().enabled = true;

				GameObject trump = GameObject.Find ("trump");

				trump.transform.GetComponent<SpriteRenderer> ().enabled = false;

				transform.GetComponent<AudioSource> ().Stop ();

				yield return new WaitForSeconds (delay);

				StartCoroutine (ProcessRestartShowObjects (1));

		}

	}

	IEnumerator ProcessGameOver(float delay){
		yield return new WaitForSeconds (delay);

		SceneManager.LoadScene ("GameMenu");
	}


	IEnumerator ProcessRestartShowObjects(float delay){

		playerText.transform.GetComponent<Text> ().enabled = false;

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {

			ghost.transform.GetComponent<SpriteRenderer> ().enabled = true;
			ghost.transform.GetComponent<Ghost> ().MoveToStartingPosition ();
		}

		GameObject trump = GameObject.Find ("trump");

		trump.transform.GetComponent<Animator> ().enabled = false;
		trump.transform.GetComponent<SpriteRenderer> ().enabled = true;
		trump.transform.GetComponent<trump> ().MoveToStartingPosition ();

		yield return new WaitForSeconds (delay);
		Restart ();
	}


	public void Restart(){

		readyText.transform.GetComponent<Text> ().enabled = false;



		GameObject trump = GameObject.Find ("trump");
		trump.transform.GetComponent<trump> ().Restart ();

		GameObject[] o = GameObject.FindGameObjectsWithTag ("Ghost");

		foreach (GameObject ghost in o) {
			ghost.transform.GetComponent<Ghost> ().Restart ();
		}

		transform.GetComponent<AudioSource> ().clip = backgroundAudioNormal;

		transform.GetComponent<AudioSource> ().Play ();

		didStartDeath = false;
	}



}
