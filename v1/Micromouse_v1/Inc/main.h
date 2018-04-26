/**
  ******************************************************************************
  * File Name          : main.hpp
  * Description        : This file contains the common defines of the application
  ******************************************************************************
  ** This notice applies to any and all portions of this file
  * that are not between comment pairs USER CODE BEGIN and
  * USER CODE END. Other portions of this file, whether 
  * inserted by the user or by software development tools
  * are owned by their respective copyright owners.
  *
  * COPYRIGHT(c) 2018 STMicroelectronics
  *
  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  *   1. Redistributions of source code must retain the above copyright notice,
  *      this list of conditions and the following disclaimer.
  *   2. Redistributions in binary form must reproduce the above copyright notice,
  *      this list of conditions and the following disclaimer in the documentation
  *      and/or other materials provided with the distribution.
  *   3. Neither the name of STMicroelectronics nor the names of its contributors
  *      may be used to endorse or promote products derived from this software
  *      without specific prior written permission.
  *
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
  * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
  * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
  * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
  * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
  * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
  * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
  * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  *
  ******************************************************************************
  */
/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __MAIN_H
#define __MAIN_H
  /* Includes ------------------------------------------------------------------*/

/* Includes ------------------------------------------------------------------*/
/* USER CODE BEGIN Includes */

/* USER CODE END Includes */

/* Private define ------------------------------------------------------------*/

#define RCV_FL_Pin GPIO_PIN_0
#define RCV_FL_GPIO_Port GPIOC
#define RCV_L_Pin GPIO_PIN_1
#define RCV_L_GPIO_Port GPIOC
#define RCV_R_Pin GPIO_PIN_2
#define RCV_R_GPIO_Port GPIOC
#define RCV_FR_Pin GPIO_PIN_3
#define RCV_FR_GPIO_Port GPIOC
#define V_METER_Pin GPIO_PIN_1
#define V_METER_GPIO_Port GPIOA
#define STATUS_Pin GPIO_PIN_2
#define STATUS_GPIO_Port GPIOA
#define ENC_CS_Pin GPIO_PIN_4
#define ENC_CS_GPIO_Port GPIOA
#define ENC_CLK_Pin GPIO_PIN_5
#define ENC_CLK_GPIO_Port GPIOA
#define MOTOR_L_IN2_Pin GPIO_PIN_6
#define MOTOR_L_IN2_GPIO_Port GPIOA
#define MOTOR_L_IN1_Pin GPIO_PIN_7
#define MOTOR_L_IN1_GPIO_Port GPIOA
#define MOTOR_R_IN2_Pin GPIO_PIN_4
#define MOTOR_R_IN2_GPIO_Port GPIOC
#define MOTOR_R_IN1_Pin GPIO_PIN_5
#define MOTOR_R_IN1_GPIO_Port GPIOC
#define ENC_LEFT_DO_Pin GPIO_PIN_10
#define ENC_LEFT_DO_GPIO_Port GPIOB
#define ENC_RIGHT_DO_Pin GPIO_PIN_11
#define ENC_RIGHT_DO_GPIO_Port GPIOB
#define EMIT_FR_Pin GPIO_PIN_12
#define EMIT_FR_GPIO_Port GPIOB
#define EMIT_R_Pin GPIO_PIN_13
#define EMIT_R_GPIO_Port GPIOB
#define EMIT_L_Pin GPIO_PIN_14
#define EMIT_L_GPIO_Port GPIOB
#define EMIT_FL_Pin GPIO_PIN_15
#define EMIT_FL_GPIO_Port GPIOB
#define BUZZER_Pin GPIO_PIN_6
#define BUZZER_GPIO_Port GPIOC
#define MOTOR_R_PWM_Pin GPIO_PIN_8
#define MOTOR_R_PWM_GPIO_Port GPIOA
#define FTDI_RX_Pin GPIO_PIN_9
#define FTDI_RX_GPIO_Port GPIOA
#define FTDI_TX_Pin GPIO_PIN_10
#define FTDI_TX_GPIO_Port GPIOA
#define MOTOR_L_PWM_Pin GPIO_PIN_11
#define MOTOR_L_PWM_GPIO_Port GPIOA
#define BT_RX_Pin GPIO_PIN_10
#define BT_RX_GPIO_Port GPIOC
#define BT_TX_Pin GPIO_PIN_11
#define BT_TX_GPIO_Port GPIOC
#define I2C_SCL_Pin GPIO_PIN_6
#define I2C_SCL_GPIO_Port GPIOB
#define I2C_SDA_Pin GPIO_PIN_7
#define I2C_SDA_GPIO_Port GPIOB

/* ########################## Assert Selection ############################## */
/**
  * @brief Uncomment the line below to expanse the "assert_param" macro in the 
  *        HAL drivers code
  */
/* #define USE_FULL_ASSERT    1U */

/* USER CODE BEGIN Private defines */

/* USER CODE END Private defines */

#ifdef __cplusplus
 extern "C" {
#endif
void _Error_Handler(char *, int);

#define Error_Handler() _Error_Handler(__FILE__, __LINE__)
#ifdef __cplusplus
}
#endif

/**
  * @}
  */ 

/**
  * @}
*/ 

#endif /* __MAIN_H */
/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
