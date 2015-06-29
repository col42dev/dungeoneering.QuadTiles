using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;




public class IntVector2 {
	public int x, y;
	public IntVector2(int x, int y) {
		this.x = x;
		this.y = y;
	}
}


public static class TileMapConstants
{
	public const int kLevel0 = 0;
}




public class MapGen : MonoBehaviour {

	public GameObject tileGFXPrefab;

	public GameObject wallPrefab;


	public GameObject polygonTilePrefab;
	
	List<Room> rooms;
	int numRooms = 35;
	int sizeMin = 5;
	int sizeMax = 10;
	private const int initSpacing = 10;

	public class TileGraphSerilizable {
		public IntVector2 tileMapDim  ;
		public Dictionary<string, Tile> tileGraph = null;

		public bool ShouldSerializetileGraphAt() { return false; }
		public Tile tileGraphAt(Vector2 pos) 
		{
			string graphKey = Tile.MakeGraphKey ( pos );
			return tileGraph [graphKey];
		}

	}

	TileGraphSerilizable tgs;




	// Use this for initialization
	void Start () {

		tgs = new TileGraphSerilizable ();

		CreateRooms ();

		SpreadRooms();

		ShiftRoomsToPositiveQuadrant ();

		// asign indexs
		for (int i = 0; i < rooms.Count; i ++) {
			rooms[i].index = i;
		}

		CreateRoomGraph();

		CreateTileGraph ();

		BuildWallsGfx ();

	}

	
	// Update is called once per frame
	void Update() {


		// Render room graph
		if (rooms != null) {
			foreach (Room r1 in rooms) {
				foreach (Room r2 in r1.edges) {
					Debug.DrawLine (r1.Center3 (), r2.Center3 ());
				}
			}
		}


		// render debug navlinks
		if (tgs != null && tgs.tileGraph != null) {
			foreach (KeyValuePair<string, Tile> tileGraphNodeKVP in tgs.tileGraph) {
				Tile thisTile = tileGraphNodeKVP.Value;

				string[] source = tileGraphNodeKVP.Key.Split (':');
				Vector2 sourcenav = new Vector2 ( float.Parse(source [0]), float.Parse(source [1]) );

				if (thisTile.navlinks != null) {
					foreach (KeyValuePair<string, NavLink> navlinkKVP in thisTile.navlinks) {
						NavLink thisNavLink = navlinkKVP.Value;
						if (thisNavLink.open == true) {
							string[] target = navlinkKVP.Key.Split (':');
							Vector2 targetnav = new Vector2 ( float.Parse(target [0]), float.Parse(target [1]) );

							Debug.DrawLine (new Vector3 (sourcenav.x + 0.5f, 0, sourcenav.y + 0.5f), new Vector3 (targetnav.x + 0.5f, 0, targetnav.y + 0.5f), Color.green, 0.0f, false);
						}
					}
				}
			}
		}

	}

	void OnGUI() {
		if (GUI.Button (new Rect (10, 10, 50, 50), "New")) {
			foreach(Transform child in this.transform)
			{
				Destroy(child.gameObject);
			}
			
			Start();
		}

		#if !UNITY_ANDROID
		/*
		if (GUI.Button (new Rect (10, 50, 50, 50), "Export")) {

			
			string json = JsonConvert.SerializeObject(tgs, Formatting.Indented, new JsonSerializerSettings { 
				ReferenceLoopHandling = ReferenceLoopHandling.Serialize
			});

			Debug.Log ("writing file");
			var sr = File.CreateText("mapgen.json");
			sr.WriteLine (json);
			sr.Close();
		}


		if (GUI.Button (new Rect (10, 90, 50, 50), "Import")) {
			
			foreach(Transform child in this.transform)
			{
				Destroy(child.gameObject);
			}

			Debug.Log ("reading file");
			string json = File.ReadAllText("mapgen.json");
			
			tgs = JsonConvert.DeserializeObject< TileGraphSerilizable >( json);


			Debug.Log ("done reading file");

			foreach(KeyValuePair<string, Tile> tileGraphNodeKVP in tgs.tileGraph)
			{
				Tile thisGraphTile =  tileGraphNodeKVP.Value;

				thisGraphTile.Init(thisGraphTile.pos, this.transform, thisGraphTile.elevation, Instantiate(tileGFXPrefab));
			}

			BuildWallsGfx();


			//Start();
		}
		*/
		#endif

	}



	public void EditTile(float x, float y) 
	{
		if ( tgs != null && tgs.tileGraph.ContainsKey( Tile.MakeGraphKey( new Vector2(x, y)) ) == true)
		{
			tgs.tileGraphAt( new Vector2(x, y) ).EditTile(  new Vector2(x, y),  tgs.tileGraph,  tileGFXPrefab, wallPrefab, this.transform);
		}
	}

