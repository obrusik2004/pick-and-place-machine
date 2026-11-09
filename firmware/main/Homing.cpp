#include "Homing.h"
#include "Config.h"
#include "Steppers.h"

HomingManager homingManager;

void HomingManager::homeAllAxes() {
  digitalWrite(pins.enableX, LOW);
  digitalWrite(pins.enableY, LOW);
  digitalWrite(pins.enableZ, LOW);

  long xTravelSteps = 0;
  long yTravelSteps = 0;
  float xTravelMM = 0;
  float yTravelMM = 0;

  // Z-axis homing
  Serial.println("Homing Z-axis...");
  stepperZ.setMaxSpeed(1000);
  state.lastZDirection = 1;

  stepperZ.setSpeed(-1000);
  while (digitalRead(pins.limitZ1) != LOW) {
    stepperZ.runSpeed(); 
    if (state.emergencyStop) return; 
  }
  stepperZ.setCurrentPosition(0); currentPos.Z = 0;
  Serial.println("Z-axis homed!");

  // Y-axis homing
  Serial.println("Homing Y-axis...");
  stepperY.setMaxSpeed(1000);
  state.lastYDirection = -1;

  stepperY.setSpeed(-700);
  while (digitalRead(pins.limitY2) != HIGH) { 
    stepperY.runSpeed(); 
    if (state.emergencyStop) return; 
  }
  delay(100);
  stepperY.setCurrentPosition(0);

  state.lastYDirection = 1;
  stepperY.setSpeed(750);
  while (digitalRead(pins.limitY1) != HIGH) { 
    stepperY.runSpeed(); 
    if (state.emergencyStop) return; 
  }

  yTravelSteps = stepperY.currentPosition();
  yTravelMM = (float)yTravelSteps / STEPS_PER_MM_Y;
  stepperY.setCurrentPosition(0); currentPos.Y = 0;
  Serial.print("Y-axis homed! Travel: ");
  Serial.print(yTravelSteps);
  Serial.print(" steps = ");
  Serial.print(yTravelMM, 2);
  Serial.println(" mm");

  // X-axis homing
  Serial.println("Homing X-axis...");
  stepperX.setMaxSpeed(1600);
  state.lastXDirection = -1;

  stepperX.setSpeed(-1100);
  while (digitalRead(pins.limitX1) != HIGH) { 
    stepperX.runSpeed(); 
    if (state.emergencyStop) return; 
  }
  delay(100);
  stepperX.setCurrentPosition(0);

  state.lastXDirection = 1;
  stepperX.setSpeed(1100);
  while (digitalRead(pins.limitX2) != HIGH) { 
    stepperX.runSpeed(); 
    if (state.emergencyStop) return; 
  }

  xTravelSteps = stepperX.currentPosition();
  xTravelMM = (float)xTravelSteps /(float) STEPS_PER_MM_X;
  stepperX.setCurrentPosition(0); currentPos.X = 0;
  Serial.print("X-axis homed! Travel: ");
  Serial.print(xTravelSteps);
  Serial.print(" steps = ");
  Serial.print(xTravelMM, 2);
  Serial.println(" mm");

  // Area calculation
  float workspaceArea = xTravelMM * yTravelMM;
  Serial.print("Workspace Area = ");
  Serial.print(workspaceArea, 2);
  Serial.println(" mm²");

  // Finish
  state.isHomed = true;
  targetPos.X = 0; targetPos.Y = 0; targetPos.Z = 0; targetPos.R = 0;
  Serial.println("Homing completed successfully.");
}