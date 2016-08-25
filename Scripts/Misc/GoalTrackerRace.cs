using UnityEngine;
using System.Collections;
/*This class keeps track of the racers as they pass the goal, and sends
 *information to the GUI-script.*/

public class GoalTrackerRace : MonoBehaviour {

    public bool[] racing = new bool[4];         //Keeps track of whether the player has begun racing.
    public MainMenu menu;                       //Reference to the menu.
    public GUICar gui;                          //Reference to the GUI script.
    public MusicHandler musicPlayer;            //Reference to the music-script.

    private int[] lapsFinished = new int[4];	//Number of laps the cars have finished.
	private int[] ranking = new int[4];			//Keeps track of the drivers' final placing.
	private int lapsToFinish = 4;               //Laps needed to finish the race minues one lap, as it starts at one.

	//On loading the track, check to see if the player chose racing or not.
	void Awake()
	{
		menu = GameObject.Find("Main Menu").GetComponent<MainMenu>();

		if (menu.timeTrialClicked == true)
		{
			Destroy(this.gameObject.GetComponent<GoalTrackerRace>());
		}
	}

	//Give a car its ranking once they finish the race.
	void CheckRanking(int car)
	{
		int rankCount = 0;
		for(int counter = 0; counter < lapsFinished.Length; counter++)
		{
			if (lapsFinished[counter] >= lapsToFinish)
			{
				rankCount++;
			}
			ranking[car] = rankCount;
		}
	}

	/*Check whether any of the cars passes the goal.
	 *If a car passes, increase the laps they've finished.
	 *If they've finished their third lap, give them their ranking and
	 *finish the race once the player finishes their third lap.*/
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "CarPlayer")
		{
			if (racing[0] == true)
			{
				lapsFinished[0]++;
				gui.CurrentLap = lapsFinished[0];


				if (lapsFinished[0] == lapsToFinish)
				{
					CheckRanking(0);
					gui.PlayerPosition = ranking[0];
					gui.RaceFinished();
					musicPlayer.RaceFinished(ranking[0]);
				}
				else if (lapsFinished[0] == (lapsToFinish-1))
				{
					musicPlayer.FinalLapPlay();
					gui.FinalLapPrompt();
				}
			}
			else
			{
				racing[0] = true;
				lapsFinished[0] = 1;
			}
		}
		else if (other.transform.tag == "Computer1")
		{
			if (racing[1] == true)
			{
				lapsFinished[1]++;
				if (lapsFinished[1] == lapsToFinish)
				{
					CheckRanking(1);
				}
			}
			else
			{
				racing[1] = true;
				lapsFinished[1] = 1;
			}
		}
		else if (other.transform.tag == "Computer2")
		{
			if (racing[2] == true)
			{
				lapsFinished[2]++;
				if (lapsFinished[2] == lapsToFinish)
				{
					CheckRanking(2);
				}
			}
			else
			{
				racing[2] = true;
				lapsFinished[2] = 1;
			}
		}
		else if (other.transform.tag == "Computer3")
		{
			if (racing[3] == true)
			{
				lapsFinished[3]++;
				if (lapsFinished[3] == lapsToFinish)
				{
					CheckRanking(3);
				}
			}
			else
			{
				racing[3] = true;
				lapsFinished[3] = 1;
			}
		}
	}
}
