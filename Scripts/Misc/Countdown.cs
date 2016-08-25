using UnityEngine;
using System.Collections;
/*This class is for taking care of the countdown mechanic for the race-mode.
 *It disables both player- and AI-control and begins counting down. Once the
 *countdown finishes, all controls are re-activated, allowing the cars to drive.
 *As it is only used as the very begining, it disables itself once it's done.*/

public class Countdown : MonoBehaviour {
    private int countDown = 3;                      //The actual countdown-number.
    private float timeCounter = 1.0f;				//Reference value for the timer and counting.
	private float timer = 0.0f;	
	private MainMenu mainMenu;						//Reference to the menu script.
	private CarAIControl[] AIscripts = new CarAIControl[3]; //Array to hold the scripts of the cars.

    public GameObject AIcompetetors;                //Reference to the computer-controller cars.
    public CarSimulator playerScript;               //Reference to the player script.


    //Initialize the text-elements for the countdown and disable AI- and player-control.
    void Start () 
	{
		mainMenu = GameObject.Find("Main Menu").GetComponent<MainMenu>();

		/*If the player chose time-trial, disable the countdown. Else, disable
		 *the control scripts to keep the cars from driving until the countdown is done.*/
		if (mainMenu.timeTrialClicked == true)
		{
			this.enabled = false;
		}
		else
		{
			playerScript.enabled = false;
			CarAIControl[] AIscripts = AIcompetetors.GetComponentsInChildren<CarAIControl>();

			for (int counter = 0; counter < AIscripts.Length; counter++)
			{
				AIscripts[counter].enabled = false;
			}

			GetComponent<GUIText>().pixelOffset = new Vector2(-Screen.width/2, -Screen.height/2);
			GetComponent<GUIText>().text = string.Format(countDown.ToString());
		}


	}

	void Update () 
	{
		timer += Time.deltaTime;	//Increment the timer.
		
		if (timer >= timeCounter)
		{
			/*If the countdown is finishes, signal the player and let everyone start driving,
			 *else, keep counting down.*/
			if(countDown == 1)
			{
				GetComponent<GUIText>().text = string.Format("GO!");
				playerScript.enabled = true;
				CarAIControl[] AIscripts = AIcompetetors.GetComponentsInChildren<CarAIControl>();
				for (int counter = 0; counter < AIscripts.Length; counter++)
				{
					AIscripts[counter].enabled = true;
				}

				//Wait for a bit, then remove the "GO!" message and disable this script.
				if (timer > 6f)
				{
					GetComponent<GUIText>().text = string.Format("");
					this.enabled = false;
				}
			}
			else
			{
				countDown--;
				timeCounter++;
				GetComponent<GUIText>().text = string.Format(countDown.ToString());
			}

		}
	}


}
