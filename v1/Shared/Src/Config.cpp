#include "Config.hpp"

const int Config::Width = 16;
const int Config::Height = 16;

const int Config::EndCellsLength = 4;
int Config::EndCells[Config::EndCellsLength * 2] =
{
	7, 7,
	7, 8,
	8, 7,
	8, 8,
};

