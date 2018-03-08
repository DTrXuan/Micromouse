#ifdef MOUSE_SOFTWARE

#include "Mouse.hpp"
#include "Utils.hpp"
#include "math.h"

Mouse* Mouse::instance = 0;

Mouse* Mouse::Instance()
{
    if (instance == 0)
    {
        instance = new Mouse();
    }

    return instance;
}

Mouse::Mouse()
{

}

void Mouse::Loop()
{

}

void Mouse::Setup()
{

}

int Mouse::GetBattery()
{
	return 100;
}

#endif
