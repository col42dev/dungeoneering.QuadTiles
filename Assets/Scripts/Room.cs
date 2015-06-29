using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Room {
	public int index = 0;
	public int x, y;
	public int width, height;

	public GameObject roomGFX;
	
	public List<Room> edges;
	
	public Room( GameObject roomGFX){
		this.roomGFX = roomGFX;	
		edges = new List<Room>();
	}
	
	public	bool Overlaps( Room other) 
	{
		int border = 1; // HACK: add a border in the calculation because it simplfies the nav graph construction if rooms do not border each other.

		bool xOverlap = Enumerable.Range(other.x - border, other.width + (border * 2)).Contains(x) || Enumerable.Range(other.x, other.width).Contains(x + width);
		bool yOverlap = Enumerable.Range(other.y - border, other.height + (border * 2)).Contains(y) || Enumerable.Range(other.y, other.height).Contains(y + height);
		
		if (xOverlap && yOverlap) {
			return true;
		}
		return false;
	}
	
	public Vector2 Center() {
		return new Vector2((float)x + (float)width/2.0f, (float)y + (float)height/2.0f);
	}
	
	public Vector3 Center3() {
		return new Vector3((float)x + (float)width/2.0f, 0f, (float)y + (float)height/2.0f);
	}
}
