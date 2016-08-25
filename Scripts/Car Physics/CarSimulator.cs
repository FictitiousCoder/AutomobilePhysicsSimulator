using System;
using UnityEngine;
/*This class takes care of the actual simulation of the car, and manipulates the 3D-model
 *of the car trough displacement and rotation. It creates a car-object containing the
 *physics of the model, then uses the values it computes to manipulate the car, coupled with user-input.*/

public class CarSimulator : MonoBehaviour
{
	public SportsCar car;
    public bool onGround;               //This is set to true if the car has contact with the ground.
    public Transform wheelFrontLeft;    //Reference to the front left wheel.
    public Transform wheelFrontRight;   //Reference to the front right wheel.
    public Transform wheelRearLeft;     //Reference to the rear left wheel.
    public Transform wheelRearRight;    //Reference to the rear right wheel.
    public Transform wheelHubLeft;      //Reference to left wheel and its mudguard.
    public Transform wheelHubRight;     //Reference to right wheel and its mudguard.
    public CarAudioHandler audioScript; //Reference to the car's audio-script.
    public GUICar guiScript;            //Reference to the GUI-script.
    public BoxCollider collisionTrigger;//Reference to the car's crash-trigger/collider.

    public double throttleInput;        //Holds the throttle amount, which is applied to the physics.
    public double turnInput;			//Holds the steering-input. Used to alter the wheelAngle.

    /*Decare some starting values and the density of the air in which the car will be driving.
	 *Some of these are public in order to utilize Unity's feature to alter them dynamicly within the Unity-
	 *edior without having to alter the script every time.*/
    public double x0;
	public double y0;
	public double z0;
    private double vx0;
    private double vy0;
    private double vz0;
    private double time;
    private double density;
	private Vector3 triggerPosition;	//This vector holds the position of the car's crash-trigger/collider.

	private double previousX;
	private double previousZ;
	private double wheelAngle;			//Holds the current angle of the wheels.
	private double forwardVelocity;		//Keeps a reference to the car's x-movement for easy access.

  void Start() {

	/*Create a BoxsterS object with default values to initialize the car.
     *It sets the starting-coordinates based on the position of the 3D-model in the world.*/
    //x0 = transform.position.x;
	//y0 = transform.position.y;
	//z0 = transform.position.z;
    vx0 = 0.0;
    vy0 = 0.0;
    vz0 = 0.0;
    time = 0.0;
    density = 1.2;
	car = new SportsCar(x0, y0, z0, vx0, vy0, vz0, time, density);
	onGround = false;
	triggerPosition = new Vector3(collisionTrigger.center.x, collisionTrigger.center.y, collisionTrigger.center.z);
	
	throttleInput = 0;
	car.Throttle = 0;
	previousX = x0;
	previousZ = z0;
	forwardVelocity = 0;
	
	//Send out references to other scripts containing the newly created car-object.
	guiScript.Car = this.car;
	audioScript.Car = this.car;
  }
	 
  //Update handles everything that is not directly related to the physics.
  void Update()
	{
		//Get user input for use in setting the throttle-level.
		throttleInput = Input.GetAxis("Vertical");

		//Get input for use in turning the wheels.
		turnInput = Input.GetAxis("Horizontal");

		/*Change in or out of Reverse trough user-input.
		 *This also changes the location of the collision-trigger from the front
		 *to the back of the car and vice versa in order to properly prevent movement.*/
		if (Input.GetKeyDown(KeyCode.R))
		{
			car.setReverse();
			if (car.InReverse == true)
			{
				triggerPosition.z = -2.15f;
			}
			else
			{
				triggerPosition.z = 1.83f;
			}
			collisionTrigger.center = triggerPosition;
		}

		//Allow player to press space to quick-restart the level.
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Application.LoadLevel (Application.loadedLevelName);
		}

		/*Check whether or not the car is grounded. This is achieved by projecting a "ray"
		 *a short distance below the car. If the ray hits the ground, the car knows its within distance.*/
		Vector3 fwd = transform.TransformDirection(Vector3.down);
		RaycastHit rayHit = new RaycastHit();
		Ray groundRay = new Ray(transform.position+Vector3.up*0.6f, fwd);
		if (Physics.Raycast(groundRay, out rayHit, 0.7f) && rayHit.transform.tag == "Ground")
		{
			if (onGround == false)
			{
				onGround = true;
				car.OnGround = true;
			}

		}
		else
		{
			if (onGround == true)
			{
				onGround = false;
				car.OnGround = false;
			}

		}
			
