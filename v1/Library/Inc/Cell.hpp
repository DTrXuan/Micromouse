#ifndef CELL_H
#define CELL_H

enum class Wall
{
	NONE = 0,
	NORTH = 1,
	EAST = 2,
	SOUTH = 4,
	WEST = 8
};

inline Wall operator|(Wall a, Wall b)
{
	return static_cast<Wall>(static_cast<int>(a) | static_cast<int>(b));
}

inline Wall operator^(Wall a, Wall b)
{
	return static_cast<Wall>(static_cast<int>(a) ^ static_cast<int>(b));
}

inline Wall operator~(Wall a)
{
	return static_cast<Wall>(~static_cast<int>(a));
}

class Cell
{
private:
	int _x;
	int _y;
	Wall _walls;

public:
	Cell();
	Cell(int x, int y);

	void AddWall(Wall wall);
	void RemoveWall(Wall wall);
};

#endif
