using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NavLink {

	public bool 	open = false;
	
	public NavLink( bool open) 
	{
		this.open = open;
	}	
}

public class Tile {
	public int elevation = TileMapConstants.kLevel0 + 1;


	public Vector2 pos = new Vector2(0.0f, 0.0f);

	public Dictionary<string, NavLink> navlinks = new Dictionary<string, NavLink>();

	public Dictionary<string, bool> adjlinks = new Dictionary<string, bool>();

	public GameObject gfx = null;
	public bool ShouldSerializegfx() { return false; }
	
	public List<GameObject> gfxWall = null;
	public bool ShouldSerializegfxWall() { return false; }

	
	public void Init(Vector2 tilepos, Transform parentTransform, int elevation, GameObject gfx) {

		this.pos = tilepos;
		this.elevation = elevation;

		gfxWall = new List<GameObject>();

		this.gfx = gfx;

		Vector3 lpos = gfx.transform.position;
		lpos.x = this.pos.x;
		lpos.y = this.elevation;
		lpos.z = this.pos.y;
		gfx.transform.position = lpos;


		this.gfx.transform.SetParent(parentTransform);


		Material mat = Resources.Load("Materials/floortile" + this.elevation, typeof(Material)) as Material; // i.e. .png, .jpg, etc
		Material [] mats = new Material[1];
		mats[0] = mat;
		gfx.transform.GetChild(0).GetComponent<Renderer> ().materials = mats;

	}

	public void EditTile( Vector2 tilepos, Dictionary<string, Tile> tileGraph, GameObject tileGFXPrefab, GameObject wallPrefab, Transform parentTransform) {

		if (gfx != null) {
			Object.Destroy( gfx);
		}

		// cycle tile elevations.
		this.elevation ++;

		if (this.elevation > TileMapConstants.kLevel0 + 3) 
		{
			this.elevation = TileMapConstants.kLevel0;
		}

		this.pos = tilepos;

		this.gfx = Object.Instantiate( tileGFXPrefab);

		Vector3 lpos = gfx.transform.position;
		lpos.x = this.pos.x;
		lpos.y = this.elevation;
		lpos.z = this.pos.y;
		gfx.transform.position = lpos;


		Material mat = Resources.Load("Materials/floortile" + this.elevation, typeof(Material)) as Material; // i.e. .png, .jpg, etc
		Material [] mats = new Material[1];
		mats[0] = mat;
		gfx.transform.GetChild(0).GetComponent<Renderer> ().materials = mats;


		gfx.transform.SetParent( parentTransform);

		// destory adjacent wall GFX
		gfxWall.ForEach( _gfx => Object.Destroy(_gfx));
		gfxWall = new List<GameObject>();
		foreach (KeyValuePair<string, bool> tileAdjacencyNodeKVP in adjlinks) 
		{
			tileGraph[ tileAdjacencyNodeKVP.Key ].gfxWall.ForEach( _gfx => Object.Destroy(_gfx));
			tileGraph[ tileAdjacencyNodeKVP.Key ].gfxWall = new List<GameObject>();
		}

		// clear nav links
		foreach ( KeyValuePair<string, NavLink> navlinkKVP in navlinks )
		{
			tileGraph[navlinkKVP.Key].navlinks.Remove( MakeGraphKey( this.pos ) ); // remove  inward link
		}
		navlinks.Clear(); // remove outward links


		// reassign nav links
		foreach (KeyValuePair<string, bool> tileAdjacencyNodeKVP in adjlinks) 
		{
			Tile adjacentTile = tileGraph[ tileAdjacencyNodeKVP.Key ];
			if ( adjacentTile.elevation == elevation) 
			{
				adjacentTile.navlinks.Add( MakeGraphKey(pos), new NavLink(true));
				navlinks.Add( MakeGraphKey( adjacentTile.pos ), new NavLink(true));
			}
		}
		
		// rebuild adjacent wall GFX
		foreach (KeyValuePair<string, bool> tileAdjacencyNodeKVP in adjlinks) 
		{
			Tile adjacentTile = tileGraph[ tileAdjacencyNodeKVP.Key ];

			adjacentTile.BuildWallGfx( tileGraph, wallPrefab,  parentTransform );
		}

	}

	public static string MakeGraphKey( Vector2 lpos)
	{	// using floats in dictionary keys is a bad idea for equlaity check purposes. 
		// Uses fixed precision to minimize possibilty of precision rounding issues.
		return string.Format( "{0:0.####}:{1:0.#####}", lpos.x,  lpos.y);
	}


	public void BuildWallGfx( Dictionary<string, Tile> tileGraph, GameObject wallPrefab, Transform parentTransform) 
	{

		foreach (KeyValuePair<string, bool> tileAdjacencyNodeKVP in adjlinks)
		{
			if ( navlinks.ContainsKey( tileAdjacencyNodeKVP.Key ) == false) 
			{
				
				if ( tileGraph.ContainsKey( tileAdjacencyNodeKVP.Key ) == true) // not needed?
				{
					Tile adjTile = tileGraph[ tileAdjacencyNodeKVP.Key ];

					Vector3 adjOffset =   new Vector3(adjTile.pos.x, 0, adjTile.pos.y) - new Vector3(pos.x, 0, pos.y) ;

					Vector3 adjHalfOffset = adjOffset / 2.0f;
					
					Quaternion rotate = Quaternion.AngleAxis( adjOffset.x * 90.0f, Vector3.up);

					for (int thisElevation = (int)Mathf.Min(elevation, adjTile.elevation); thisElevation < Mathf.Max(elevation, adjTile.elevation); thisElevation ++) 
					{
						Vector3 wallPos = new Vector3(pos.x+0.5f+adjHalfOffset.x, thisElevation, pos.y+0.5f+adjHalfOffset.z);
						gfxWall.Add( (GameObject)Object.Instantiate(wallPrefab, wallPos, rotate));
					}
					
					// zero width walls
					if (navlinks.Count > 0 && elevation == tileGraph[ tileAdjacencyNodeKVP.Key ].elevation)
					{
						Vector3 wallPos = new Vector3(pos.x+0.5f+adjHalfOffset.x, elevation, pos.y+0.5f+adjHalfOffset.z);

						gfxWall.Add( (GameObject)Object.Instantiate(wallPrefab, wallPos, rotate));
					}
				}
				
				gfxWall.ForEach( gfx => gfx.transform.SetParent(parentTransform));

			}
		}
	}
	

}
