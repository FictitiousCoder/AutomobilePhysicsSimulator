using System;
/*The Car-class contains the meat of the physics-part of the simulation. One object of
 *this class is made to represent a single car in the world. The CarSimulator in turn creates
 *a car-object and uses it to simulate the car in 3D-space. The car takes care of the actual
 *physics-related computations, while the CarSimulator puts those values to use in the visuals.
 *The simulator also alters some values for the object, such as the angle of the wheels, the angle
 *of the slope the car is driving at, the throttle, as well as altering the speed directly in
 *special circumstances such as a crash.*/

public class Car : DragProjectile
{
	private double muR;				//Coefficient of rolling friction.
	private double muK;				//Coefficient of friction between tires and road.	
	private double omegaE;			//Engine rpm.
	private double redline;			//The redline rpm value of the car.
	private double finalDriveRatio;
	private double wheelRadius;
	private int gearNumber;     	//The gear the car is currently in.
	private int numberOfGears;  	//Total number of gears.
	private double[] gearRatio;  	//Gear ratios
	private double wheelbase;		

	private double FtMax;			//Maximum frictional force(that can be applied without sliding)
	private double centripetal;		//Centripetal force on car.
	private double throttle;		//The amount of applied throttle.
	private double wheelAngle;		//The angle of the wheels.
	private double slopeAngle;		//The angle of the terrain the car is driving on.
	private bool isColliding;		//Keeps track of whetehr the front of the car is colliding with something.
	private bool collisionCooldown; //Activates once per collision. Used to perform changes on collision once.
	private bool inReverse;			//This is true if the car is in reverse, false if it is in gear.
	private bool onGround;			//Keep track of whether the wheels have contact with the ground.

    /*The Car constructor calls the 
     *DragProjectile constructor and then initializes
     *the car-specific variables.*/
	public Car(double x, double y, double z, double vx, 
             double vy, double vz, double time, double mass, 
             double area, double density, double Cd, double redline,
             double finalDriveRatio, double wheelRadius, double wheelbase,
             int numberOfGears) : base(x, y, z, 
             vx, vy, vz, time, mass, area, density, Cd)
	{
		//  Initialize some fields based on values passed
		//  to the constructor.
		this.redline = redline;           		 //  redline rpm
		this.finalDriveRatio = finalDriveRatio;  //  final drive ratio
		this.wheelRadius = wheelRadius;  		 //  wheel radius
		this.numberOfGears = numberOfGears;  	 //  number of gears
		this.wheelbase = wheelbase;

		//  Initialize the array that stores the gear ratios.
		//  The array is shifted so the first index in the 
		//  array correpsonds to first gear and so on.
		//  Give all gear ratios the dummy value of 1.0
		gearRatio = new double[numberOfGears + 1];
		gearRatio[0] = 0.0; 
		for(int i=1; i<numberOfGears+1; ++i) 
		{
			gearRatio[i] = 1.0;
		}

		//Initialize some values and set some coefficients.
		muR = 0.015;             
		muK = 0.7;				 //The coefficient of friction between dry asphalt and the car tires.
		omegaE = 0.0;      
		gearNumber = 1;         
		throttle = 0.0;
		wheelAngle = 0.0;
		slopeAngle = 0.0;
		isColliding = false;
		collisionCooldown = false;
		inReverse = false;
		onGround = true;

		//Compute maximum frictional force.
		FtMax = muK * mass * (-G);	 //If torque exceeds maximum frictional force, the tires will spin.
  	}

  



