#ifndef MAZE_H
#define MAZE_H

#include "Cell.hpp"
#include "Config.hpp"

class Maze
{
private:
	/* Here will be the instance stored. */
	static Maze* instance;

	/* Private constructor to prevent instancing. */
	Maze();


public:
    /* Static access method. */
	static Maze* Instance();

	Cell* cells;
};

#endif
