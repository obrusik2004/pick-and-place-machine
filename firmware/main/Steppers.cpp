#include "Steppers.h"
#include "Config.h"

StepperManager stepperManager;

// Stepper objects
AccelStepper stepperY(AccelStepper::DRIVER, pins.pulseY, pins.dirY);
AccelStepper stepperX(AccelStepper::DRIVER, pins.pulseX, pins.dirX);
AccelStepper stepperZ(AccelStepper::DRIVER, pins.pulseZ, pins.dirZ);
AccelStepper stepperRot(AccelStepper::DRIVER, pins.pulseRot, pins.dirRot);

bool StepperManager::needsSafetyLift(long currentZ) {   // Scope Resolution Operator 
//-It indicates that need safetylift is a member function belonging to the StepperManager class.
    return currentZ > 7000;
}
bool StepperManager::isMoving() {
    // True if multi-move engine is active OR any axis still has steps remaining.
    return targetPos.moving ||
           stepperX.distanceToGo() != 0 ||
           stepperY.distanceToGo() != 0 ||
           stepperZ.distanceToGo() != 0 ||
           stepperRot.distanceToGo() != 0;
}


void StepperManager::initializeSteppers() {
    stepperX.setMaxSpeed(1200); stepperX.setAcceleration(1500);
    stepperY.setMaxSpeed(1650); stepperY.setAcceleration(1200);
    stepperZ.setMaxSpeed(5400); stepperZ.setAcceleration(5000);
    stepperRot.setMaxSpeed(620); stepperRot.setAcceleration(350);

    pinMode(pins.enableX, OUTPUT);
    pinMode(pins.enableY, OUTPUT);
    pinMode(pins.enableZ, OUTPUT);
    pinMode(pins.enableRot, OUTPUT);
    pinMode(pins.dirRot, OUTPUT);
    pinMode(pins.pump, OUTPUT);
    

    digitalWrite(pins.enableX, LOW);
    digitalWrite(pins.enableY, LOW);
    digitalWrite(pins.enableZ, LOW);
    digitalWrite(pins.enableRot, HIGH);
    digitalWrite(pins.pump, LOW);

    // making sure flag starts off
    procedureOverrideZReturn = false;
}

void StepperManager::setTargetPosition(String input) {
    input.toUpperCase();
  
    targetPos.X = currentPos.X;
    targetPos.Y = currentPos.Y;
    targetPos.Z = currentPos.Z;
    targetPos.R = currentPos.R;

    long xMove = parseAxisValue(input, 'X', STEPS_PER_MM_X);
    long yMove = parseAxisValue(input, 'Y', STEPS_PER_MM_Y);
    long zMove = parseAxisValue(input, 'Z', STEPS_PER_MM_Z);
    long rMove = parseAxisValue(input, 'R', 1);

    if (input.indexOf('X') != -1) targetPos.X += xMove;
    if (input.indexOf('Y') != -1) targetPos.Y += yMove;
    if (input.indexOf('Z') != -1) targetPos.Z += zMove;
    if (input.indexOf('R') != -1) targetPos.R += rMove;
    float z_mm = targetPos.Z / STEPS_PER_MM_Z;
    // Z limit
    if (input.indexOf('Z') != -1 && targetPos.Z > 12000 || z_mm > 60 ) {
        Serial.println("ERROR: Z-axis movement rejected! Maximum Z position is 11000 (55 mm)");
        targetPos.moving = false;
        return;
    }

    if (xMove != 0) state.lastXDirection = (xMove > 0) ? 1 : -1;
    if (yMove != 0) state.lastYDirection = (yMove > 0) ? 1 : -1;
    if (zMove != 0) state.lastZDirection = (zMove > 0) ? 1 : -1;

    targetPos.moving = true;

    // digitalWrite(pins.enableX, LOW);
    // digitalWrite(pins.enableY, LOW);
    // digitalWrite(pins.enableZ, LOW);
    digitalWrite(pins.enableRot, (rMove != 0) ? LOW : HIGH);

    stepperX.moveTo(targetPos.X);
    stepperY.moveTo(targetPos.Y);
    stepperZ.moveTo(targetPos.Z);
    stepperRot.moveTo(targetPos.R);

    Serial.println("Multi-axis move started: X" + String(targetPos.X) +
                   " Y" + String(targetPos.Y) +
                   " Z" + String(targetPos.Z) +
                   " R" + String(targetPos.R));
}

