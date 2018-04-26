#include <u8g_arm.hpp>
#include "Oled.hpp"
#include "Mouse.hpp"

u8g_t u8g;

Oled* Oled::instance = 0;

Oled* Oled::Instance()
{
    if (instance == 0)
    {
        instance = new Oled();
    }

    return instance;
}

Oled::Oled()
{

}

void DrawDebug()
{
	u8g_SetFont(&u8g, u8g_font_profont10r);
	u8g_DrawStr(&u8g,  2, 12, "Battery =");

	char batteryStr[5];
	snprintf(batteryStr, sizeof(batteryStr), "%d%%", Mouse::Instance()->GetBattery());
	u8g_DrawStr(&u8g,  50, 12, batteryStr);
}

void DrawLogo()
{
	u8g_SetFont(&u8g, u8g_font_profont10r);
	u8g_DrawStr(&u8g,  2, 12, "Bye World!");
}

void DrawMaze()
{

}

void DrawSettings()
{

}

void Oled::Draw(Page page)
{
	u8g_FirstPage(&u8g);
	do {
		switch(page)
		{
			case Page::Debug:
				DrawDebug();
				break;
			case Page::Logo:
				DrawLogo();
				break;
			case Page::Maze:
				DrawMaze();
				break;
			case Page::Settings:
				DrawSettings();
				break;
		}
	} while(u8g_NextPage(&u8g));

}

void Oled::Setup()
{
	u8g_InitComFn(&::u8g, &u8g_dev_ssd1306_128x64_i2c, u8g_com_hw_i2c_fn);
}
