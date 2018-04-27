#ifndef MOUSE_H
#define MOUSE_H

#include "vector"

class Cell;

enum class State
{
	IDLE,
	SEARCHING,
	RETURNING,
	RUNNING
};

enum class Target
{
	NONE,
	START,
	END,
	UNVISITED
};


class Mouse
{
private:
	/* Here will be the instance stored. */
	static Mouse* instance;

	/* Private constructor to prevent instancing. */
	Mouse();

	float _rotation;
	float _x, _y;
	State _state;
	Target _target;
	std::vector<Cell*> _targetCells;
	int _battery;
	const int _detectionWindowSize = 10;
	int __detectionValues[4][10];
	int _currentDetection = 0;

public:

    /* Static access method. */
	static Mouse* Instance();

	void Loop();
	void Setup();

	int UpdateBatteryPercentage();
	void ReadAllSensors();

	float GetPositionX();
	float GetPositionY();
	void SetPosition(float x, float y);

	float GetRotation();
	void SetRotation(float rotation);

	int GetBattery();
	void GetWallSensors();
	int GetWallRead(int wallIndex);
	void StepMotion();

	void SetState(State state);
	State GetState();
	void SetTarget(Target target);
	Target GetTarget();

	void SetTargetCells(std::vector<Cell*> cells);
	std::vector<Cell*> GetTargetCells();

	bool IsRotatedTowards(Cell* cell);
	void RotateTowards(Cell* cell);

	void Reset();

	static bool SortFloodfillCells(Cell* a, Cell* b);
};

#endif
