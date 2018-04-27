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

	for(int i = 0; i < 4; i++)
	{
		int width = 5;
		int height = Mouse::Instance()->GetWallRead(i) / 30;

		u8g_DrawBox(&u8g, 128 - (i + 1) * (width + 1), 64 - height - 1, width, height);
	}

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
	u8g_InitComFn(&u8g, &u8g_dev_ssd1306_128x64_i2c, u8g_com_hw_i2c_fn);
	u8g_SetRot180(&u8g);
}
