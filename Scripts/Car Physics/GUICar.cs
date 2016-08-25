using UnityEngine;
using System.Collections;
/*This class handles the GUI-elements for the car-simulator. It takes care of
 *both race-aspects and time trial ones, as well as displaying the state of the car.*/

[RequireComponent(typeof(GUIText))]
public class GUICar : MonoBehaviour
{
	public Car car;                 	   	    // Reference to the car controller to get the information needed for the display.
	public string[] finishText = new string[4];
    public Transform goalText;                  //Reference to the goalText GameObject.
    public Transform timeText;                  //Reference to the timeText GameObject.
    public GameObject AIcompetetors;            //Reference to the computer-controller cars.

    private const float mpsToKmph = 3.6f;       // Constant for converting meters per second to kiloemters per hour.
    private string display =                    // Template string for GUI car-info.
        "{0:0} km/h \n" +
            "Gear: {1:0}/{2:0}\n" +
            "Revs {3:0}\n" +
            "Throttle: {4:0%}\n" +
            "Rev: {5:0}\n";
    private string raceFinishText =
            "You made {0:0} place!\n\n" +
            "Space to Restart/Esc for Menu";
    private string timeCurrent;					//Shows the time since the player last passed the goal.
	private string timeBest;					//Shows the player's time-record for one lap.
	private string time =
		"Current Time: {0:0}s\n" +
			"Best Time: {1:0}s\n";
	private int playerPosition;				    //Shows how the player placed in a race.
	private string currentLap;					//Shows the lap the player is currently on.

	private string laps = 
			"{1:0} / 4\n" +
			"{2:0} / 3";
	private bool timeTrial = false;				//Determines whether to shows the UI for race, or for time-trial.
	private bool countDown = false;
	private GUITexture darkOverlay;				//Texture to darken the screen.

		
	//These vectors stores some screen-positions for the text, based on the screen's resolution.
	Vector2 textPositionLeft = new Vector2(-Screen.width/1.02f, -20);
	Vector2 textPositionCenter = new Vector2(-Screen.width/2, -Screen.height/2);

	//Initialize some strings and check whether the player is racing agaisnt time or oponents.
	void Start()
	{
		GameObject menu = GameObject.Find("Main Menu");
		MainMenu menuScript = menu.GetComponent<MainMenu>();
		timeTrial =	menuScript.timeTrialClicked;
		finishText[0] = "You made 1st place!\nYOU WIN!";
		finishText[1] = "You made 2nd place!\nCongratulations!";
		finishText[2] = "You made 3rd place!\nCongratulations!";
		finishText[3] = "You made 4th place!\nBetter luck next time!";
		timeCurrent = "00.00";
		timeBest = "--.--";
		playerPosition = 0;
		currentLap = "1";
		timeText.GetComponent<GUIText>().pixelOffset = textPositionLeft;
		goalText.GetComponent<GUIText>().pixelOffset = textPositionCenter;
		darkOverlay = this.GetComponent<GUITexture>();
		darkOverlay.pixelInset = new Rect(-Screen.width/2, -Screen.height/2, Screen.width*2, Screen.height*2);

		if (timeTrial == false)
		{
			timeText.GetComponent<GUIText>().text = string.Format("Lap " + currentLap + " / 3");
		}
		else
		{
			AIcompetetors.SetActive(false);
		}
	}

	void LateUpdate()
	{
		//Set up the args. This is done for every string that requires text formatting.
		object[] args = new object[] { car.GetVx()*mpsToKmph, car.GearNumber, 6, Car.OmegaE, car.Throttle ,car.InReverse};
		
		//Display the car gui information
		GetComponent<GUIText>().text = string.Format(display, args);

		if (timeTrial == true)
		{
			object[] timeStat = new object[] {timeCurrent, timeBest};
			timeText.GetComponent<GUIText>().text = string.Format(time, timeStat);
		}
	}

	//Called at begining of final lap.
	public void FinalLapPrompt()
	{
		goalText.GetComponent<GUIText>().enabled = true;
		Destroy(goalText.gameObject, 3);
	}

	/*Called when the player finishes a race. The function changes the UI to
	 *display the results of the race.*/
	public void RaceFinished()
	{
		if (timeTrial == false)
		{
			darkOverlay.enabled = true;
			finishText[playerPosition-1]+= "\n\nSpace=Restart     Esc=Menu";
			timeText.GetComponent<GUIText>().text = string.Format(finishText[playerPosition-1], playerPosition);
			timeText.GetComponent<GUIText>().pixelOffset = textPositionCenter;
			timeText.GetComponent<GUIText>().fontSize = 80;
			timeText.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
			timeText.GetComponent<GUIText>().alignment = TextAlignment.Center;
		}
	}

	/*Declared some get-/set-functions. They convert their recieved values
	 *to strings, in order to use them in text-formatting and UI-elements.*/
	public float TimeCurrent{
		set {
			timeCurrent = value.ToString("f2");
		}
	}
	public float TimeBest{
		set {
			timeBest = value.ToString("f2");
		}
	}
	public int CurrentLap{
		set {
			if (timeTrial == false && value < 4)
			{
				currentLap = value.ToString();
				timeText.GetComponent<GUIText>().text = string.Format("Lap " + currentLap + " / 3");
			}
			else
			{
				currentLap = value.ToString();
			}
		}
	}
	public int PlayerPosition{
		set {
			playerPosition = value;
		}
	}

	//Allows classes to set a reference to the car in the physics-model.
	public Car Car {
		get {
			return car;
		}
		set {
			car = value;
		}
	}

	//Change between race and time-trial GUI.
	public bool TimeTrial {
		get {
			return timeTrial;
		}
		set {
			timeTrial = value;
		}
	}
}

