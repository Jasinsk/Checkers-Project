using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    public Piece[ , ] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public GameObject camera;
    public GameObject startScreen;
    public GameObject endScreen;
    public GameObject endScreenText;
    public GameObject cover;
    public AudioClip pieceMovementSound;
    public AudioClip movementErrorSound;


    public Vector3 offset = new Vector3(0, 0, 0);

    private Vector2 mousePosition;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private bool isWhiteTurn = true;
    private bool hasKilled = false;

    private Piece selectedPiece;
    private List<Piece> forcedPieces;

    private bool runningGame;
    private bool playerIsWhite;
    private AudioSource sound;
	
	struct Point
    {
		public int x_0;
		public int y_0;
        public int x;
        public int y;
		public bool isForcedToMove;
		public int movementQuality;
    }

    private void Start()
    {
        GenerateNewBoard();
        runningGame = false;

        camera.transform.position.Set(4, 8, 2);
        camera.transform.rotation.SetEulerAngles(78, 0, 0);

        sound = GetComponent<AudioSource>();
        sound.Play();
    }

    private void Update()
    {
        if (runningGame)
        {
            //Uncomment all this to start implementing enemy movement in EnemyTurn()
            if (playerIsWhite == isWhiteTurn)
            {
                MouseUpdate();

                int x = (int)mousePosition.x;
                int y = (int)mousePosition.y;

                if (selectedPiece != null)
                {
                    updatePieceMovement(selectedPiece);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    SelectPiece(x, y);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    endDrag = mousePosition;
                    TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                }
            }
            else
            {
				EnemyTurn();
            }
            

            CheckVictory();
        }
    }

    private void MouseUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mousePosition.x = (int)hit.point.x;
            mousePosition.y = (int)hit.point.z;
        }
        else
        {
            mousePosition.x = -1;
            mousePosition.y = -1;
        }
    }

    private void updatePieceMovement(Piece p)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    private void SelectPiece(int x, int y)
    {
        if(x < 0 || x >= 8 || y < 0 || y >= 8)
        {
            return;
        }
        else
        {
            Piece p = pieces[x, y];

            //This allows us to move only the pieces whose turn it is. It will have to be changed when the enemy will be implemented
            if (p != null && p.isWhite == isWhiteTurn)
            {
                selectedPiece = p;
                startDrag = mousePosition;
            }
			else{
			
			}
        }
    }

    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForcedMoves();

        //out of bounds
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            if (selectedPiece != null)
            {
                MovePiece(selectedPiece, x1, y1);
            }
            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            if (endDrag == startDrag && playerIsWhite)
            {
                MovePiece(selectedPiece, x1, y1);

                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            // Checking move validity
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                // If this was a kill
                if (Mathf.Abs(x1-x2) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p.gameObject);
                        hasKilled = true;
                    }
                }

                // You try not to kill on your turn
                if(forcedPieces.Count != 0 && !hasKilled)
                {
                    // Cue error sound
                    sound.clip = movementErrorSound;
                    sound.Play();
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                sound.clip = pieceMovementSound;
                sound.Play();
				
                MovePiece(selectedPiece, x2, y2);
				
                EndTurn(x2, y2);
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
    }

    private void EnemyTurn()
    {
		
		List<Point> possibleMoves = GetPossibleMoves();
		var id_list = new List<int>();
		int maxQ_ID = 0;
		
		int x = Random.Range(0,possibleMoves.Count);
		
		
		if(possibleMoves.Count != 0){
			int maxQ = possibleMoves[0].movementQuality;
			for(int i = 0; i<possibleMoves.Count; i++){
				if(possibleMoves[i].movementQuality>maxQ) {
					maxQ=possibleMoves[i].movementQuality;
					maxQ_ID =i;
				}
			}
			for(int i = 0; i<possibleMoves.Count; i++){
				if(possibleMoves[i].movementQuality==maxQ) id_list.Add(i);
			}
			maxQ_ID = id_list[Random.Range(0, id_list.Count)];
		}			
		
		if(possibleMoves.Count == 0) {
			CheckVictory();
		}else{
			selectedPiece = pieces[possibleMoves[maxQ_ID].x_0, possibleMoves[maxQ_ID].y_0];
			Thread.Sleep(500);
			TryMove(possibleMoves[maxQ_ID].x_0, possibleMoves[maxQ_ID].y_0, possibleMoves[maxQ_ID].x, possibleMoves[maxQ_ID].y);
			selectedPiece = null;		
		}

    }
	
	private List<Point> GetPossibleMoves()
    {
        List<Point> possiblePoints = new List<Point>();
		bool forcedToMoveIn = false;

		for(int x = 0; x<8; x++){
			for(int y = 0; y<8; y++){
				if (pieces[x,y] != null && pieces[x,y].isWhite!=playerIsWhite)
				{
					List<int> possibleX = new List<int> {x+2,x+1, x-1,x-2};
					List<int> possibleY = new List<int> {y-2, y-1, y+1, y+2};
					foreach (int xi in possibleX)
					{
						foreach (int yi in possibleY)
						{
							if(xi<0 || xi>7 || yi<0 || yi>7) continue;
							
							if (pieces[x, y].ValidMove(pieces, x, y, xi, yi))
							{
								Point p = new Point();
								p.x = xi;
								p.y = yi;
								p.x_0=x;
								p.y_0=y;
								p.movementQuality = Scoring(x, y, xi,yi);
								if(xi==x+2 || xi==x-2 || yi == y+2 || yi == y-2){
									p.isForcedToMove = true;
									forcedToMoveIn = true;
								}else{
									p.isForcedToMove = false;
								}
								 
								//Debug.Log("x0: " + p.x_0 + "    y0: " + p.y_0 + "    x: " + xi + "   y: " + yi + " " + p.isForcedToMove + " " + p.movementQuality);
								possiblePoints.Add(p);
							}
						}
					}
				}
			}
		}
		
		if(forcedToMoveIn){
			List<Point> onlyForcedToMove = new List<Point>();
			foreach(Point p in possiblePoints){
				if(p.isForcedToMove == true) onlyForcedToMove.Add(p);
			}
			return onlyForcedToMove;
		}
        return possiblePoints;
    }
	
	private int Scoring(int x_0, int y_0, int xi, int yi){
		int value_=0;
		
		for(int x = 0; x<8; x++){
			for(int y = 0; y<8; y++){
				if (pieces[x,y] != null && pieces[x,y].isWhite==playerIsWhite){
					if(Mathf.Abs(xi - x)==1 && Mathf.Abs(yi - y)==1){
						value_ +=1;
						
						if(
						( (x+2)==x_0 && (y+2)==y_0) ||
						( (x+2)==x_0 && (y-2)==y_0) ||
						( (x-2)==x_0 && (y+2)==y_0) ||
						( (x-2)==x_0 && (y-2)==y_0) ||
						pieces[x,y].IsForcedToMove(pieces, xi, yi)
						){
							value_-=3;
						}
					}					
				}
			}
		}
		return value_;
	}

    private void EndTurn(int x2, int y2)
    {
        int x = x2;
        int y = y2;

        // promote piece to king
        if (selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
        {
            selectedPiece.isKing = true;

            //Change piece prefab to king prefab
            selectedPiece.transform.Rotate(Vector3.right * 180);
        }

        if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
        {
            selectedPiece.isKing = true;

            //Change piece prefab to king prefab
            selectedPiece.transform.Rotate(Vector3.right * 180);
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        // allowing chain kills
        if (ScanForcedMoves(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        hasKilled = false;
    }

    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;

        for(int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }

        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);
    }

    private void Victory(bool isWhite)
    {
        if (isWhite)
        {
            endScreenText.GetComponent<Text>().text = "White wins!";
        }
        else
        {
            endScreenText.GetComponent<Text>().text = "Black wins!";
        }

        endScreen.SetActive(true);
        cover.SetActive(true);
        runningGame = false;
    }

    private List<Piece> ScanForcedMoves(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x, y].IsForcedToMove(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        }

        return forcedPieces;
    }
    private List<Piece> ScanForcedMoves()
    {
        forcedPieces = new List<Piece>();

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if(pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn)
                {
                    if(pieces[i,j].IsForcedToMove(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]);
                    }
                }
            }
        }

        return forcedPieces;
    }

    private void GenerateNewBoard()
    {

        // Generating White Team
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);

            for (int x = 0; x < 8; x += 2)
            {
                if (oddRow)
                {
                    GenerateNewPiece(x, y, whitePiecePrefab);
                }
                else
                {
                    GenerateNewPiece(x + 1, y, whitePiecePrefab);
                }
            }
        }

        // Generating Black Team
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);

            for (int x = 0; x < 8; x += 2)
            {
                if (oddRow)
                {
                    GenerateNewPiece(x, y, blackPiecePrefab);
                }
                else
                {
                    GenerateNewPiece(x + 1, y, blackPiecePrefab);
                }
            }
        }
    }

    private void GenerateNewPiece( int x, int y, GameObject piecePrefab)
    {
        GameObject go = Instantiate(piecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }

    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = transform.position + (Vector3.right * x) + (Vector3.forward * y) + offset;
		
    }

    public void StartWhite()
    {
        playerIsWhite = true;
        runningGame = true;
        startScreen.SetActive(false);
        cover.SetActive(false);
    }

    public void StartBlack()
    {
		
        playerIsWhite = false;
        runningGame = true;
		//isWhiteTurn=false;

        camera.transform.position = camera.transform.position + new Vector3(0, 0, 4);
        camera.transform.Rotate(24, 0, 180);

        startScreen.SetActive(false);
        cover.SetActive(false);
    }

    public void StartRandom()
    {
        if (Random.Range(0.0f, 1.0f) >= 0.5)
            StartWhite();
        else
            StartBlack();
    }

    public void ResetGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}
