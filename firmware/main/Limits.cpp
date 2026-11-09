#include "Limits.h"
#include "Config.h"
#include "Emergency.h"

LimitManager limitManager;

void LimitManager::initializeLimitPins() {
  pinMode(pins.limitY1, INPUT);
  pinMode(pins.limitY2, INPUT);
  pinMode(pins.limitX1, INPUT);
  pinMode(pins.limitX2, INPUT);
  pinMode(pins.limitZ1, INPUT);
}

void LimitManager::checkLimitsDuringMove() {
  if (state.autoRecoveryInProgress) return;

  if (stepperX.isRunning()) {
    if ((digitalRead(pins.limitX1) == HIGH && state.lastXDirection == -1) ||
        (digitalRead(pins.limitX2) == HIGH && state.lastXDirection == 1)) {
      Serial.println(" X-axis LIMIT TRIGGERED -> HARD STOP");
      immediateAxisStop(stepperX, pins.enableX);
      recoverFromLimitX();
    }
  }

  if (stepperY.isRunning()) {
    if ((digitalRead(pins.limitY1) == HIGH && state.lastYDirection == 1) ||
        (digitalRead(pins.limitY2) == HIGH && state.lastYDirection == -1)) {
      Serial.println(" Y-axis LIMIT TRIGGERED -> HARD STOP");
      immediateAxisStop(stepperY, pins.enableY);
      recoverFromLimitY();
    }
  }

  if (stepperZ.isRunning()) {
    if ((digitalRead(pins.limitZ1) == LOW && state.lastZDirection == -1)) {
      Serial.println(" Z-axis LIMIT TRIGGERED -> HARD STOP");
      immediateAxisStop(stepperZ, pins.enableZ);
      recoverFromLimitZ();
    }
  }
}

void LimitManager::recoverFromLimitX() {
  state.autoRecoveryInProgress = true;
  Serial.println("X-axis recovery started...");

  int recoveryDirection = 0;
  int approachDirection = 0;
  bool isHomingLimit = false;

  if (digitalRead(pins.limitX1) == HIGH && state.lastXDirection == -1) {
    recoveryDirection = 1;
    approachDirection = -1;
    isHomingLimit = false;
    Serial.println("Recovering from limitX1...");
  } 
  else if (digitalRead(pins.limitX2) == HIGH && state.lastXDirection == 1) {
    recoveryDirection = -1;
    approachDirection = 1;
    isHomingLimit = true;
    Serial.println("Recovering from limitX2 (X=0)...");
  } else {
    state.autoRecoveryInProgress = false;
    Serial.println("No X limit active, recovery aborted.");
    return;
  }

  digitalWrite(pins.enableX, LOW);

  // Phase 1: Back off fixed distance WITH SERIAL PROCESSING
  stepperX.move(300 * recoveryDirection); 
  while (stepperX.distanceToGo() != 0) {
    stepperX.run();
    
    if (Serial.available() > 0) {
      String input = Serial.readString();
      input.trim();
      if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        return;
      }
    }
    
    if (state.emergencyStop) {
      state.autoRecoveryInProgress = false;
      return;
    }
  }

  delay(200);

  // Phase 2: Approach limit again slowly WITH SERIAL PROCESSING
  stepperX.setSpeed(approachDirection * 350);
  while (true) {
    if ((approachDirection == -1 && digitalRead(pins.limitX1) == HIGH) ||
        (approachDirection ==  1 && digitalRead(pins.limitX2) == HIGH)) {
      stepperX.stop();
      
      if (isHomingLimit) {
        stepperX.setCurrentPosition(0);
        currentPos.X = 0;
        Serial.println("X position RESET TO 0 at homing limit");
      }
      break;
    }
    stepperX.runSpeed();
    
    if (Serial.available() > 0) {
      String input = Serial.readString();
      input.trim();
      if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        return;
      }
    }
    
    if (state.emergencyStop) {
      state.autoRecoveryInProgress = false;
      return;
    }
  }

  while (stepperX.isRunning()) stepperX.run();
  state.autoRecoveryInProgress = false;
  digitalWrite(pins.enableX, LOW);
  
  Serial.print("X-axis recovery completed. Final position: ");
  Serial.println(stepperX.currentPosition());
}

