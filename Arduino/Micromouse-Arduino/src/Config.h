#ifndef Config_h
#define Config_h

namespace Pins
{
	const int SWITCH_1 = 2;
	const int SWITCH_2 = 3;

	const int STEPPER_LEFT_STEP = 9;
	const int STEPPER_LEFT_DIR = 8;
	const int STEPPER_RIGHT_STEP = 11;
	const int STEPPER_RIGHT_DIR = 10;
}

namespace Constants
{
	const int MAZE_WIDTH = 16;
	const int MAZE_HEIGHT = 16;

	const int END_WIDTH = 2;
	const int END_HEIGHT = 2;

	const int END_DELTA_X = 0;
	const int END_DELTA_Y = 0;

	const int DELAY_PAUSE_SECONDS = 3;

	const int CONNECTION_MAX_RETRIES = 5;
	const int DELAY_CONNECTION = 1000;

	const int SENSOR_READ_MICROSECONDS = 5;

	const int WHEEL_SPACING_MM = 81; // mm
	const int WHEEL_RADIUS_MM = 31; // mm
	const int WHEEL_STEPS_PER_REVOLUTION = 200;

	const bool connectedToSerial = false;
}

namespace Variables
{
	extern bool forcedPause;
	extern bool pause;
}

#endif
