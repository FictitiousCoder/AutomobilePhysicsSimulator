using UnityEngine;
using System.Collections;
/*The MusicHandler class is responsible for handling all script-dependent music.
 *The music changes depending on what mode the player chooses and what stare the race
 *is in if the player is racing against oponents. Proper intros and looping of
 *the pre-edited tracks are also taken care of here.*/

public class MusicHandler : MonoBehaviour {

	public AudioClip[] raceTracks = new AudioClip[2];			//Tracks for racing.
	public AudioClip[] raceFinishTracks = new AudioClip[3];		//Tracks when finishing a range (changes by rank).
	public AudioClip[] timeTrialTracks = new AudioClip[2];		//Track for the time-trial mode.
	public AudioClip finalLap;                                  //Music for the final race-lap.
    public MainMenu menu;                                       //Reference to the menu.
    private AudioClip[] music;									//Empty array, gets loaded depending on mode.
	private AudioSource audioPlayer;							//The source of the music in the game-world.
	private AudioSource counter;								//The source of the countdown-sound in the game-world.
	private bool race = true;
	private bool intro = false;									//True if the non-looped intro tracks are playing.
	private bool setUp = true;									//True until the music enters its default loop.
	private float timer = 0f;									//Timer for knowing when to change/toggle tracks.

	//Awake is called when the scripts becomes enables, and is called before Start().
	void Awake () 
	{
		//Initiliase some values and get references.
		AudioSource[] temp = GetComponents<AudioSource>();
		audioPlayer = temp[0];
		counter = temp[1];
		menu = GameObject.Find("Main Menu").GetComponent<MainMenu>();
		audioPlayer.volume = menu.sliderValue;
		race =! menu.timeTrialClicked;

		//Load the music-array depending on the chosen mode of play.
		if (race == true)
		{
			music = raceTracks;
		}
		else
		{
			music = timeTrialTracks;
		}
	}

	void Update () 
	{
		/*Run this if the music is in its set-up phase, building to the standard loop.
		 *Play the intro-track (for race, start and finish countdown first) for about the duration
		 *of time that the track lasts, then transition to the looped track and activate looping.*/
		if (setUp == true)
		{
			timer += Time.deltaTime;

			if (race == true)
			{
				if (timer >= 3.0f && intro == false)
				{
					audioPlayer.clip = music[0];
					audioPlayer.Play();
					intro = true;
				}
				else if (counter.isPlaying == false && timer >= 0.8f && timer < 1.2f)
				{
					counter.Play();
				}
				else if (timer >= music[0].length+2.9f)
				{
					audioPlayer.clip = music[1];
					audioPlayer.Play();
					audioPlayer.loop = true;
					setUp = false;
				}
			}
			
			if (race == false)
			{
				if (intro == false)
				{
					audioPlayer.clip = music[0];
					audioPlayer.Play();
					intro = true;
				}
				if (timer >= 13.25f)
				{
					audioPlayer.clip = music[1];
					audioPlayer.Play();
					audioPlayer.loop = true;
					setUp = false;
				}
			}
		}

	}

	//Called to play a sped up version of the race-track.
	public void FinalLapPlay()
	{
		audioPlayer.clip = finalLap;
		audioPlayer.Play();
	}
	
	/*Called to play a track when the race is finished. What track to play
	 *depends on how the player ranks in the race.*/
	public void RaceFinished(int rank)
	{
		if (rank == 1)
		{
			audioPlayer.clip = raceFinishTracks[0];
		}
		else if (rank == 4)
		{
			audioPlayer.clip = raceFinishTracks[2];
		}
		else
		{
			audioPlayer.clip = raceFinishTracks[1];
		}
		audioPlayer.Play();
	}

	public bool Race {
		get {
			return race;
		}
		set {
			race = value;
		}
	}
}
