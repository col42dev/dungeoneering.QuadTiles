using UnityEngine;
using System;
using System.Collections;

public class CameraControl : MonoBehaviour {

	private float mouseSensitivity  = 0.2f;
	private Vector3 lastPosition = new Vector3();

	Plane floorPlane;

	 int lastXpos = -1;
	 int lastZpos = -1;

	// Use this for initialization
	void Start () {
	
		floorPlane = new Plane(Vector3.up, new Vector3(0.0f, 0.0f));
	}
	
	// Update is called once per frame
	void Update () {
	

		#if !UNITY_ANDROID
		if (Input.GetMouseButtonDown(1) )
		{
			lastPosition = Input.mousePosition;
		}
		
		if (Input.GetMouseButton(1))
		{
			Vector3 delta  = Input.mousePosition - lastPosition;
			transform.Translate(delta.x * mouseSensitivity, delta.y * mouseSensitivity, delta.y * mouseSensitivity);
			lastPosition = Input.mousePosition;
		}


		transform.Translate(0.0f, 0.0f, Input.GetAxis("Mouse ScrollWheel") * 12);
		#endif 

		// Edit tile
		if (Input.GetMouseButton(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)) {

			Vector2 screenSelectionPos = Input.mousePosition;
			if ( Input.touchCount == 1)
			{
				screenSelectionPos = Input.GetTouch(0).position;
			}

			Ray ray = Camera.main.ScreenPointToRay (screenSelectionPos);
			float hitDist = 0.0f;

			if ( floorPlane.Raycast( ray, out hitDist)) {
				MapGen map  = GameObject.Find("Map").GetComponent<MapGen>();
				Vector3 pointerPosition = ray.GetPoint(hitDist);

				if ( lastXpos != (int)Math.Floor(pointerPosition.x) || lastZpos !=(int)Math.Floor(pointerPosition.z)) {

		
					Debug.Log ("EditTile" + (int)Math.Floor(pointerPosition.x) + ", " + (int)Math.Floor(pointerPosition.z));

					lastXpos = (int)Math.Floor(pointerPosition.x);
					lastZpos = (int)Math.Floor(pointerPosition.z);

					map.EditTile( (int)Math.Floor(pointerPosition.x), (int)Math.Floor(pointerPosition.z));

				}
			}
		} else{

			lastXpos = -1;
			lastZpos = -1;
		}



	}
}
