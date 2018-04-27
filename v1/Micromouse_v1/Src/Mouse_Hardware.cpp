#ifdef MOUSE_HARDWARE

#include "Mouse.hpp"
#include "Maze.hpp"
#include "Oled.hpp"
#include "Utils.hpp"
#include "math.h"
#include "stm32f4xx_hal.h"
#include "dwt_stm32_delay.h"

extern ADC_HandleTypeDef hadc1;
extern ADC_HandleTypeDef hadc2;

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

int lastBatteryUpdate = -10000;

void Mouse::Loop()
{
	if(GetBattery())
	  HAL_GPIO_WritePin(GPIOA, STATUS_Pin, GPIO_PIN_SET);

	GetWallSensors();

	Oled::Instance()->Draw(Oled::Page::Debug);
}

void Mouse::Setup()
{
	Oled::Instance()->Setup();
}

int Mouse::GetBattery()
{
	int newBatteryUpdate = HAL_GetTick();

	// Update once every 10 seconds.
	if(newBatteryUpdate - lastBatteryUpdate < 10000)
		return _battery;

	lastBatteryUpdate = newBatteryUpdate;

	HAL_ADC_Start(&hadc1);
	if(HAL_ADC_PollForConversion(&hadc1, 100) == HAL_OK)
	{
        int adc = HAL_ADC_GetValue(&hadc1);

        float vdd = 3.3f;
        int r1 = 18;
        int r2 = 10;
        float divider = (float) r2 / (r1 + r2);
        float vInMax = 8.4f;
        float vInMin = 6.0f;
        float vOutMax = vInMax * divider;
        float vOutMin = vInMin * divider;

        int exponent = 0;
        switch(hadc1.Init.Resolution)
        {
			case ADC_RESOLUTION_6B:
				exponent = 6;
				break;
			case ADC_RESOLUTION_8B:
				exponent = 8;
				break;
			case ADC_RESOLUTION_10B:
				exponent = 10;
				break;
			case ADC_RESOLUTION_12B:
				exponent = 12;
				break;
        }

        int adcRealMax = pow(2, exponent);
        int adcMax = vOutMax / vdd * adcRealMax;
        int adcMin = vOutMin / vdd * adcRealMax;

        // TODO(Pedro): this value should be an average of N consecutive reads
        _battery = Utils::Map(adc, adcMin, adcMax, 0, 100);
	}
	HAL_ADC_Stop(&hadc1);

	return _battery;
}

void Mouse::GetWallSensors()
{
	_currentDetection = (_currentDetection + 1) % _detectionWindowSize;

	HAL_GPIO_WritePin(GPIOB, EMIT_FR_Pin|EMIT_R_Pin|EMIT_L_Pin|EMIT_FL_Pin, GPIO_PIN_SET);
	HAL_ADC_Start(&hadc2);
	for(int i = 0; i < 4; i++)
	{
		if(HAL_ADC_PollForConversion(&hadc2, 100) == HAL_OK)
		{
			int adc = HAL_ADC_GetValue(&hadc2);

			__detectionValues[i][_currentDetection] = adc;
		}
	}
	HAL_ADC_Stop(&hadc2);
	HAL_GPIO_WritePin(GPIOB, EMIT_FR_Pin|EMIT_R_Pin|EMIT_L_Pin|EMIT_FL_Pin, GPIO_PIN_RESET);
}

int Mouse::GetWallRead(int index)
{
	return __detectionValues[index][_currentDetection];
}

#endif