	private void CreateRooms() {
		rooms = new List<Room>();

		for (int i = 0; i < numRooms; i ++) {
			Room r = new Room( Instantiate(tileGFXPrefab));
			r.roomGFX.transform.SetParent( this.transform);

			r.x = Random.Range(-initSpacing, initSpacing);
			r.y = Random.Range(-initSpacing, initSpacing);
			
			bool wider = false; //Random.Range(0, 2) == 2;
			r.width = Random.Range(sizeMin, sizeMax) - (wider ? 0 : Random.Range(0,4));
			r.height = Random.Range(sizeMin, sizeMax) - (!wider ? 0 : Random.Range(0,4));

			rooms.Add(r);
		}
	}


	private void SpreadRooms() 
	{
		bool bOverlap = false;
		int spreadCount = 0;
		do {
			bOverlap = false;
			for (int i = 0; i < rooms.Count; i ++) {
				for (int j = 0; j < rooms.Count; j ++) {
					if (i == j) {
						continue;
					}
					
					if (rooms [i].Overlaps (rooms [j])) 
					{
						spreadCount ++;
						bOverlap = true;
						Vector2 ci = rooms [i].Center ();
						Vector2 cj = rooms [j].Center ();
						
						Vector2 dir = ci - cj;
						if (dir.x > 0) {
							rooms [i].x += Random.Range (0, 2); // use random offset to enable a winner in competing overlaps
						}
						if (dir.y > 0) {
							rooms [i].y += Random.Range (0, 2); // use random offset to enable a winner in competing overlaps
						}
						if (dir.x < 0) {
							rooms [i].x -= Random.Range (0, 2); // use random offset to enable a winner in competing overlaps
						}
						if (dir.y < 0) {
							rooms [i].y -= Random.Range (0, 2); // use random offset to enable a winner in competing overlaps
						}
					}	
					
					if( spreadCount > 10000) { //hack to prevent occasional infinite loop
						Debug.Log ("Room layout spreading was interrupted");
						break;
					}
				}
			}
		} while (bOverlap == true);
	}

	// Simplify tilemap processing by moving rooms entirely in to positive coordinate space
	private void ShiftRoomsToPositiveQuadrant() 
	{
		int minX = 0;
		int minY = 0;
		for (int i = 0; i < rooms.Count; i ++) {
			if (rooms[i].x < minX) {
				minX = rooms[i].x;
			}
			
			if (rooms[i].y < minY) {
				minY = rooms[i].y;
			}
		}
		
		for (int i = 0; i < rooms.Count; i ++) {
			rooms[i].x += Mathf.Abs(minX) + 1;
			rooms[i].y += Mathf.Abs(minY) + 1;
		}
	}


	private void CreateRoomGraph() {
		
		// relative neighbor graph https://en.wikipedia.org/wiki/Relative_neighborhood_graph
		// In computational geometry, the relative neighborhood graph (RNG) is an undirected graph defined on a set of points 
		// in the Euclidean plane by connecting two points p and q by an edge whenever there does not exist a third point r 
		// that is closer to both p and q than they are to each other.In computational geometry, the relative neighborhood graph (RNG) 
		// is an undirected graph defined on a set of points in the Euclidean plane by connecting two points p and q by an edge whenever 
		// there does not exist a third point r that is closer to both p and q than they are to each other.
		int createEdgeCount = 0;
		
		for (int i = 0; i < rooms.Count-1; i ++) {
			for (int j = i+1; j < rooms.Count; j ++) {
				float dist = Vector2.Distance( rooms[i].Center(), rooms[j].Center());
				
				bool createEdge = true;
				for (int k = 0; k < rooms.Count; k ++) {
					if ( k==i || k==j) {
						continue;
					}
					
					if ( Vector2.Distance(rooms[i].Center(), rooms[k].Center())  < dist && 
					    Vector2.Distance(rooms[j].Center(), rooms[k].Center())  < dist) {
						createEdge = false;	
						break;
					}
				}
				
				if (createEdge) {
					rooms[i].edges.Add(rooms[j]);
					rooms[j].edges.Add(rooms[i]);
					createEdgeCount ++;
				}
			}
		}
	}

	private void CreateTileGraph() 
	{
		BuildTileGraph();

		BuildTileRectangularAdjacencyGraph();

		BuildRoomTilesNavGraph();
		BuildCorridorTileNodes();

		BuildElevatedTiles();
		BuildElevatedTilesNavGraph ();
	}

