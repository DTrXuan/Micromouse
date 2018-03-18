#ifndef CELL_H
#define CELL_H

enum class Wall : unsigned char
{
	NONE = 0,
	NORTH = 1,
	EAST = 2,
	SOUTH = 4,
	WEST = 8
};

Wall GetOppositeWall(Wall wall);


inline Wall operator&(Wall a, Wall b)
{
	return static_cast<Wall>(static_cast<int>(a) & static_cast<int>(b));
}

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
	Wall _detectedWalls;

	bool _visited;
	bool _queued;
	bool _skipped;
	bool _unreachable;
	int _floodfill;

public:
	Cell();
	Cell(int x, int y);

	void AddWall(Wall wall, bool detected);
	void SetWall(Wall wall, bool detected);
	void RemoveWall(Wall wall, bool detected);
	bool HasWall(Wall wall, bool detected);
	Wall GetWalls(bool detected);

	bool IsVisited();
	void SetVisited(bool visited);

	bool IsSkipped();
	void SetSkipped(bool skipped);

	bool IsUnreachable();
	void SetUnreachable(bool reachable);

	int GetFloodfill();
	void SetFloodfill(int floodfill);

	int GetPositionX();
	int GetPositionY();

	void SetQueued(bool queued);
	bool GetQueued();

	void Reset(bool detected);

	bool IsStart();
	bool IsTarget();
	bool IsEnd();
};

#endif
