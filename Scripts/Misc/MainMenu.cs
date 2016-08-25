using UnityEngine;
using System.Collections;
/*This class takes care of the interaction with the main menu, and the things
 *related to it. It properly scales and fits UI-elements based on the current
 *screen-resolution, and allows the user to adjust music volume trough the menu.*/

public class MainMenu : MonoBehaviour {

	public Texture butttonTexture1;		//The button texture for the first button.
	public Texture butttonTexture2;		//The button texture for the second button.
	public float sliderValue = 1.0f;	//Current value/position of slider.
	public GUITexture background;		//Reference to backround image.
	public AudioSource menuMusic;		//Reference to the manu music.
	public bool timeTrialClicked = false;

    private int buttonHeight = 145;
    private int buttonWidth = 250;
    private bool titleScreen = true;
    private Rect slider;                //Contains the proportions for the slider.
    private float buttonScreenheight;   //The height at which the buttons are placed.
    private float labelHeight;			//Height of the "Music Volume"-label.

    // Use this for initialization
    void Start () 
	{
		DontDestroyOnLoad(transform.gameObject);
		buttonScreenheight = Screen.height/2-buttonHeight/2;
		labelHeight = Screen.height/1.5f - 20;
		slider = new Rect(Screen.width/2-40, Screen.height/1.5f, 80, 10);
		Vector4 center = new Vector4(Screen.width/2, Screen.height/2, background.texture.width/2, background.texture.height/2);
		background.pixelInset = new Rect(-Screen.width/2, -Screen.height/2, Screen.width, Screen.height);
	}

	void Update()
	{
		//If 'Esc' is clicked in-game, return to the main menu.
		if(titleScreen == false && Input.GetKeyDown(KeyCode.Escape))
		{
			Application.LoadLevel("MainMenu");
			Destroy(GameObject.Find("Main Menu"));
		}
	}

	void OnGUI() 
	{
		if (titleScreen == true)
		{
			//Set up a slider that adjusts the music volume.
			sliderValue = GUI.HorizontalSlider(slider, sliderValue, 0.0f, 1.0f);
			GUI.Label(new Rect(Screen.width/2 -41, Screen.height/1.5f - 20, 200, 20) , "Music Volume");
			menuMusic.volume = sliderValue;

			//Set up a menu-button for selecting the racing mode.
			if (GUI.Button(new Rect(Screen.width/4f-buttonWidth/2, buttonScreenheight, buttonWidth, buttonHeight), butttonTexture1))
			{
				Debug.Log("Clicked the button with an image");
				Application.LoadLevel("TheTrack");

				titleScreen = false;
			}
			
			//Set up a menu-button for selecting the time-trial mode.
			if (GUI.Button(new Rect(Screen.width/1.333333f-buttonWidth/2, buttonScreenheight, buttonWidth, buttonHeight), butttonTexture2))
			{
				Debug.Log("Clicked the button with text");
				timeTrialClicked = true;
				Application.LoadLevel("TheTrack");

				titleScreen = false;
			}

			//Set up a button for exiting the entire application.
			if (GUI.Button(new Rect(Screen.width/2 - buttonWidth/4, buttonScreenheight*1.92f, buttonWidth/2, buttonHeight/2), "Exit"))
			{
				Application.Quit();
			}
		}
	}

	//Hide the elements related to the title screen.
	private void HideTitle()
	{
		titleScreen = false;
	}
	
	public float SliderValue {
		get {
			return sliderValue;
		}
	}
}
