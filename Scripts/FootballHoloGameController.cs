using UnityEngine;
using System.Collections;
using System;

public class FootballHoloGameController : MonoBehaviour {
	public GameObject _ball;
	public GameObject objectMain;
	public GameObject[] _Targets;
	private bool _dontFake = false;
	private Rigidbody _football;
	public Vector2 _resetAngleLimits = new Vector2(10.0f, 170.0f), _timeLimit = new Vector2(0.5f, 2.0f);
	public float _resetRadius = 7.0f, _maxHeight = 2.5f, _flatFactor = 6.0f;
	public Vector3 _resetPosition;
	public float _scale = 0.5f, _resetDuration = 7.0f;
	public Vector2 _minGoal, _maxGoal;
	public bool _showTargets, _gameKicking, _fakeKicking;
	public bool _alignCamera = true;
	public bool _resetAndKickBall = false, _autoRepeat = true, _resetBallOnly = false, _movingReset = true;
    public GUIText Score;
    private int count;

	void Awake()
	{
		_Targets = GameObject.FindGameObjectsWithTag("Targets");
		//Console.WriteLine (_Targets [0]);
		/*foreach (GameObject found in _Targets)
		{
			objectMain = found.transform.parent.gameObject;
		}*/
		//objectMain.AddComponent<TargetHit> ();
		Debug.Log("Awake");
		for (int i = 0; i < _Targets.Length; i++) {
			_Targets[i].AddComponent<TargetHit>();
		}
	}
	// Use this for initialization
	void Start () {
		//         _Targets = GameObject.FindGameObjectsWithTag("Targets");
		//         _StrikerMeshRenderes = GameObject.FindGameObjectsWithTag("Striker");
		//         _GoalKeeperMeshRenders = GameObject.FindGameObjectsWithTag("GoalKeeper");
		_football = _ball.GetComponent<Rigidbody>();
		_resetPosition = _ball.transform.position;
        count = 0;
        updateScore();
	}

	void resetBall()
	{
		_dontFake = false;
		setBall(_resetPosition);
	}

	void setBall(Vector3 position)
	{
		_football.transform.position = position;
		_football.velocity = Vector3.zero;
		_football.angularVelocity = Vector3.zero;
		//if (_alignCamera) 
		//alignCamera (position);
	}

	// 	void alignCamera(Vector3 position) {
	// 		if (GameObject.Find ("Striker-skeleton") != null && GameObject.Find ("Skeleton") != null) {
	// 			Vector3 strikerPosition = GameObject.Find ("Striker-skeleton").transform.TransformPoint(GameObject.Find ("Skeleton").GetComponent<HNSkeleton>().getJoint(HNJOINT.HEAD));
	// 			//Camera currentCam = GameObject.Find("CameraController").GetComponent<CameraController>().currentCamera();
	// 			//currentCam.transform.rotation = Quaternion.Lerp (Quaternion.LookRotation ( position - strikerPosition), Quaternion.LookRotation (position - currentCam.transform.position), 0.05f);
	void kickBall()
	{
		Vector3 ballLocation = _football.transform.position;
		Vector2 point = angularSelector(new Vector2(0,359), new Vector2(0,0.8f));
		Vector3 targetLocation = gameObject.transform.TransformPoint (new Vector3 (point.x, point.y, 0));
		_football.velocity = estimateVelocity(ballLocation, targetLocation, UnityEngine.Random.Range(_timeLimit.x, _timeLimit.y), 2);
	}
		
	void resetAndKick()
	{
		resetBall();
		CancelInvoke("resetAndKick");
		CancelInvoke("kickBall");
		Invoke("kickBall", 2.0f);
		if (_autoRepeat)
		{
			Invoke("resetAndKick", _resetDuration);
		}
	}

	void resetBallOnly()
	{
		CancelInvoke("resetBallOnly");
		if (_movingReset)
			movingReset();
		else
			resetBall();
		if (_autoRepeat)
		{
			Invoke("resetBallOnly", _resetDuration);
		}
	}

	void moveBack()
	{
		_football.velocity = estimateVelocity(_football.transform.position, _resetPosition, UnityEngine.Random.Range(_timeLimit.x, _timeLimit.y), _flatFactor)*1.5f;
	}

	Vector2 angularSelector(Vector2 angleRangle, Vector2 radiusRange)
	{
		Vector2 point = Vector2.zero;
		float angle = UnityEngine.Random.Range(angleRangle.x, angleRangle.y);
		float radius = UnityEngine.Random.Range(radiusRange.x, radiusRange.y);
		point.x = radius * (float)Math.Sin(angle);
		point.y = radius * (float)Math.Cos(angle);
		return point;
	}

	void movingReset()
	{
		Vector3 startPosition = Vector3.zero;
		Vector2 point = angularSelector(_resetAngleLimits, new Vector2(_resetRadius, _resetRadius));
		startPosition.x = point.x;
		startPosition.y = UnityEngine.Random.Range(0.2f, _maxHeight);
		startPosition.z = -Math.Abs(point.y);
		setBall(startPosition + _resetPosition);
		CancelInvoke("moveBack");
		Invoke("moveBack", 2.0f);
	}

	/*void showTargets()
	{
		for (int i = 0; i < _Targets.Length; i++)
		{
			_Targets[i].gameObject.SetActive(_showTargets);
		}
	}*/


	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T) || _resetAndKickBall)
		{
			_resetAndKickBall = false;
			resetAndKick();
		}
		if (Input.GetKeyDown(KeyCode.R) || _resetBallOnly)
		{
			_resetBallOnly = false;
			resetBallOnly();
		}  
			
	}

	private Vector3 estimateVelocity(Vector3 initialLocation, Vector3 targetLocation, float time, float flatFactor)
	{
		return (targetLocation - initialLocation)/time - Physics.gravity*time/flatFactor;
	}

	int colori = 1;

	private void setFootballTargetFromPosition(Collision collision) {

		//_football.velocity = Vector3.zero;
		//_football.angularVelocity = Vector3.zero;
		foreach(ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.name.StartsWith("Bone")) {
				Debug.DrawRay(contact.point, contact.normal, new Color(0.5f/colori,1f/colori, 0.25f*colori), 5.0f);
				//				Vector3 velocity = contact.normal*1.0f;
				//				velocity.Scale(contact.otherCollider.GetComponent<Rigidbody>().velocity);
				//				_football.velocity += velocity;
				colori++;
			}

		}
		//		Vector3 targetLocation = new Vector3(UnityEngine.Random.Range(_minGoal.x, _maxGoal.x), UnityEngine.Random.Range(_minGoal.y, _maxGoal.y), gameObject.transform.position.z);
		//		_football.velocity = estimateVelocity(_football.transform.position, targetLocation, UnityEngine.Random.Range(_timeLimit.x, _timeLimit.y));
	}

	public void ballKicked(Collision collision) {
		if (_fakeKicking && !_dontFake) {
			_dontFake = true;
			Vector3 targetLocation = new Vector3(UnityEngine.Random.Range(_minGoal.x, _maxGoal.x), UnityEngine.Random.Range(_minGoal.y, _maxGoal.y), gameObject.transform.position.z);
			_football.velocity = estimateVelocity(_football.transform.position, targetLocation, UnityEngine.Random.Range(_timeLimit.x, _timeLimit.y), 2);
		}  
		if(_gameKicking) {
			setFootballTargetFromPosition(collision);
		}
	}
    public void addScore(int newcount)
    {
		Debug.Log("Add collision score" + newcount);
        count += newcount;
        updateScore();
    }
    public void updateScore()
    {
        Score.text = "Score: "+count;
    }
}