  //  The GetRightHandSide() method returns the right-hand
  //  sides of the six first-order projectile ODEs
  //  q[0] = vx = dxdt
  //  q[1] = x
  //  q[2] = vy = dydt
  //  q[3] = y
  //  q[4] = vz = dzdt
  //  q[5] = z
  public override double[] GetRightHandSide(double s, double[] q, 
                              double[] deltaQ, double ds,
                              double qScale) {
    double[] dQ = new double[6];
    double[] newQ = new double[6];

    //  Compute the intermediate values of the 
    //  dependent variables.
    for(int i=0; i<6; ++i) {
      newQ[i] = q[i] + qScale*deltaQ[i];
    }

    //  Compute the constants that define the
    //  torque curve line.
    double b, d;
    if ( this.OmegaE <= 1000.0 ) {
      b = 0.0;
      d = 220.0;
    }
    else if ( this.OmegaE < 4400.0 ) {
      b = 0.025;
      d = 195.0; 
    }
    else {
      b = -0.032;
      d = 457.2;
    }

    //  Declare some convenience variables representing
    //  the intermediate values of velocity.
    double vx = newQ[0];
    double vy = newQ[2];
    double vz = newQ[4];
    double v = Math.Sqrt(vx*vx + vy*vy + vz*vz) + 1.0e-8;

    //  Compute the total drag force.
    double density = this.Density;
    double area = this.Area;
    double cd = this.Cd;
	double Fd = 0.5*density*area*cd*v*v;

	double Te = b*omegaE + d; 											//Engine Torque
	double Ft = (Te*gearRatio[gearNumber]*finalDriveRatio)/wheelRadius;	//Force applied to the wheels (Engine Torque Force)

	//The force of gravity paralel to the slope the car might be standing on.
	//This force is positive if traveling upwards, and negative when going downhill.
	double Fgp = Mass*G*Math.Sin(slopeAngle);	

    //  Compute the force of rolling friction. Because
    //  the G constant defined in the SimpleProjectile
    //  class has a negative sign, the value computed here
    //  will be negative.
    double mass = this.Mass;
    double Fr = muR*mass*G;
	
	//  Compute the new engine rpm value
	double rpm = vx*60.0*GetGearRatio()*finalDriveRatio/(2.0*Math.PI*wheelRadius);
	OmegaE = rpm;


	//Compute the lateral force for turning
	double Flateral = 0;

		if (wheelAngle >= 0)
		{
			double turnRadius = wheelbase/(Math.Sin((wheelAngle*Math.PI)/180));
			Flateral = ((mass*vx*vx)/turnRadius) - muK*mass*(-G);
			Flateral = ds*(Flateral/mass);
			if (Flateral < 0)
			{
				Flateral = 0;
			}
			newQ[4] = -Flateral;
		}

		else if (wheelAngle < 0)
		{
			double turnRadius = wheelbase/(Math.Sin((wheelAngle*Math.PI)/180));
			Flateral = ((mass*vx*vx)/turnRadius) - muK*mass*(-G);
			Flateral = ds*(Flateral/mass);
			if (Flateral > 0)
			{
				Flateral = 0;
			}
			newQ[4] = -Flateral;
		}

	
	//Check if the car is driving on a slope, and if so then compute the values
	//related to it for use in the c3 constant in the equation for the acceleration.
	double c4 = 0;			//The angle-component of the c3 constant.

	if (slopeAngle < -1 || slopeAngle > 1)
		{
			c4 = muR*G*Math.Cos(slopeAngle) + G*Math.Sin(slopeAngle);
		}

	//Check whether something is blocking the car's path, if not then allow it to drive.
	//Also make sure the car is touching the ground.
	if (isColliding == false && onGround == true)
	{
		//If the car is not in reverse, allow for forward acceleration, and compute the acceleration
		//based on the car's current values and the gear it is in. The throttle factor comes from user input.
		if (inReverse == false)
		{
			if (throttle > 0) 
			{
				double c1 = -Fd/mass;
				double tmp = GetGearRatio()*finalDriveRatio/wheelRadius;
				double c2 = 60.0*tmp*tmp*b*v/(2.0*Math.PI*mass);
				double c3 = (tmp*d + Fr)/mass;
				dQ[0] = ds*((c1 + c2 + c3 + c4)*throttle);
				
				//Limit acceleration based on the angle the car is turning at.
				if ( vx > 5 && (wheelAngle > 5 || wheelAngle < -5) )
				{
					dQ[0] *= 5/(wheelAngle + vx);
					//dQ[0] *= 5/(wheelAngle +50);
				}
				
				//Restrict forward-acceleration to positive values when not in reverse.
				if (dQ[0] < 0)
				{
					dQ[0] = 0;
				}
			}
			//If the throttle is negative, register its value as braking.
			else if (throttle < 0) 
			{
				// Only brake if the velocity is positive.
				if ( newQ[0] > 0.1 )
				{
					dQ[0] = ds*(20.0*throttle);
				}
				else 
				{
					dQ[0] = 0.0;
				}
			}
		}
		//If the car is in reverse, allow the car to back up by using the throttle.
		else
		{
			if (throttle < 0 && vx > -15)
			{
				dQ[0] = ds* (3.0*throttle);// + (10*(Fr-Fd)-Fgp*3)/(mass));
			}
			else if (throttle > 0 && vx < 0)
			{
				dQ[0] = ds* (3.0*throttle);
			}
		}
	}

	//If the neither throttle or break is applied, base acceleration on drag and friction.
	//If car is in gear the tires can not roll backwards and vice versa.
	if (throttle == 0) 
	{
		if (inReverse == false)
		{
			if (GetQ(0) > 0)
			{
				dQ[0] = (ds*(10*(Fr-Fd)-Fgp*3)/(mass));
			}
			else
			{
				dQ[0] = 0;
				this.SetQ(0.0, 0);
			}
		}
		else if (inReverse == true) //&& GetQ(0 > -15
		{
			if (GetQ(0) < 0)
			{
				dQ[0] = (-ds*(10*(Fr-Fd+Fgp))/(mass));
			}
			else
			{
				dQ[0] = 0;
				this.SetQ(0.0, 0);
			}
		}
    }

	/*The rpmFactor is used to decide when to shift gear. It is based on
	 *the car's rpm, but also considers speed, acceleration, and driving on slopes.*/
	double rpmFactor ; 
	
	if (rpm == 0)
	{
		rpmFactor = 0;
	}
	else
	{
		rpmFactor = rpm + vx*1.5 + dQ[0]*3000 - Fgp*0.35;
	}

	//If the rpmFactor exceeds or drops below the values, shift gears accordingly.
	if ( rpmFactor > 6000) 
	{
		ShiftGear(1);
	}
	else if(rpmFactor < 3500)
	{
		ShiftGear(-1);
	}
	
	//If the car is backing up, make sure the rpm stays positive for other purposes than gear shifting.
	//As far as gear shifting goes, the car should not automaticly shift while in reverse.
	if(omegaE < 0)
		{
			omegaE = -omegaE;
		}

	/*Finally the array is loaded for the ODE-solver, the speed and acceleration for the x-cordinate 
	 *is computed mainly from the car's forward acceleration, while the lateral force is used for the
	 *y-cordinate. Y is not used here, as it interferes with the visual engine, so gravity si handled seperately.*/
    dQ[1] = ds*newQ[0];
	dQ[2] = 0.0;
	dQ[3] = 0.0;
    dQ[4] = newQ[4]*0.01; 
	dQ[5] = ds*newQ[4];
		
    return dQ;
  }