	private void BuildTileGraph() 
	{
		int maxX = 0;
		int maxY = 0;
		for (int i = 0; i < rooms.Count; i ++) {
			if (rooms [i].x + rooms [i].width > maxX) {
				maxX = rooms [i].x + rooms [i].width;
			}
			if (rooms [i].y + rooms [i].height > maxY) {
				maxY = rooms [i].y + rooms [i].height;
			}
		}
		
		tgs.tileMapDim = new IntVector2( maxX + 1, maxY + 1);

		tgs.tileGraph = new Dictionary<string, Tile> ();

		for (int x = 0; x < tgs.tileMapDim.x; x++) {
			for (int y = 0; y < tgs.tileMapDim.y; y++) {

				tgs.tileGraph.Add ( Tile.MakeGraphKey( new Vector2(x, y)), new Tile() );

				Tile thisGraphTile = tgs.tileGraphAt( new Vector2(x, y));

				thisGraphTile.pos.x = x;
				thisGraphTile.pos.y = y;
			}
		}
	}


	private void BuildTileRectangularAdjacencyGraph() 
	{
		foreach(KeyValuePair<string, Tile> tileGraphNodeKVP in tgs.tileGraph)
		{
			Tile thisGraphTile =  tileGraphNodeKVP.Value;
				
			string adjGraphKey = Tile.MakeGraphKey( thisGraphTile.pos - new Vector2(1.0f, 0.0f));
			if ( tgs.tileGraph.ContainsKey( adjGraphKey ) == true ) 
			{
				tileGraphNodeKVP.Value.adjlinks.Add( adjGraphKey, true );
			} 

			adjGraphKey = Tile.MakeGraphKey( thisGraphTile.pos + new Vector2(1.0f, 0.0f));
			if ( tgs.tileGraph.ContainsKey( adjGraphKey ) == true ) 
			{
				tileGraphNodeKVP.Value.adjlinks.Add( adjGraphKey, true );
			} 

			adjGraphKey = Tile.MakeGraphKey( thisGraphTile.pos - new Vector2(0.0f, 1.0f));
			if ( tgs.tileGraph.ContainsKey( adjGraphKey ) == true ) 
			{
				tileGraphNodeKVP.Value.adjlinks.Add( adjGraphKey, true );
			} 

			adjGraphKey = Tile.MakeGraphKey( thisGraphTile.pos + new Vector2(0.0f, 1.0f));
			if ( tgs.tileGraph.ContainsKey( adjGraphKey ) == true ) 
			{
				tileGraphNodeKVP.Value.adjlinks.Add( adjGraphKey, true );
			} 
		}
	}


	void BuildRoomTilesNavGraph()
	{
		for (int i = 0; i < rooms.Count; i ++) {
			Room r = rooms [i];
			for (int x = 0; x < r.width; x++) {
				for (int y = 0; y < r.height; y++) {

					Tile thisGraphTile = tgs.tileGraphAt( new Vector2(r.x + x, r.y + y) );

					thisGraphTile.Init ( new Vector2(r.x + x, r.y + y), this.transform, TileMapConstants.kLevel0, Instantiate (tileGFXPrefab));

					if (x != 0) {
						thisGraphTile.navlinks.Add( Tile.MakeGraphKey( new Vector2(r.x + x - 1, r.y + y)), new NavLink(true) );
					} 

					if (x != r.width - 1) {
						thisGraphTile.navlinks.Add( Tile.MakeGraphKey( new Vector2(r.x + x + 1, r.y + y) ), new NavLink(true) );
					} 

					if (y != 0) {
						thisGraphTile.navlinks.Add( Tile.MakeGraphKey( new Vector2(r.x + x, r.y + y - 1) ), new NavLink(true) );
					} 
					
					if (y != r.height - 1) {
						thisGraphTile.navlinks.Add( Tile.MakeGraphKey( new Vector2(r.x + x, r.y + y + 1) ), new NavLink(true) );
					} 
				}
			}
		}
	}

	private void BuildCorridorTileNodes() {
		
		for (int i = 0; i < rooms.Count; i ++) {
			
			int x = Mathf.FloorToInt(rooms[i].Center().x);
			int y = Mathf.FloorToInt(rooms[i].Center().y);
			
			foreach (Room e in rooms[i].edges) 
			{
				if (e.index < i) { continue; } // so that corridors are not constructed twice for bi-directional edges.
				
				BuildCorridorTileNode(x,y, Mathf.FloorToInt( e.Center().x), Mathf.FloorToInt( e.Center().y));
			}
		}
	}
	
