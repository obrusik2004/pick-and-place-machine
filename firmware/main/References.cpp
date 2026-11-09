#include "References.h"
#include "Config.h"
#include "Steppers.h"
#include "Limits.h"

ReferenceManager referenceManager;

void ReferenceManager::setPCBReference() {
  pcbRef.X = currentPos.X;
  pcbRef.Y = currentPos.Y;
  pcbRef.Z = currentPos.Z;
  pcbRef.R = currentPos.R;
  pcbRef.isSet = true;
  
  Serial.println("=== PCB REFERENCE POSITION SET ===");
  Serial.print("X:"); Serial.print(pcbRef.X);
  Serial.print(" Y:"); Serial.print(pcbRef.Y);
  Serial.print(" Z:"); Serial.print(pcbRef.Z);
  Serial.print(" R:"); Serial.println(pcbRef.R);
  Serial.println("==================================");
  delay(100);
}

void ReferenceManager::setPickReference() {
  pickRef.X = currentPos.X;
  pickRef.Y = currentPos.Y;
  pickRef.Z = currentPos.Z;
  pickRef.R = currentPos.R;
  pickRef.isSet = true;
  
  Serial.println("=== PICK REFERENCE POSITION SET ===");
  Serial.print("X:"); Serial.print(pickRef.X);
  Serial.print(" Y:"); Serial.print(pickRef.Y);
  Serial.print(" Z:"); Serial.print(pickRef.Z);
  Serial.print(" R:"); Serial.println(pickRef.R);
  Serial.println("===================================");
  delay(100);
}

void ReferenceManager::pickComponent() {
  if (!pickRef.isSet) {
    Serial.println("ERROR: Pick reference not set! Use SETPICK first.");
    return;
  }
  
  if (pickRef.Z > 120000) {
    Serial.println("ERROR: Pick Z position exceeds maximum limit of 12000!");
    return;
  }
  
  Serial.println("Starting pick operation...");
  moveToPickPosition();
  
  if (state.emergencyStop) {
    Serial.println("Pick operation interrupted by emergency stop!");
    return;
  }
  
  digitalWrite(pins.pump, HIGH);
  Serial.println("PUMP ON - Component picked!");
  delay(500);
  stepperZ.move(-6000);
  Serial.println("Pick operation completed successfully!");
}

void ReferenceManager::moveToPickPosition() {
  digitalWrite(pins.enableX, LOW);
  digitalWrite(pins.enableY, LOW);
  digitalWrite(pins.enableZ, LOW);
  digitalWrite(pins.enableRot, LOW);
  
  targetPos.X = pickRef.X;
  targetPos.Y = pickRef.Y;
  targetPos.Z = pickRef.Z;
  targetPos.R = pickRef.R;
  targetPos.moving = true;
  
  stepperX.moveTo(targetPos.X);
  stepperY.moveTo(targetPos.Y);
  stepperZ.moveTo(targetPos.Z);
  stepperRot.moveTo(targetPos.R);
  
  Serial.println("Moving to ABSOLUTE pick position...");
  Serial.print("Moving to X:"); Serial.print(targetPos.X);
  Serial.print(" Y:"); Serial.print(targetPos.Y);
  Serial.print(" Z:"); Serial.print(targetPos.Z);
  Serial.print(" R:"); Serial.println(targetPos.R);
  
  while (targetPos.moving && !state.emergencyStop) {
    stepperManager.runMultiMove();
    limitManager.checkLimitsDuringMove();
    currentPos.X = stepperX.currentPosition();
    currentPos.Y = stepperY.currentPosition();
    currentPos.Z = stepperZ.currentPosition();
    currentPos.R = stepperRot.currentPosition();
    
    if (stepperX.distanceToGo() == 0 && stepperY.distanceToGo() == 0 && 
        stepperZ.distanceToGo() == 0 && stepperRot.distanceToGo() == 0) {
      targetPos.moving = false;
    }
  }
}