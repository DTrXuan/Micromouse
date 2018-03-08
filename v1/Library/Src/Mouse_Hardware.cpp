#ifdef MOUSE_HARDWARE

#include "Mouse.hpp"
#include "Oled.hpp"
#include "Utils.hpp"
#include "math.h"
#include "stm32f4xx_hal.h"

extern ADC_HandleTypeDef hadc1;

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

int battery = 0;

void Mouse::Loop()
{
	Oled::Instance()->Draw(Oled::Page::Debug);
}

void Mouse::Setup()
{
	Oled::Instance()->Setup();
}

int Mouse::GetBattery()
{
	HAL_ADC_Start(&hadc1);
	if(HAL_ADC_PollForConversion(&hadc1, 1000) == HAL_OK)
	{
        int adc = HAL_ADC_GetValue(&hadc1);

        float vdd = 3.3f;
        int r1 = 20;
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
        battery = Utils::Map(adc, adcMin, adcMax, 0, 100);
	}
	HAL_ADC_Stop(&hadc1);

	return battery;
}
#endif
