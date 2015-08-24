# dungeoneering.QuadTiles
A dungeon layout editor using quad based tiles. Implemented using Unity it run in desktop browsers which have the Unity plugin installed.

The editor is a grid based world map which is initialized with a procedurally generated dungeon layout, a random number of rooms which are connected with corridors. A [relative neighbor graph](https://en.wikipedia.org/wiki/Relative_neighborhood_graph) is used to connect the room nodes. 

Left clicking will place a quad tile at that location or raise the height of the existing tile by one. If there is already a four height tile at the location then its height will be set to zero. 

# screen grabs
![Screen1](https://raw.githubusercontent.com/col42dev/dungeoneering.QuadTiles/master/Documentation/Screen%20Shot%202015-08-24%20at%2015.59.39.png)
