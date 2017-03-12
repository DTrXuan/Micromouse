#include <Arduino.h>
#include "WallSensors.h"
#include "Config.h"

int WallSensors::sensorsCalibrationMin[NUM_SENSORS];
int WallSensors::sensorsCalibrationMax[NUM_SENSORS];

int WallSensors::sensorValuesWindow[NUM_SENSORS][READS_WINDOW_MAX_SIZE];
int WallSensors::sensorValuesSmooth[NUM_SENSORS];
int WallSensors::sensorValuesRaw[NUM_SENSORS];

int WallSensors::countReads = 0;
int WallSensors::sensorWindowRealSize = 0;

void WallSensors::Setup()
{
	pinMode(LED_LF, OUTPUT);
	pinMode(LED_DL, OUTPUT);
	pinMode(LED_DR, OUTPUT);
	pinMode(LED_RF, OUTPUT);

	digitalWrite(LED_LF, LOW);
	digitalWrite(LED_DL, LOW);
	digitalWrite(LED_DR, LOW);
	digitalWrite(LED_RF, LOW);
}

void WallSensors::ReadSensorsRaw()
{
	for (int i = 0; i < NUM_SENSORS; i++)
	{
		ReadSensorRaw(i);
	}
}

void WallSensors::ReadSensorRaw(int i)
{
	digitalWrite(SensorsPairs[i][0], LOW);
	int sensorDark = analogRead(SensorsPairs[i][1]);
	digitalWrite(SensorsPairs[i][0], HIGH);

	delayMicroseconds(Constants::SENSOR_READ_MICROSECONDS);

	int sensorLight = analogRead(SensorsPairs[i][1]);
	digitalWrite(SensorsPairs[i][0], LOW);
	sensorValuesRaw[i] = sensorLight - sensorDark;
}

void WallSensors::PrintSensorsRaw()
{
	for (int i = 0; i < NUM_SENSORS; i++)
	{
		Serial.print("|");
		Serial.print(sensorValuesRaw[i]);
	}

	Serial.println("|");
}

void WallSensors::CalibrateSensors()
{
	Serial.println("Calibrating sensors...");
	analogWrite(LED_BUILTIN, 100);

	long startTime = millis();

	for (int i = 0; i < NUM_SENSORS; i++)
	{
		sensorsCalibrationMin[i] = 1023;
		sensorsCalibrationMax[i] = 0;
	}

	while (millis() - startTime < CALIBRATION_SECONDS * 1000)
	{
		ReadSensorsRaw();

		for (int i = 0; i < NUM_SENSORS; i++)
		{
			int sensorValue = sensorValuesRaw[i];
			sensorsCalibrationMin[i] = min(sensorsCalibrationMin[i], sensorValue);
			sensorsCalibrationMax[i] = max(sensorsCalibrationMax[i], sensorValue);
		}

		delay(10);
	}

	analogWrite(LED_BUILTIN, 0);
	Serial.println("Sensors calibrated!");
	delay(1000);
}

void WallSensors::PrintSensorsInterval()
{
	for (int i = 0; i < NUM_SENSORS; i++)
	{
		Serial.print("|");
		Serial.print(sensorsCalibrationMin[i]);
		Serial.print(" , ");
		Serial.print(sensorsCalibrationMax[i]);
	}

	Serial.println("|");
}

void WallSensors::ReadSensorsSmooth()
{
	ReadSensorsRaw();

	int readIndex = countReads % READS_WINDOW_MAX_SIZE;

	if (sensorWindowRealSize < READS_WINDOW_MAX_SIZE)
		sensorWindowRealSize++;

	countReads++;
	if (countReads == READS_WINDOW_MAX_SIZE)
		countReads = 0;

	for (int i = 0; i < NUM_SENSORS; i++)
	{
		sensorValuesSmooth[i] = 0;
		sensorsCalibrationMin[i] = min(sensorsCalibrationMin[i], sensorValuesRaw[i]);
		sensorsCalibrationMax[i] = max(sensorsCalibrationMax[i], sensorValuesRaw[i]);

		sensorValuesWindow[i][readIndex] = map(sensorValuesRaw[i], sensorsCalibrationMin[i], sensorsCalibrationMax[i], 0, 255);
		sensorValuesWindow[i][readIndex] = constrain(sensorValuesWindow[i][readIndex], 0, 255);

		for (int r = 0; r < sensorWindowRealSize; r++)
			sensorValuesSmooth[i] += sensorValuesWindow[i][r];

		sensorValuesSmooth[i] /= sensorWindowRealSize;
	}
}

void WallSensors::PrintSensorsSmooth()
{
	for (int i = 0; i < NUM_SENSORS; i++)
	{
		Serial.print("|");
		Serial.print(sensorValuesSmooth[i]);
	}

	Serial.println("|");
}

bool WallSensors::HasWallAhead()
{
	int threshold = 50;

	if (sensorValuesSmooth[LED_INDEX_LF] > threshold || sensorValuesSmooth[LED_INDEX_RF] > threshold)
		return true;

	return false;
}
