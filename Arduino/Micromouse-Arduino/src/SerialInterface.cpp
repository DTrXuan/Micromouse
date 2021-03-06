﻿#include <Arduino.h>
#include "SerialInterface.h"
#include "Config.h"
#include "Mouse.h"

void SerialInterface::Read()
{
	if (!Constants::connectedToSerial)
	{
		return;
	}

	int incomingBytes = Serial1.available();
	if (incomingBytes <= 0)
	{
		return;
	}

	char* command = (char*)malloc(incomingBytes * sizeof(byte));
	int readBytes = Serial1.readBytes(command, incomingBytes);

	//Serial.print("Mouse received: ");
	//Serial.println(command);

	switch (command[0])
	{
	case 'K':
		digitalWrite(LED_BUILTIN, LOW);
		break;

	case 'L':
		digitalWrite(LED_BUILTIN, HIGH);
		break;

	case 'P':
		Variables::forcedPause = !Variables::forcedPause;
		break;

	case '+':
		Mouse::_stepperPulseDelayUs += 1;
		Mouse::stepperLeft.setMinPulseWidth(Mouse::_stepperPulseDelayUs);
		Mouse::stepperRight.setMinPulseWidth(Mouse::_stepperPulseDelayUs);
		Serial.println(Mouse::_stepperPulseDelayUs);
		break;

	case '-':
		Mouse::_stepperPulseDelayUs -= 1;
		Mouse::stepperLeft.setMinPulseWidth(Mouse::_stepperPulseDelayUs);
		Mouse::stepperRight.setMinPulseWidth(Mouse::_stepperPulseDelayUs);
		Serial.println(Mouse::_stepperPulseDelayUs);
		break;
	}

	free(command);
}