void StepperManager::runMultiMove() {
    if (!targetPos.moving || state.emergencyStop || state.autoRecoveryInProgress) return;
    
    static int  movePhase   = 0; // 0=check, 1=safety lift, 2=XY move, 3=Z return
    static long originalZ   = 0;
    static long userZMove   = 0;
    static bool hasXYMove   = false;
    static bool isPureZMove = false;
    
    if (movePhase == 0) {
        // Phase 0: Check if safety lift needed
        originalZ = currentPos.Z;
        userZMove = targetPos.Z - originalZ;
        hasXYMove = (stepperX.distanceToGo() != 0 || stepperY.distanceToGo() != 0);
        isPureZMove = (userZMove != 0) && !hasXYMove; // Only Z movement, no XY
        
        if (hasXYMove && needsSafetyLift(originalZ)) {
            movePhase = 1; // Need safety lift
            Serial.println("Safety: Z too low for XY move, lifting to safe height 1000");
        } else {
            movePhase = 2;
            
            if (isPureZMove) {
                Serial.println("Pure Z move - moving directly to target Z");
                stepperZ.moveTo(targetPos.Z);
            } else {
                Serial.println("Moving directly to all targets");
                stepperX.moveTo(targetPos.X);
                stepperY.moveTo(targetPos.Y);
                stepperZ.moveTo(targetPos.Z);
            }
            stepperRot.moveTo(targetPos.R);
        }
    }
    else if (movePhase == 1) {
        // Phase 1: Safety lift - Move Z UP to 6000 (only for XY moves)
        stepperZ.moveTo(6000);
        stepperZ.run();
        
        if (stepperZ.distanceToGo() == 0) {
            movePhase = 2; // Safety lift complete
            // set the final positions for XY movement (but keep Z at safe height)
            stepperX.moveTo(targetPos.X);
            stepperY.moveTo(targetPos.Y);
            stepperRot.moveTo(targetPos.R);
            Serial.println("Safety lift completed, moving XY");
        }
    } 
    else if (movePhase == 2) {
        // Phase 2: Move axes based on move type
        if (hasXYMove && needsSafetyLift(originalZ)) {
            // For safety moves: Move X and Y only (Z stays at safe height)
            stepperX.run();
            stepperY.run();
            stepperRot.run();
            
            if (stepperX.distanceToGo() == 0 &&
                stepperY.distanceToGo() == 0 &&
                stepperRot.distanceToGo() == 0) {

                // XY completed.
                if (!procedureOverrideZReturn) {
                    // MANUAL MODE (or default): return Z to original position
                    movePhase = 3;
                    stepperZ.moveTo(originalZ);
                    Serial.print("XY completed, returning Z to original position: ");
                    Serial.println(originalZ);
                } else {
                    // PROCEDURE MODE: keep Z at safety height (1000)
                    Serial.println("XY completed in procedure mode: keeping Z at safety height (1000)");
                    // No phase 3, Z already at 1000 and stays there
                    movePhase = 0; // allow final "all done" block to finish
                }
            }
        } else {
            // For direct moves: Move all axes to their final positions
            stepperX.run();
            stepperY.run();
            stepperZ.run();
            stepperRot.run();
        }
    }
    else if (movePhase == 3) {
        // Phase 3: Return Z to original position (only for safety moves in manual mode)
        stepperZ.run();
    }
    
    // Check if ALL movements are complete
    if (stepperX.distanceToGo() == 0 &&
        stepperY.distanceToGo() == 0 && 
        stepperZ.distanceToGo() == 0 &&
        stepperRot.distanceToGo() == 0) {
        
        currentPos.X = stepperX.currentPosition();
        currentPos.Y = stepperY.currentPosition();
        currentPos.Z = stepperZ.currentPosition();
        currentPos.R = stepperRot.currentPosition();
        
        targetPos.moving = false;
        movePhase = 0; // Reset for next movement
        digitalWrite(pins.enableRot, HIGH);
        
        if (hasXYMove && needsSafetyLift(originalZ)) {
            if (!procedureOverrideZReturn) {
                Serial.println("Safety move completed - Z returned to original position");
            } else {
                Serial.println("Safety move completed - Z kept at safe height (procedure mode)");
            }
        } else {
            Serial.println("Move completed");
        }
    }
}

void StepperManager::stopAllSteppers() {
    stepperX.stop();
    stepperY.stop();
    stepperZ.stop();
    stepperRot.stop();
}

long StepperManager::parseAxisValue(String input, char axis, float stepsPerMM) {  // Change to float
    if (input.indexOf(axis) == -1) return 0;

    int start = input.indexOf(axis) + 1;
    int end = getNextAxisIndex(input, start);
    String valueStr = input.substring(start, end);
    valueStr.trim();
    valueStr.toUpperCase();

    long value = 0;
    float mmValue = 0;

    if (valueStr.endsWith("MM")) {
        valueStr.remove(valueStr.length() - 2);
        mmValue = valueStr.toFloat();  
        value = round(mmValue * stepsPerMM);  // Round to nearest integer
    } else {
        value = valueStr.toInt();
    }

    return value;
}
int StepperManager::getNextAxisIndex(String input, int start) {
    int nextX = input.indexOf('X', start);
    int nextY = input.indexOf('Y', start);
    int nextZ = input.indexOf('Z', start);
    int nextR = input.indexOf('R', start);
  
    int nextIndex = input.length();
    if (nextX != -1 && nextX < nextIndex) nextIndex = nextX;
    if (nextY != -1 && nextY < nextIndex) nextIndex = nextY;
    if (nextZ != -1 && nextZ < nextIndex) nextIndex = nextZ;
    if (nextR != -1 && nextR < nextIndex) nextIndex = nextR;
  
    return nextIndex;
}
