using UnityEngine;
using System.Collections;
/*This class keeps track of the current time of the player's round around the track
 *and the record time, and feeds the information to the GUI-script.*/

public class GoalTrackerTime : MonoBehaviour {

    public float timer;         //Current lap-time.
    public bool racing;         //Keeps track of whether the player has begun racing.
    public GUICar gui;          //Reference to the GUI script.
    private float bestTime;		//Personal record-time for one lap.
	private int lapsFinished;   //Number of times the player has passed the finish line.

	void Start () 
	{
		timer = 0;
		bestTime = 0;
		lapsFinished = 0;
		racing = false;
	}
	
	//Increment the timer and update the UI once per frame.
	void Update () 
	{
		if (racing == true)
		{
			timer += Time.deltaTime;
			gui.TimeCurrent = timer;
		}
	}

	//If the player reaches the goal, compare their time to the record.
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "CarPlayer")
		{
			if (racing == true)
			{
				if (lapsFinished == 0)
				{
					bestTime = timer;
					gui.TimeBest = bestTime;
				}
				else if (timer < bestTime)
				{
					bestTime = timer;
					gui.TimeBest = bestTime;
				}
				lapsFinished++;
				timer = 0;
			}
			else
			{
				racing = true;
			}
		}
	}
}
