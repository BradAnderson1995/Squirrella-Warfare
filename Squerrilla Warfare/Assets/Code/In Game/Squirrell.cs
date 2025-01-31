using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly] public class Squirrell : MonoBehaviour {
	const float speed = 3f;
	const float jumpHeight = 4f;
	const float climbSpeed = 2f;
	bool canJump = true;
	HashSet<Collider> currentlyColliding;
	/* Weapon Code Ends */
	Collider currentTree;
	int currentHealth;
	int CurrentHealth {
		get {return currentHealth;}
		set {
			currentHealth = value;
			if (currentHealth <= 0)
				Die();
		}
	}
	// ReSharper disable MemberCanBePrivate.Global
	public Weapon weapon1;
	public Weapon weapon2;
	// ReSharper restore MemberCanBePrivate.Global
	int MaxAmmo {get {return currentWeapon.MaxAmmo;}}
    int MaxClip {get {return currentWeapon.ClipSize;}}

	float FireDelay {get {return currentWeapon.FireDelay;}}
	/* Weapon Code Begins */
	float fireTimer;
	[UsedImplicitly] public GameObject gunHand;
	// new NetworkView networkView;
	//private float camRayLength = 100f;
	float h, v;
	const int maxHealth = 100; //max health. Change as needed
	Vector3 movement;
	new NetworkView networkView;
	Rigidbody playerRigidbody;
	Rigidbody rigidBody;
	GameObject weaponModel = null;
	//getters and setters for health and ammo
	public void Damage (int amount) {CurrentHealth -= amount;}
	public void Damage (float amount) {Damage(Mathf.RoundToInt(amount));}
    private Animator anim;
	int Ammo {
        get {return CurrentWeapon.totalAmmo;}
        set {CurrentWeapon.totalAmmo = value;}
    }
	int AmmoInClip { 
        get {return CurrentWeapon.ammoInClip;}
        set {CurrentWeapon.ammoInClip = value;}
    }
	void UseAmmo () {CurrentWeapon.ammoInClip -= 1;}
	Weapon currentWeapon;
	void Die () {}//todo
	// ReSharper disable once MemberCanBePrivate.Global
	public Weapon CurrentWeapon {get {return currentWeapon;}
		set {
			currentWeapon = value;
			if (weaponModel != null) //Destroy our previous prefab if it exists
				Network.Destroy(weaponModel);
			weaponModel = gunHand.InstantiateChild(value.ModelPrefab);
			weaponModel = value.ModelPrefab;
		}
	}
	void Shoot () {
		fireTimer = 0f;
		UseAmmo();
		Debug.Log(CurrentHealth);
		currentWeapon.Fire();
	}
	bool CanFullyReload {get {return Ammo >= MaxClip - AmmoInClip;}}
	void TryToReload () {
		if (CanFullyReload) {
			Ammo = Ammo - MaxClip - AmmoInClip;
			AmmoInClip =(MaxClip);
			fireTimer = -currentWeapon.ReloadTime + currentWeapon.FireDelay;
		}
		else if (Ammo > 0) {
			AmmoInClip =(AmmoInClip + Ammo);
			Ammo = 0;
			fireTimer = currentWeapon.FireDelay - currentWeapon.ReloadTime;
		}
	}
	[UsedImplicitly] void Start () {
		CurrentWeapon = new AssaultRifle();
		if (MaxAmmo == 0)//Prevents warning. Remove when implemented!
			fireTimer = 0;
		Ammo = MaxAmmo;
		AmmoInClip = MaxClip;
		fireTimer = 0f;
		CurrentHealth = maxHealth;
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.freezeRotation = true;
		networkView = GetComponent<NetworkView>();
		playerRigidbody = GetComponent<Rigidbody>();
		currentlyColliding = new HashSet<Collider>();
		// currentTree = null;
		if (networkView.isMine) {
			var cameraObj = new GameObject("squirrell camera");
			cameraObj.transform.parent = transform;
			var camera = cameraObj.AddComponent<Camera>();
			camera.transform.localPosition = new Vector3(0, 5, -10);
			cameraObj.AddComponent<CameraVert>();
		}
	    anim = GetComponent<Animator>();
	}
	[UsedImplicitly] void Update () {
		if (networkView.isMine && !Game.showMenu)
			InputMovement();
		fireTimer += Time.deltaTime;
        if (Input.GetButton("Fire1"))
            TryToShoot();
        else if (Input.GetButton("Reload") && AmmoInClip < MaxClip)
            TryToReload();
        else if (Input.GetButton("Slot1") && CurrentWeapon != weapon1)
            CurrentWeapon = weapon1;
        else if (Input.GetButton("Slot2") && CurrentWeapon != weapon2)
            CurrentWeapon = weapon2;
	}
	void TryToShoot () {
		if (AmmoInClip == 0)
			TryToReload();
		else if (fireTimer >= FireDelay)
			Shoot();
	}
	void InputMovement () {
		h = Input.GetAxis("Horizontal");
		v = Input.GetAxis("Vertical");
		if (currentTree == null) {
			if (Input.GetKeyDown("space") && canJump) {
				var vel = playerRigidbody.velocity;
				vel.y = jumpHeight;
				playerRigidbody.velocity = vel;
			    anim.SetTrigger("Jump");
			}
			Move();
		}
		if (!Game.showMenu)
			Turning();
	}
	[UsedImplicitly] void OnCollisionEnter (Collision collision) {
		//string tag = collision.collider.gameObject.tag;
		currentlyColliding.Add(collision.collider);
	}
	[UsedImplicitly] void OnCollisionStay (Collision collision) {
		if (collision.collider == currentTree) {
			if (!WantsClimbing()) {
				currentTree = null;
				playerRigidbody.useGravity = true;
				return;
			}
		}
		else {
			if (collision.collider.gameObject.tag == "Tree" && WantsClimbing()) {
				currentTree = collision.collider;
				playerRigidbody.useGravity = false;
				playerRigidbody.velocity.Set(playerRigidbody.velocity.x, playerRigidbody.velocity.y, climbSpeed);
			}
		}
		// Only allow jumping on the ground and trees
		if (collision.transform.tag == "Climbable")
			canJump = true;
	}
	bool WantsClimbing () {return Input.GetKey(KeyCode.LeftShift) && v > 0.5;}
	[UsedImplicitly] void OnCollisionExit (Collision collision) {
		currentlyColliding.Remove(collision.collider);
		if (collision.collider == currentTree) {
			currentTree = null;
			playerRigidbody.useGravity = true;
		}
		canJump = false;
	}

    private void Move()
    {
        movement.Set(h, 0f, v);
        //movement = movement.normalized;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement.z = movement.z*2;
        }
        movement = playerRigidbody.rotation*movement;
        movement *= speed*Time.deltaTime;
        playerRigidbody.MovePosition(transform.position + movement);
        bool walking = h != 0f || v != 0f;
        bool leftStrafe = h < 0f && v == 0f;
        bool rightStrafe = h > 0f && v == 0f;
        if (!leftStrafe)
            anim.SetBool("LeftStrafe", false);
        if (!rightStrafe)
            anim.SetBool("RightStrafe", false);
        if (walking)
        {
            if (!leftStrafe && !rightStrafe)
                anim.SetBool("IsRunning", true);
            else if (leftStrafe)
                anim.SetBool("LeftStrafe", true);
            else if (rightStrafe)
                anim.SetBool("RightStrafe", true);
        }
        else
        {
            anim.SetBool("IsRunning", false);
        }
    }

    void Turning () {
		var rotX = Input.GetAxis("Mouse X");
		playerRigidbody.MoveRotation(playerRigidbody.rotation * Quaternion.AngleAxis(rotX * 2, Vector3.up));
	}
}
