#include "Maze.hpp"
#include "Config.hpp"

Maze* Maze::instance = 0;

Maze* Maze::Instance()
{
    if (instance == 0)
    {
        instance = new Maze();
    }

    return instance;
}

Maze::Maze()
{
	cells = new Cell[Config::Width * Config::Height * sizeof(Cell)];

	for(int y = 0; y < Config::Height; y++)
	{
		for(int x = 0; x < Config::Width; x++)
		{
			int index = y * Config::Width + x;
			Cell newCell = Cell(x, y);

			if(x == 0)
				newCell.AddWall(Wall::WEST);

			if(x == Config::Width - 1)
				newCell.AddWall(Wall::EAST);

			if(y == 0)
				newCell.AddWall(Wall::SOUTH);

			if(y == Config::Height - 1)
				newCell.AddWall(Wall::NORTH);

			cells[index] = newCell;
		}
	}
}
