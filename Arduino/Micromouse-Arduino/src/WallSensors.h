#ifndef WallSensors_h
#define WallSensors_h

#include <Arduino.h>

namespace WallSensors
{
	const int NUM_SENSORS = 4;
	const int READS_WINDOW_MAX_SIZE = 5;

	const int CALIBRATION_SECONDS = 3;

	const int LED_LF = 4;
	const int LED_DL = 5;
	const int LED_DR = 6;
	const int LED_RF = 7;

	const int LEFT_FT = A1;
	const int LEFT_DG = A2;
	const int RIGHT_DG = A3;
	const int RIGHT_FT = A0;

	enum LED_INDEX
	{
		RF,
		DR,
		DL,
		LF,
	};

	const int SensorsPairs[NUM_SENSORS][2] = {
		{ LED_LF, LEFT_FT },
		{ LED_DR, RIGHT_DG },
		{ LED_DL, LEFT_DG },
		{ LED_RF, RIGHT_FT }
	};

	extern int sensorsCalibrationMin[NUM_SENSORS];
	extern int sensorsCalibrationMax[NUM_SENSORS];

	extern int sensorWindowRealSize;
	extern int sensorValuesWindow[NUM_SENSORS][READS_WINDOW_MAX_SIZE];
	extern int sensorValuesSmooth[NUM_SENSORS];
	extern int sensorValuesRaw[NUM_SENSORS];

	extern int countReads;

	void Setup();

	void ReadSensorsRaw();
	void ReadSensorRaw(int i);

	void CalibrateSensors();
	void ReadSensorsSmooth();
	
	void PrintSensorsInterval();
	void PrintSensorsRaw();
	void PrintSensorsSmooth();

	bool HasWallAhead();

	bool HasWall(LED_INDEX index);
};

#endif
