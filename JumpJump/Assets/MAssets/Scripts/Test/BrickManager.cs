using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrickManager : MonoBehaviour
{
	
	//pool
	List<DynamicBrick> pool = new List<DynamicBrick> ();// demo use the list instead
	int initSize = 100;
	int addSize = 20;

	void InitPool (int size)
	{
		for (int i=0; i<size; i++) {
			GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
			go.transform.parent = this.transform;
			go.SetActiveRecursively (false);
			DynamicBrick db = new DynamicBrick (go);
			pool.Add (db);
		}
	}
	
	DynamicBrick ObtainDynamicBrick ()
	{
		if (pool.Count < 1) {
			InitPool (addSize);
		}
		
		if (pool.Count == 0) {
			Debug.Log ("The pool size null");
			return null;
		}
		
		DynamicBrick db = pool [0];
		pool.RemoveAt (0);
		
		db.Reset ();
		
		return db;
		
	}
	
	void ResetDynamicBrick (DynamicBrick db)
	{
		db.Reset ();
		pool.Add (db);
		
	}
	
	void Awake ()
	{
		InitPool (initSize);
	}
	
	

	// Use this for initialization
	void Start ()
	{		
		GenerateBlockBrick (Vector2.zero, 10, 3, new Vector3 (20, 20, 0), delayTime_Cell, false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		DynamicEffect ();
	}
	
	public static bool useContanstSpeed = true;
	float cellSizeX = 1f;
	float cellSizeY = 1f;
	Vector2 endPot = Vector3.zero;
	float delayTime_Cell = 0.01f;// 0.02f;
	
	void GenerateBlockBrick (Vector2 startPot, int xCount, int yCount, Vector3 spanPot, float delayTime_Cell, bool useConstantSpeed)
	{
		float xPot, yPot;
		int count = 0;
		int tmpCount = 0;
		float maxTime = ComputeMaxTime (spanPot);
		
		for (int i=0; i<xCount; i++) {
			xPot = startPot.x + i * cellSizeX;
			
			for (int j=0; j<yCount; j++) {
				yPot = startPot.y + j * cellSizeY;
//				if(spanPot.y>=0) 
//				else {
//					yPot= startPot.y + (yCount-j-1) * cellSizeY;
//				}
			
				count++;
				tmpCount = count;
				if (spanPot.y < 0)
					tmpCount = count - (yCount - j - 1);
				
				DynamicBrick db = ObtainDynamicBrick ();
				db.Comming = true;
				db.DelayTime = tmpCount * delayTime_Cell;
				db.LeastFrame = tmpCount;
				db.UseConstantSpeed = useConstantSpeed;
				db.MaxMoveTime = maxTime;
				
				db.TargetPot = new Vector3 (xPot, yPot, 0);
				db.InitPot = db.TargetPot + spanPot;
				db.Go.transform.localPosition = db.InitPot;
				db.Go.GetComponent<Renderer>().material.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
				db.Go.SetActiveRecursively (false);
				
				dynamicBricks.Add (db);
				
				
			}
		}
		
		endPot.x = startPot.x + xCount * cellSizeX;
		endPot.y = startPot.y + yCount * cellSizeY;
		
		useContanstSpeed = !useContanstSpeed;
		
	}
	
	List<DynamicBrick> dynamicBricks = new List<DynamicBrick> ();
	List<DynamicBrick> leaveDynamicBricks = new List<DynamicBrick> ();
	float curValidateX = 0;

	void DynamicEffect ()
	{	
		
		bool leave = player.transform.localPosition.x > curValidateX + 2f;
	
		
		for (int i=0; i<dynamicBricks.Count; i++) {
			DynamicBrick db = dynamicBricks [i];
			db.Update ();
			
//			if (player.transform.localPosition.x > db.Go.transform.localPosition.x) {
//				leaveCount++;
//				leaveDynamicBricks.Add (db);
//				SetLeave (db, leaveCount);
//				dynamicBricks.Remove (db);
//				i--;
//			}
		}
		
		UpdateLeaveDynamicBrick ();
		UpdateSimulatorPot ();
		leave = true;
		if (leave && dynamicBricks.Count < 20) {
			curValidateX = player.transform.localPosition.x;
			int x,y;
			int r=Random.Range (0, 10) ;
			if (r<3) {
				x = Random.Range (10, 15);
				y = Random.Range (-5, 5);
			} else if(r<6) {
				x = Random.Range (3, 8);
				y = Random.Range (-15, -10);
			}else{
				x = Random.Range (3, 8);
				y = Random.Range (10, 15);
			}
			GenerateBlockBrick (new Vector2 (endPot.x, 0), 3, 3, new Vector3 (x, y, x), delayTime_Cell * 5, false);
		}
		
	}
	
	void SetLeave (DynamicBrick db, int leaveCount)
	{
		db.Reset ();
		db.Comming = false;
		db.DelayTime = leaveCount * delayTime_Cell * 5f;
		db.LeastFrame = leaveCount;
		db.InitPot = db.Go.transform.localPosition;
		db.TargetPot = db.InitPot + leaveSpan;
		db.MaxMoveTime = ComputeMaxTime (leaveSpan);
	}
	
	void UpdateLeaveDynamicBrick ()
	{
		for (int i=0; i<leaveDynamicBricks.Count; i++) {
			DynamicBrick db = leaveDynamicBricks [i];
			db.Update ();
			if (db.Over) {
				db.Reset ();
				pool.Add (db);
				leaveDynamicBricks.Remove (db);
				i--;
			}
		}
	}
	
	float time = 0;
	
	void UpdateSimulatorPot ()
	{
		if ((time += Time.deltaTime) > 2f) {
			if (!start) {
				start = true;
				Init ();
			}
		}
		if (start) {
			v.y -= 10f * Time.deltaTime;
			Vector3 pot = player.transform.localPosition;
			pot += v * Time.deltaTime;
			if (pot.y < 3) {
				v.y = 6;
				int leaveCount = 0;
				DynamicBrick lastDB = null;
				for (int i=0; i<dynamicBricks.Count; i++) {
					DynamicBrick db = dynamicBricks [i];
					if (player.transform.localPosition.x > db.Go.transform.localPosition.x) {
						leaveCount++;
						leaveDynamicBricks.Add (db);
						SetLeave (db, leaveCount);
						dynamicBricks.Remove (db);
						i--;
						
						
					} 
					if (db.Go.transform.localPosition.x - 0.5f < pot.x &&
						pot.x < db.Go.transform.localPosition.x + 0.5f) {
						if (db.Go.transform.localPosition.y > 1.5f)
							lastDB = db;
			
					} else {
					
						if (lastDB != null) {
							player.GetComponent<Renderer>().material.color = lastDB.Go.GetComponent<Renderer>().material.color;
							lastDB = null;
						}
						
					}
					
				}
				
			}
			player.transform.localPosition = pot;
			
		}
	
	}
	
	public Vector3 leaveSpan = new Vector3 (0, -15, 0);
	public float brickMoveSpeed = 10f;
	public float brickMoveMaxTime = 1f;
	
	float ComputeMaxTime (Vector3 span)
	{
		float maxLenght = (Mathf.Abs (span.x) > Mathf.Abs (span.y)) ? Mathf.Abs (span.x) : Mathf.Abs (span.y);
		float time = maxLenght / brickMoveSpeed;
		if (time < brickMoveMaxTime)
			return time;
		return brickMoveMaxTime;
	}
	
	public ShpereTest player;
	bool start = false;
	Vector3 v = new Vector3 (2.5f, 6, 0);
	
	public void Init ()
	{
		player.transform.localPosition = new Vector3 (0, 3, 0);
	}
	
	
	
}

public class DynamicBrick
{
	GameObject go;

	public GameObject Go {
		get { return go;}
	}
	
	Vector3 initPot;

	public Vector3 InitPot {
		get { return initPot;}
		set { initPot = value;}
	}
	
	Vector3 targetPot;

	public Vector3 TargetPot {
		get { return targetPot;}
		set { targetPot = value;}
	}
	
	float time = 0;
	float delayTime = 0;

	public float DelayTime {
		get { return delayTime;}
		set { delayTime = value;}
	}

	bool comming;
	
	public bool Comming {
		get{ return comming;}
		set{ comming = value;}
	}
	
	bool over = false;
	
	public bool Over {
		get { return over;}
		set { over = value;}
	}
	
	float moveTime = 0;
	int frame = 0;
	int leastFrame = 0;

	public int LeastFrame {
		get { return leastFrame;}
		set { leastFrame = value;}
	}
	
	bool useConstantSpeed = true;

	public bool UseConstantSpeed {
		get { return useConstantSpeed;}
		set { useConstantSpeed = value;}
	}
	
	float maxMovetime = 0;

	public float MaxMoveTime {
		set { maxMovetime = value;}
	}
	
	public DynamicBrick (GameObject go)
	{
		this.go = go;
	}
	
	public void Update ()
	{
		if (over)
			return;

		if (comming) {
			if (time > delayTime && frame > leastFrame) {
				if (!go.active)
					go.SetActiveRecursively (true);
				
				if (useConstantSpeed)
					UpdatePot_ConstantSpeed ();
				else
					UpdatePot_VaryingSpeed ();
			}
		} else {
			if (time > delayTime && frame > leastFrame) {
			
				if (useConstantSpeed)
					UpdatePot_ConstantSpeed ();
				else
					UpdatePot_VaryingSpeed ();
				
				if (over && go.active)
					go.SetActiveRecursively (false);
			}
		}
		
		time += Time.deltaTime;
		frame++;
	}
	
	void UpdatePot_VaryingSpeed ()
	{
		Vector3 curPot = this.go.transform.localPosition;
		this.go.transform.localPosition = Vector3 .Lerp (curPot, targetPot, Time.deltaTime * 5);
		if ((curPot - targetPot).sqrMagnitude < 0.001f * 0.001f)
			over = true;

	}
	
	void UpdatePot_ConstantSpeed ()
	{
		if (moveTime < maxMovetime) {
			moveTime += Time.deltaTime * 0.8f;
		} else {
			moveTime = maxMovetime;
			over = true;
		}
		this.go.transform.localPosition = Vector3 .Lerp (initPot, targetPot, Mathf.Clamp (moveTime / maxMovetime, 0f, 1f));
	}
	
	public void Reset ()
	{
		time = 0;
		delayTime = 0;
		comming = true;
		over = false;
		moveTime = 0;
		frame = 0;
		leastFrame = 0;
		useConstantSpeed = false;
		maxMovetime = 1f;
		
	}
}