	private void BuildElevatedTiles() 
	{
		//any tile which doesn't yet have a GFX is now assigned to be an elevated tile.
		foreach(KeyValuePair<string, Tile> tileGraphNodeKVP in tgs.tileGraph)
		{
			Tile thisGraphTile =  tileGraphNodeKVP.Value;
			
			if ( thisGraphTile.gfx == null) 
			{
				thisGraphTile.Init( thisGraphTile.pos, this.transform, TileMapConstants.kLevel0 + 1, Instantiate(tileGFXPrefab));
			}
		}
	}


	private void BuildElevatedTilesNavGraph()
	{
		foreach(KeyValuePair<string, Tile> tileGraphNodeKVP in tgs.tileGraph)
		{
			Tile thisGraphTile =  tileGraphNodeKVP.Value;

			if ( thisGraphTile.elevation > 0 ) // 'blocked' tile
			{
				foreach(KeyValuePair<string, bool> tileAdjacencyNodeKVP in thisGraphTile.adjlinks)
				{
					if ( tgs.tileGraph[ tileAdjacencyNodeKVP.Key ].elevation == thisGraphTile.elevation )
					{
						thisGraphTile.navlinks.Add( tileAdjacencyNodeKVP.Key, new NavLink(true) );
					}
				}
			}
		}

	}






	private void BuildWallsGfx() 
	{
		foreach(KeyValuePair<string, Tile> tileGraphNodeKVP in tgs.tileGraph)
		{
			tileGraphNodeKVP.Value.BuildWallGfx( tgs.tileGraph, wallPrefab, this.transform );
		}
	}

	private void BuildCorridorTileNode(int x1, int y1, int x2, int y2) 
	{
		bool doXFirst = true;

		int dirX = x2 > x1 ? +1 : -1;
		int dirY = y2 > y1 ? +1 : -1;

		while (x1 != x2 || y1 != y2) {

			if (x1 == x2 || (doXFirst==false && y1!=y2)) 
			{
				Vector2 thisTilePos = new Vector2(x1, y1);
				// nav 
				string adjGraphKey = Tile.MakeGraphKey( new Vector2(x1, y1 + dirY));
				if ( tgs.tileGraphAt( thisTilePos ).navlinks.ContainsKey( adjGraphKey ) == false)
				{
					tgs.tileGraphAt( thisTilePos ).navlinks.Add( adjGraphKey, new NavLink(true) );
				}

				y1 += dirY;
				thisTilePos = new Vector2(x1, y1);

				// new tile
				if ( tgs.tileGraphAt( thisTilePos ).elevation != TileMapConstants.kLevel0) 
				{
					tgs.tileGraphAt( thisTilePos ).Init( thisTilePos, this.transform , TileMapConstants.kLevel0, Instantiate(tileGFXPrefab));
				}

				// new tile nav
				adjGraphKey = Tile.MakeGraphKey( new Vector2( x1, y1 - dirY) );
				if ( tgs.tileGraphAt( thisTilePos ).navlinks.ContainsKey( adjGraphKey ) == false)
				{
					tgs.tileGraphAt( thisTilePos ).navlinks.Add( adjGraphKey, new NavLink(true) );
				}
			} 
			else 
			{
				Vector2 thisTilePos = new Vector2(x1, y1);
				// nav 
				string adjGraphKey = Tile.MakeGraphKey( new Vector2( x1 + dirX, y1) );
				if ( tgs.tileGraphAt( thisTilePos ).navlinks.ContainsKey( adjGraphKey ) == false)
				{
					tgs.tileGraphAt( thisTilePos ).navlinks.Add( adjGraphKey, new NavLink(true) );
				}

				x1 += dirX;
				thisTilePos = new Vector2(x1, y1);

				if ( tgs.tileGraphAt( thisTilePos ).elevation != TileMapConstants.kLevel0) 
				{
					tgs.tileGraphAt( thisTilePos ).Init( thisTilePos, this.transform , TileMapConstants.kLevel0, Instantiate(tileGFXPrefab));
				}

				// new tile nav
				adjGraphKey = Tile.MakeGraphKey( new Vector2(x1 - dirX, y1) );
				if ( tgs.tileGraphAt( thisTilePos ).navlinks.ContainsKey( adjGraphKey ) == false)
				{
					tgs.tileGraphAt( thisTilePos ).navlinks.Add( adjGraphKey, new NavLink(true) );
				}
			}
		}

	}

	private bool IsRoomTile( int x1, int y1) 
	{
		for (int i = 0; i < rooms.Count; i ++) 
		{
			Room r = rooms [i];
			for (int x = 0; x < r.width; x++) 
			{
				for (int y = 0; y < r.height; y++) 
				{
					if (x1 == r.x + x && y1 == r.y + y)
					{
						return true;
					}
				}
			}
		}

		return false;
	}
					
















}