void LimitManager::recoverFromLimitY() {
  state.autoRecoveryInProgress = true;
  Serial.println("Y-axis recovery started...");

  int recoveryDirection = 0;
  int approachDirection = 0;
  bool isHomingLimit = false;

  if (digitalRead(pins.limitY1) == HIGH && state.lastYDirection == 1) {
    recoveryDirection = -1;
    approachDirection = 1;
    isHomingLimit = true;
    Serial.println("Recovering from limitY1 (Y=0)...");
  } 
  else if (digitalRead(pins.limitY2) == HIGH && state.lastYDirection == -1) {
    recoveryDirection = 1;
    approachDirection = -1;
    isHomingLimit = false;
    Serial.println("Recovering from limitY2...");
  } else {
    state.autoRecoveryInProgress = false;
    Serial.println("No Y limit active, recovery aborted.");
    return;
  }

  digitalWrite(pins.enableY, LOW);

  // Phase 1: Back off fixed distance WITH SERIAL PROCESSING
  stepperY.move(350 * recoveryDirection); 
  while (stepperY.distanceToGo() != 0) {
    stepperY.run();
    
    if (Serial.available() > 0) {
      String input = Serial.readString();
      input.trim();
      if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        state.autoRecoveryInProgress = false;
        return;
      }
    }
    
    if (state.emergencyStop) {
      state.autoRecoveryInProgress = false;
      return;
    }
  }

  delay(200);

  // Phase 2: Approach limit again slowly WITH SERIAL PROCESSING
  stepperY.setSpeed(approachDirection * 350); 
  while (true) {
    if ((approachDirection == 1  && digitalRead(pins.limitY1) == HIGH) ||
        (approachDirection == -1 && digitalRead(pins.limitY2) == HIGH)) {
      stepperY.stop();
      
      if (isHomingLimit) {
        stepperY.setCurrentPosition(0);
        currentPos.Y = 0;
        Serial.println("Y position RESET TO 0 at homing limit");
      }
      break;
    }
    stepperY.runSpeed();
    
    if (Serial.available() > 0) {
      String input = Serial.readString();
      input.trim();
      if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        state.autoRecoveryInProgress = false;
        return;
      }
    }
    
    if (state.emergencyStop) {
      state.autoRecoveryInProgress = false;
      return;
    }
  }

  while (stepperY.isRunning()) stepperY.run();
  state.autoRecoveryInProgress = false;
  digitalWrite(pins.enableY, LOW);
  
  Serial.print("Y-axis recovery completed. Final position: ");
  Serial.println(stepperY.currentPosition());
}

void LimitManager::recoverFromLimitZ() {
  state.autoRecoveryInProgress = true;
  Serial.println("Z-axis recovery started...");

  digitalWrite(pins.enableZ, LOW);

  // Phase 1: Move down (away from limit)
  stepperZ.move(200);
  while (stepperZ.distanceToGo() != 0) {
    stepperZ.run();
    
    if (Serial.available() > 0) {
      String input = Serial.readString();
      input.trim();
      if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        state.autoRecoveryInProgress = false;
        return;
      }
    }
    
    if (state.emergencyStop) {
      state.autoRecoveryInProgress = false;
      return;
    }
  }

  delay(200);

  // Phase 2: Move UP slowly until bottom limit switch triggers
  stepperZ.setSpeed(-300);
  while (true) {
    if (digitalRead(pins.limitZ1) == LOW) {
      stepperZ.stop();
      stepperZ.setCurrentPosition(0);
      currentPos.Z = 0;
      Serial.println("Z position RESET TO 0 at homing limit");
      break;
    }
    stepperZ.runSpeed();
    
    if (Serial.available() > 0) {
      String input = Serial.readString();
      input.trim();
      if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        state.autoRecoveryInProgress = false;
        return;
      }
    }
    
    if (state.emergencyStop) {
      state.autoRecoveryInProgress = false;
      return;
    }
  }

  while (stepperZ.isRunning()) stepperZ.run();
  state.autoRecoveryInProgress = false;
  digitalWrite(pins.enableZ, LOW);
  Serial.println("Z-axis recovery completed.");
}

void LimitManager::immediateAxisStop(AccelStepper &stepper, int enablePin) {
  digitalWrite(enablePin, HIGH);
  stepper.stop();
  stepper.setCurrentPosition(stepper.currentPosition());
  delay(50);
}

void LimitManager::processRecoveryWithSerial() {
}