		//Draws the ray. Purely for testing.
		Debug.DrawRay (transform.position+Vector3.up*0.6f, Vector3.down*0.7f, Color.blue);
	}

  /*FixedUpdate handles everything physics-related and it is
   *is called by once every 0.02 seconds.*/
  void FixedUpdate()
  {
	//Set throttle based on input.
	car.Throttle = throttleInput;

	// Update the car velocity and position at the next time increment. 
    double timeIncrement = 0.02;
    car.UpdateLocationAndVelocity(timeIncrement);

	//Compute the distance the car-model should be moved based on the physics.
	double distanceTraveledX = car.GetX() - previousX;
	double distanceTraveledZ = car.GetZ() - previousZ;
	
	//Store the car's physical velocity for ease of use.
	forwardVelocity = car.GetVx();

	//Move the car sideways, making it skid based on the car's laterfal force when going around a curve.
	if (forwardVelocity > 25)
	{
		GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.right*0.05f*(float)distanceTraveledZ*Math.Abs((float)wheelAngle));
	}
	

	//Move the car forward.
	if (distanceTraveledX > 1.0e-8 || distanceTraveledX < -1.0e-8)
	{
		GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.forward *(float)distanceTraveledX);
	}

	/*Store the current X- and Z-movement for comparison in the next loop.
	 *These are substracted from the new values in the next loop to compute the new location.*/
	previousX = car.GetX();
	previousZ = car.GetZ();
	
	//Check for whether the car is driving on a slope and the angle of the slope.
	double pitchAngle = transform.localEulerAngles.x;
	pitchAngle = (pitchAngle*Math.PI)/180;
	car.SlopeAngle = pitchAngle;

	//Take the user input and alter the angle of the wheels accordingly.
	wheelAngle = turnInput*20;
	float turnSpeed = (float)turnInput;

	
	//Restrict the wheels' angle.
	if (wheelAngle > 40)
	{
		wheelAngle = 40;
			if (turnSpeed > 0)
			{
				turnSpeed = 0;
			}
	}
	else if (wheelAngle < -40)
	{
		wheelAngle = -40;
			if (turnSpeed < 0)
			{
				turnSpeed = 0;
			}
	}
	car.WheelAngle = this.wheelAngle;

	//Rotate the wheels based on the car's speed.
	float wheelRotation = (float)forwardVelocity*1.3f;
	wheelFrontLeft.Rotate(wheelRotation, 0f, 0f, Space.Self);
	wheelFrontRight.Rotate(wheelRotation, 0f, 0f, Space.Self);
	wheelRearLeft.Rotate(wheelRotation, 0f, 0f, Space.Self);
	wheelRearRight.Rotate(wheelRotation, 0f, 0f, Space.Self);

	//Turn the wheel-models to match the computations.
	wheelHubLeft.localEulerAngles = new Vector3(wheelHubLeft.localEulerAngles.x, (float)wheelAngle, wheelHubLeft.localEulerAngles.z);
	wheelHubRight.localEulerAngles = new Vector3(wheelHubRight.localEulerAngles.x, (float)wheelAngle, wheelHubRight.localEulerAngles.z);	

	double steeringDelay = 1;
	
	//Make the wheels 'harder' to rotate with increasing velocities.
	if (car.GetVx() > 20)
	{
		steeringDelay = 20/(forwardVelocity*2);
	}
	else if (car.GetVx() > 10)
	{
		steeringDelay = 10/(forwardVelocity);
	}

	//Turn the car according to the car, if the wheels are grounded.
	if (onGround == true)
	{
		transform.Rotate(Vector3.up * (float)(wheelAngle*steeringDelay*0.8 * forwardVelocity)*0.01f);
	}
  }

	/*This function is called when an object enters the car's collision-trigger.
	 *The trigger is located at the very front or very back of the car, dependent on
	 *whether the car is in reverse or not. If the trigger detects an obstacle, the
	 *collision-function in the car-object is called, and the crash-noise plays.*/
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Obstacle")
		{
			car.Collision();

			if(forwardVelocity < 15.0 && forwardVelocity > -15)
			{
				audioScript.CrashNoise(0);
			}
			else
			{
				audioScript.CrashNoise(1);
			}
		}

	}

	//Called when the obstacle in the car's path exits its trigger.
	void OnTriggerExit(Collider other)
	{
		if (other.transform.tag == "Obstacle")
		{
			car.CollisionExit();
		}

	}
}
