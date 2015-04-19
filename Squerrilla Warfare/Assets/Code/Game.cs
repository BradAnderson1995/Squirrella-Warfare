using UnityEngine;
using JetBrains.Annotations;

public class Game : MonoBehaviour {
	// ReSharper disable once MemberCanBePrivate.Global
	// ReSharper disable once NotAccessedField.Global
	public static Game game;
	// ReSharper disable once MemberCanBePrivate.Global
	public static NetworkManager networkManager;
	// ReSharper disable once MemberCanBePrivate.Global
	public static Assets assets;
	[UsedImplicitly] void Start () {
		game = this;
		networkManager = GetComponent<NetworkManager>();
		assets = GetComponent<Assets>();
	}
	[UsedImplicitly] void Update () {
		//if (Input.GetKeyDown("1"))
		//	SpawnSquirrell();
	}
	// ReSharper disable once InconsistentNaming
	[UsedImplicitly] void OnGUI () {
		Cursor.visible = showMenu;
		Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
		if (!NetworkManager.Connected)
			showMenu = true;
		if (showMenu)
			networkManager.DrawMenu();
    if (showLoadoutMenu)
      //Draw Loadout UI
      GUI.Box (new Rect (Screen.width/2 - 100, 20, 200, 220), "Loadout");
      //First Loadout
      GUI.BeginGroup (new Rect (0,0,Screen.width,Screen.height/3));
      GUI.Box (new Rect (50, 50, Screen.width/2 - 200, 400), primary_slot);
      if(GUI.Button (new Rect (20, 180, 20, 40), ""))
        //Change Weapon
      if(GUI.Button (new Rect (Screen.width/2 - 120, Screen.height/3 - 90, 20, 40), ""))
        //Change Weapon
      GUI.Box (new Rect(Screen.width/2 + 100, 10, Screen.width/2 - 200, 100), "Weapon 1")
      GUI.Box (new Rect(Screen.width/2 + 100, Screen.height/3 + 150, Screen.width/2 - 200, 300), "Description 1")
      GUI.EndGroup ();
      if (GUI.Button (new Rect (Screen.width/2 - 100, Screen.height - 120, 200, 100), "Spawn"))
		    SpawnSquirrell();
	}
	public static void JoinedGame () {
    showMenu = false;
    showLoadoutMenu = true;
	}
	public static void SpawnSquirrell () {Network.Instantiate(assets.squirrell, Vector3.zero, Quaternion.identity, 0);}
	public static bool showMenu;
}