	//  This method simulates a gear shift
	public void ShiftGear(int shift) {
		//  If the car will shift beyond highest gear, return.
		if ( shift + this.GearNumber > this.NumberOfGears ) {
			return;
		}
		//  If the car will shift below 1st gear, return.
		else if ( shift + this.GearNumber  < 1 ) {
			return;
		}
		//  Otherwise, change the gear and recompute
		//  the engine rpm value.
		else {
			double oldGearRatio = GetGearRatio();
			this.GearNumber = this.GearNumber + shift;
			double newGearRatio = GetGearRatio();
			this.OmegaE = this.OmegaE*newGearRatio/oldGearRatio;
		}
		
		return;
	}

	//This function is to be called when the Unity engine detects a front-collision.
	public void Collision()
	{
		isColliding = !isColliding;

		if (isColliding == true)
		{
			this.SetQ(0.0, 0);
		}
	}

	//This function is called when the car's front ceases to make contact with a surface.
	public void CollisionExit()
	{
		isColliding = false;
	}

	//This function puts the car into and out of reverse.
	//The car needs to come to a near full stop before being allowed to change.
	public void setReverse()
	{
		throttle = 0;
		if (inReverse == false && GetQ(0) < 1)
		{
			inReverse = !inReverse;
		}
		else if (inReverse == true && GetQ(0) > -1)
		{
			inReverse = !inReverse;
		}

	}

	//These methods allow access to the gearRatio array.
	public double GetGearRatio() 
	{
		return gearRatio[gearNumber];
	}
	public void SetGearRatio(int index, double value) 
	{
		gearRatio[index] = value;
	}

	//The following are simply get/set-methods to allow for altering of properties during simulaton.
	public double Throttle {
		get {
			return throttle;
		}
		set {
			throttle = value;
		}
	}
	
	public double WheelAngle {
		get {
			return wheelAngle;
		}
		set {
			wheelAngle = value;
		}
	}
	
	public bool InReverse {
		get {
			return inReverse;
		}
	}
	
	public double SlopeAngle {
		get {
			return slopeAngle;
		}
		set {
			slopeAngle = value;
		}
	}
	
	public bool OnGround {
		get {
			return onGround;
		}
		set {
			onGround = value;
		}
	}
	
	
	public double MuR {
		get {
			return muR;
		}
	}
	
	public double OmegaE {
		get {
			return omegaE;
		}
		set {
			omegaE = value;
		}
	}
	
	public double Redline {
		get {
			return redline;
		}
	}
	
	public double FinalDriveRatio {
		get {
			return finalDriveRatio;
		}
	}
	
	public double WheelRadius {
		get {
			return wheelRadius;
		}
	}
	
	public int GearNumber {
		get {
			return gearNumber;
		}
		set {
			gearNumber = value;
		}
	}
	
	public int NumberOfGears {
		get {
			return numberOfGears;
		}
	}
	


}
