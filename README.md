# SeamlessVoronoi
Seamless voronoi custom node for the Unity's shader graph.

# How to use the node: 
The node have 4 inputs and 3 outputs. The first input: UV. It is ,you guessed it, for any kind of UV. The second one: Height, is used as a variation for the voronoi. Use for exemple with a time node multiplied by 0.1. The third: CellDensity, is a zoom for more/less cells created. And the last input: Period. It represent how often the noise tile.

The first output is a basic voronoi ![Basic voronoi](https://github.com/Xentiie/SeamlessVoronoi/tree/master/Screenshots/VoronoiExemple1.png).
The second output is only the cells of the voronoi ![Cells](https://github.com/Xentiie/SeamlessVoronoi/tree/master/Screenshots/VoronoiExemple2.png).
And the last one is for the borders of the cells ![Borders](https://github.com/Xentiie/SeamlessVoronoi/tree/master/Screenshots/VoronoiExemple3.png).
