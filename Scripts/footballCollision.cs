using UnityEngine;
using System.Collections;
using System;

public class footballCollision : MonoBehaviour {
	public string nameStartsWith;
    public int countval;
	public Transform footballObject;
    private FootballHoloGameController footballHoloGameController;
    // Use this for initialization
    void Start () {
		//Debug.Log("Enter collision score" + countval);
		footballHoloGameController=footballObject.GetComponent<FootballHoloGameController>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log("collision gameobject name"+collision.gameObject.name);
        if (collision.gameObject.name != null && collision.gameObject.name.StartsWith(nameStartsWith)) {
			Debug.Log("Enter collision score"+countval);
			footballHoloGameController.updateScore();
            footballHoloGameController.addScore(countval);
            //GameObject.Find("BodyView").GetComponent<FootballHoloGameController>().ballKicked(collision);
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().GetComponent<FootballHoloGameController>().ballKicked(collision);
			//GameObject.FindWithTag("MainCamera").GetComponent<Camera>().ballKicked(collision);
      	 } 
	}

}
