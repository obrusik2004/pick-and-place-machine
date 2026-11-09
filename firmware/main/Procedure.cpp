#include "Procedure.h"
#include "Config.h"
#include "Steppers.h"
#include "Limits.h"
#include "Feeder.h"
#include "VisionSystem.h"
#include <EEPROM.h>

ProcedureManager procedureManager;

// ---- CONFIG ----
static const long Z_TRAVEL_HEIGHT     = 2000;  // steps
static const long Z_INSPECTION_HEIGHT = 6500;  // steps

// Camera inspection offsets (calibratable)
long INSPECTION_OFFSET_STEPS_X = 2230;  // default
long INSPECTION_OFFSET_STEPS_Y = 38;    // default

static const int EEPROM_OFFSET_X_ADDR = 0;
static const int EEPROM_OFFSET_Y_ADDR = sizeof(long);

// ----------------- helpers -----------------
static void syncCurrentPosFromSteppers() {
    currentPos.X = stepperX.currentPosition();
    currentPos.Y = stepperY.currentPosition();
    currentPos.Z = stepperZ.currentPosition();
    currentPos.R = stepperRot.currentPosition();
}

static bool eepromOffsetLooksValid(long v) {
    return (v > -20000 && v < 20000);
}

// Z safety helper for camera moves
static bool ensureZAtInspectionHeightBeforeCamera() {
    long currentZ = stepperZ.currentPosition();

    // So if currentZ > Z_INSPECTION_HEIGHT, we need to retract up to inspection height
    if (currentZ > Z_INSPECTION_HEIGHT) {
        Serial.print("Moving Z from ");
        Serial.print(currentZ);
        Serial.print(" to inspection height: ");
        Serial.println(Z_INSPECTION_HEIGHT);

        stepperZ.moveTo(Z_INSPECTION_HEIGHT);

        unsigned long start = millis();
        while (stepperZ.distanceToGo() != 0 && (millis() - start < 30000)) {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();
            delay(1);
        }

        if (stepperZ.distanceToGo() != 0) {
            Serial.println("ERROR: Z move timeout during camera move");
            return false;
        }

        Serial.println("Z now at safe inspection height");
        return true;
    }

    Serial.println("Z already at safe inspection height");
    return true;
}

// Guard flags to prevent re-triggering actions every loop
static bool pickDoneThisCycle  = false;
static bool placeDoneThisCycle = false;

// ============ INIT ============
void ProcedureManager::initialize() {
    long savedX = 0, savedY = 0;
    EEPROM.get(EEPROM_OFFSET_X_ADDR, savedX);
    EEPROM.get(EEPROM_OFFSET_Y_ADDR, savedY);

    if (eepromOffsetLooksValid(savedX)) INSPECTION_OFFSET_STEPS_X = savedX;
    if (eepromOffsetLooksValid(savedY)) INSPECTION_OFFSET_STEPS_Y = savedY;

    Serial.print("Inspection offsets loaded: X=");
    Serial.print(INSPECTION_OFFSET_STEPS_X);
    Serial.print(" Y=");
    Serial.println(INSPECTION_OFFSET_STEPS_Y);

    // Initialize correction delta (one iteration)
    corrDeltaX = 0;
    corrDeltaY = 0;
    corrDeltaR = 0;

    // Initialize cumulative correction (sum for current component)
    cumulativeCorrX = 0;
    cumulativeCorrY = 0;
    cumulativeCorrR = 0;

    syncCurrentPosFromSteppers();
    calibrationState.isCalibrating = false;

    pickDoneThisCycle  = false;
    placeDoneThisCycle = false;

    visionSystem.initialize();
}

// ============ CALIBRATION ============
void ProcedureManager::startCalibration() {
    if (!state.isHomed) {
        Serial.println("ERROR: Machine not homed");
        return;
    }
    if (procedureState.active) {
        Serial.println("ERROR: Procedure active. ABORT first.");
        return;
    }

    syncCurrentPosFromSteppers();

    calibrationState.isCalibrating    = true;
    calibrationState.nozzlePosX       = currentPos.X;
    calibrationState.nozzlePosY       = currentPos.Y;

    calibrationState.originalOffsetX  = INSPECTION_OFFSET_STEPS_X;
    calibrationState.originalOffsetY  = INSPECTION_OFFSET_STEPS_Y;
    calibrationState.currentOffsetX   = INSPECTION_OFFSET_STEPS_X;
    calibrationState.currentOffsetY   = INSPECTION_OFFSET_STEPS_Y;

    Serial.println("CAMERA_CALIBRATION_STARTED");
    Serial.print("Current offsets: X=");
    Serial.print(INSPECTION_OFFSET_STEPS_X);
    Serial.print(" Y=");
    Serial.println(INSPECTION_OFFSET_STEPS_Y);

    Serial.println("Commands:");
    Serial.println("MOVE_TO_CAMERA");
    Serial.println("CX:value   (relative adjust X by value steps)");
    Serial.println("CY:value   (relative adjust Y by value steps)");
    Serial.println("RECALIBRATE (save)");
    Serial.println("CANCEL");
}

void ProcedureManager::handleCalibrationCommand(String command) {
    if (!calibrationState.isCalibrating) return;

    command.trim();
    command.toUpperCase();

    if (procedureState.active) {
        Serial.println("ERROR: Procedure active during calibration (unexpected).");
        return;
    }

    syncCurrentPosFromSteppers();

    // ---------------- MOVE_TO_CAMERA ----------------
    if (command.equalsIgnoreCase("MOVE_TO_CAMERA")) {
        if (!ensureZAtInspectionHeightBeforeCamera()) {
            Serial.println("ERROR: Failed to move Z to safe height");
            return;
        }

        long offsetX = calibrationState.currentOffsetX;
        long offsetY = calibrationState.currentOffsetY;

        state.lastXDirection = (offsetX > 0) ? 1 : -1;
        state.lastYDirection = (offsetY > 0) ? 1 : -1;

        stepperX.move(offsetX);
        stepperY.move(offsetY);

        Serial.print("Moving to camera (relative) X=");
        Serial.print(offsetX);
        Serial.print(" Y=");
        Serial.println(offsetY);

        unsigned long start = millis();
        while ((stepperX.distanceToGo() != 0 || stepperY.distanceToGo() != 0) &&
               (millis() - start < 30000)) {
            stepperX.run();
            stepperY.run();
            limitManager.checkLimitsDuringMove();
            delay(1);
        }

        syncCurrentPosFromSteppers();
        Serial.println("MOVE_TO_CAMERA_DONE");
        return;
    }

    // ---------------- CX ----------------
    if (command.startsWith("CX:")) {
        if (!ensureZAtInspectionHeightBeforeCamera()) {
            Serial.println("ERROR: Failed to move Z to safe height");
            return;
        }

        long adj = command.substring(3).toInt();
        calibrationState.currentOffsetX = calibrationState.originalOffsetX + adj;

        stepperX.move(adj);

        unsigned long start = millis();
        while (stepperX.distanceToGo() != 0 && (millis() - start < 30000)) {
            stepperX.run();
            limitManager.checkLimitsDuringMove();
            delay(1);
        }

        syncCurrentPosFromSteppers();
        Serial.print("CX updated -> offsetX=");
        Serial.println(calibrationState.currentOffsetX);
        return;
    }

    // ---------------- CY ----------------
    if (command.startsWith("CY:")) {
        if (!ensureZAtInspectionHeightBeforeCamera()) {
            Serial.println("ERROR: Failed to move Z to safe height");
            return;
        }

        long adj = command.substring(3).toInt();
        calibrationState.currentOffsetY = calibrationState.originalOffsetY + adj;

        stepperY.move(adj);

        unsigned long start = millis();
        while (stepperY.distanceToGo() != 0 && (millis() - start < 30000)) {
            stepperY.run();
            limitManager.checkLimitsDuringMove();
            delay(1);
        }

        syncCurrentPosFromSteppers();
        Serial.print("CY updated -> offsetY=");
        Serial.println(calibrationState.currentOffsetY);
        return;
    }

    // ---------------- RECALIBRATE ----------------
    if (command.equalsIgnoreCase("RECALIBRATE")) {
        if (!ensureZAtInspectionHeightBeforeCamera()) {
            Serial.println("ERROR: Failed to move Z to safe height before recalibrate");
            return;
        }

        INSPECTION_OFFSET_STEPS_X = calibrationState.currentOffsetX;
        INSPECTION_OFFSET_STEPS_Y = calibrationState.currentOffsetY;

        EEPROM.put(EEPROM_OFFSET_X_ADDR, INSPECTION_OFFSET_STEPS_X);
        EEPROM.put(EEPROM_OFFSET_Y_ADDR, INSPECTION_OFFSET_STEPS_Y);

        Serial.print("CALIBRATION_SAVED X=");
        Serial.print(INSPECTION_OFFSET_STEPS_X);
        Serial.print(" Y=");
        Serial.println(INSPECTION_OFFSET_STEPS_Y);

        state.lastXDirection = (calibrationState.currentOffsetX > 0) ? -1 : 1;
        state.lastYDirection = (calibrationState.currentOffsetY > 0) ? -1 : 1;

        stepperX.move(-calibrationState.currentOffsetX);
        stepperY.move(-calibrationState.currentOffsetY);

        unsigned long start = millis();
        while ((stepperX.distanceToGo() != 0 || stepperY.distanceToGo() != 0) &&
               (millis() - start < 30000)) {
            stepperX.run();
            stepperY.run();
            limitManager.checkLimitsDuringMove();
            delay(1);
        }

        syncCurrentPosFromSteppers();
        calibrationState.isCalibrating = false;
        Serial.println("CALIBRATION_DONE_RETURNED_TO_NOZZLE");
        return;
    }

    // ---------------- CANCEL ----------------
    if (command.equalsIgnoreCase("CANCEL")) {
        state.lastXDirection = (calibrationState.currentOffsetX > 0) ? -1 : 1;
        state.lastYDirection = (calibrationState.currentOffsetY > 0) ? -1 : 1;

        stepperX.move(-calibrationState.currentOffsetX);
        stepperY.move(-calibrationState.currentOffsetY);

        unsigned long start = millis();
        while ((stepperX.distanceToGo() != 0 || stepperY.distanceToGo() != 0) &&
               (millis() - start < 30000)) {
            stepperX.run();
            stepperY.run();
            limitManager.checkLimitsDuringMove();
            delay(1);
        }

        syncCurrentPosFromSteppers();
        calibrationState.isCalibrating = false;
        Serial.println("CALIBRATION_CANCELLED");
        return;
    }

    Serial.println("ERROR: Unknown calibration command");
}

// ============ PROCEDURE ============
void ProcedureManager::startProcedure() {
    if (!state.isHomed) {
        Serial.println("ERROR: Machine not homed");
        return;
    }
    if (!pickRef.isSet) {
        Serial.println("ERROR: Pick reference not set. Use SETPICK first.");
        return;
    }
    if (!pcbRef.isSet) {
        Serial.println("ERROR: PCB reference not set. Use SETPCB first.");
        return;
    }
    if (calibrationState.isCalibrating) {
        Serial.println("ERROR: Calibration active. CANCEL it first.");
        return;
    }

    syncCurrentPosFromSteppers();

    procedureState.active = true;
    procedureState.currentState = 0;
    procedureState.componentPicked = false;

    // Reset correction delta
    corrDeltaX = 0;
    corrDeltaY = 0;
    corrDeltaR = 0;

    // Reset cumulative correction for NEW component stream
    cumulativeCorrX = 0;
    cumulativeCorrY = 0;
    cumulativeCorrR = 0;

    pickDoneThisCycle  = false;
    placeDoneThisCycle = false;

    Serial.println("PROCEDURE_STARTED");
    sendAwaitingApproval();
}

void ProcedureManager::handleProcedureApproval() {
    if (procedureState.active && procedureState.currentState == 0) {
        feeder.advanceTape();
        delay(500);

        procedureState.currentState = 1;
        Serial.println("Moving to pick position...");
        moveToPickPosition();
    } else {
        Serial.println("APPROVAL_REJECTED");
    }
}

// state 3 => normal placement coords 
// state 8 => correction DELTA coords
void ProcedureManager::parseProcedureCoordinates(String command) {
    if (!procedureState.active) return;
    if (procedureState.currentState != 3 && procedureState.currentState != 8) return;

    command.toUpperCase();

    long xMove = stepperManager.parseAxisValue(command, 'X', STEPS_PER_MM_X);
    long yMove = stepperManager.parseAxisValue(command, 'Y', STEPS_PER_MM_Y);
    long rMove = stepperManager.parseAxisValue(command, 'R', 1);

    if (procedureState.currentState == 3) {
        procedureState.placeX = xMove;
        procedureState.placeY = yMove;
        procedureState.placeR = rMove;

        Serial.println("COORDS_RECEIVED");
        stepperManager.procedureOverrideZReturn = false;
        placeDoneThisCycle = false;

        procedureState.currentState = 20;
        return;
    }

    if (procedureState.currentState == 8) {
        corrDeltaX = xMove;
        corrDeltaY = yMove;
        corrDeltaR = rMove;

        Serial.println("CORRECTION_COORDS_RECEIVED");
        procedureState.currentState = 45;
        return;
    }
}

void ProcedureManager::runProcedureStateMachine() {
    if (!procedureState.active || state.emergencyStop) return;
    if (calibrationState.isCalibrating) return;

    switch (procedureState.currentState) {

        case 0:
            break;

        case 1:
            stepperManager.runMultiMove();
            limitManager.checkLimitsDuringMove();
            if (!stepperManager.isMoving()) {
                Serial.println("ARRIVED_AT_PICK_POSITION");
                procedureState.currentState = 2;
                pickDoneThisCycle = false;
            }
            break;

        case 2:
            if (!pickDoneThisCycle) {
                Serial.println("PERFORMING_PICK");
                performPick();
                pickDoneThisCycle = true;
            }
            break;

        case 3:
            break;

        // ===== PRE-PLACE  =====
        case 20: {
            syncCurrentPosFromSteppers();
            stepperZ.moveTo(Z_TRAVEL_HEIGHT);
            Serial.print("Retracting Z to travel height (abs): ");
            Serial.println(Z_TRAVEL_HEIGHT);
            procedureState.currentState = 21;
            break;
        }

        case 21: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();
            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("Z_RETRACTED_TO_TRAVEL_HEIGHT");
                moveToPlacePosition();
                procedureState.currentState = 22;
            }
            break;
        }

        case 22: {
            stepperManager.runMultiMove();
            limitManager.checkLimitsDuringMove();
            if (!stepperManager.isMoving()) {
                syncCurrentPosFromSteppers();
                Serial.println("XY_AT_PLACE_POSITION_SAFE_Z");
                stepperZ.moveTo(pcbRef.Z);
                Serial.print("Moving Z to pcbRef.Z (abs): ");
                Serial.println(pcbRef.Z);
                procedureState.currentState = 23;
            }
            break;
        }

        case 23: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();
            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("ARRIVED_AT_PLACE_POSITION");
                procedureState.currentState = 5;
                placeDoneThisCycle = false;
            }
            break;
        }

        case 5:
            if (!placeDoneThisCycle) {
                Serial.println("STARTING_PLACE_SEQUENCE");
                startPlaceSequence();
                placeDoneThisCycle = true;
            }
            break;

        // ===== POST-PLACE INSPECTION =====
        case 24: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();

            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("Z_RETRACTED_TO_INSPECTION_HEIGHT");

                
                if (cumulativeCorrX != 0 || cumulativeCorrY != 0) {
                    Serial.print("RETURN_TO_P1_BEFORE_CAMERA: X=");
                    Serial.print(-cumulativeCorrX);
                    Serial.print(" Y=");
                    Serial.println(-cumulativeCorrY);
                    Serial.println(-cumulativeCorrR);
                    stepperRot.move(-cumulativeCorrR);
                    stepperX.move(-cumulativeCorrX);
                    stepperY.move(-cumulativeCorrY);

                    procedureState.currentState = 245; 
                } else {
                    stepperX.move(INSPECTION_OFFSET_STEPS_X);
                    stepperY.move(INSPECTION_OFFSET_STEPS_Y);

                    Serial.print("Moving to camera (inspection offsets) X=");
                    Serial.print(INSPECTION_OFFSET_STEPS_X);
                    Serial.print(" Y=");
                    Serial.println(INSPECTION_OFFSET_STEPS_Y);

                    procedureState.currentState = 25;
                }
            }
            break;
        }

        case 245: { 
            stepperX.run();
            stepperY.run();
            stepperRot.run();
            limitManager.checkLimitsDuringMove();

            if (stepperX.distanceToGo() == 0 && stepperY.distanceToGo() == 0 && stepperRot.distanceToGo()== 0) {
                syncCurrentPosFromSteppers();
                Serial.println("AT_P1_FRAME_NOW_GO_CAMERA");

                stepperX.move(INSPECTION_OFFSET_STEPS_X);
                stepperY.move(INSPECTION_OFFSET_STEPS_Y);

                Serial.print("Moving to camera (inspection offsets) X=");
                Serial.print(INSPECTION_OFFSET_STEPS_X);
                Serial.print(" Y=");
                Serial.println(INSPECTION_OFFSET_STEPS_Y);

                procedureState.currentState = 25;
            }
            break;
        }

        case 25: {
            stepperX.run();
            stepperY.run();
            limitManager.checkLimitsDuringMove();

            if (stepperX.distanceToGo() == 0 && stepperY.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("ARRIVED_AT_INSPECTION_POSITION");

                visionSystem.startVisionCheck();
                procedureState.currentState = 26;
            }
            break;
        }

        case 26:
            break;

        // ===== INCORRECT FLOW =====
        case 41: {
            static bool issued = false;

            if (!issued) {
                Serial.println("INCORRECT: Returning from camera to nozzle (P1)");
                stepperX.move(-INSPECTION_OFFSET_STEPS_X);
                stepperY.move(-INSPECTION_OFFSET_STEPS_Y);
                issued = true;
            }

            stepperX.run();
            stepperY.run();
            limitManager.checkLimitsDuringMove();

            if (stepperX.distanceToGo() == 0 && stepperY.distanceToGo() == 0) {
                issued = false;
                syncCurrentPosFromSteppers();
                Serial.println("RETURNED_FROM_CAMERA_TO_NOZZLE_P1");

                procedureState.currentState = 411;
            }
            break;
        }

        case 411: {
            static bool issued = false;

            if (!issued) {
                if (cumulativeCorrX != 0 || cumulativeCorrY != 0) {
                    Serial.print("Moving to ACTUAL component position (P1 + cumulative) X=");
                    Serial.print(cumulativeCorrX);
                    Serial.print(" Y=");
                    Serial.println(cumulativeCorrY);

                    stepperX.move(cumulativeCorrX);
                    stepperY.move(cumulativeCorrY);
                    stepperRot.move(cumulativeCorrR);
                } else {
                    Serial.println("No cumulative correction yet -> already above component at P1");
                }
                issued = true;
            }

            stepperX.run();
            stepperY.run();
            stepperRot.run();
            limitManager.checkLimitsDuringMove();

            if (stepperX.distanceToGo() == 0 && stepperY.distanceToGo() == 0) {
                issued = false;
                syncCurrentPosFromSteppers();
                Serial.println("ABOVE_COMPONENT_FOR_REPICK");

                stepperZ.moveTo(pcbRef.Z);
                procedureState.currentState = 42;
            }
            break;
        }

        case 42: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();

            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("AT_PLACE_HEIGHT_FOR_REPICK");

                digitalWrite(pins.pump, HIGH);
                delay(500);
                procedureState.componentPicked = true;

                stepperZ.moveTo(Z_INSPECTION_HEIGHT);
                procedureState.currentState = 43;
            }
            break;
        }

        case 43: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();

            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("Z_AT_INSPECTION_HEIGHT_AFTER_REPICK");

                corrDeltaX = 0;
                corrDeltaY = 0;
                corrDeltaR = 0;

                procedureState.currentState = 8;
                sendAwaitingCoordinatesForCorrection();
            }
            break;
        }

        case 8:
            break;

        case 45: {
            long dx = corrDeltaX;
            long dy = corrDeltaY;
            long dr = corrDeltaR;

            Serial.print("Applying NEW correction DELTA while holding component: X=");
            Serial.print(dx);
            Serial.print(" Y=");
            Serial.print(dy);
            Serial.print(" R=");
            Serial.println(dr);

            String moveCommand =
                "X" + String(dx) +
                " Y" + String(dy) +
                " R" + String(dr);

            stepperManager.setTargetPosition(moveCommand);
            procedureState.currentState = 46;
            break;
        }

        case 46: {
            stepperManager.runMultiMove();
            limitManager.checkLimitsDuringMove();

            if (!stepperManager.isMoving()) {
                syncCurrentPosFromSteppers();
                Serial.println("CORRECTION_DELTA_APPLIED");

                // Update cumulative correction sum (for multi incorrect loops)
                cumulativeCorrX += corrDeltaX;
                cumulativeCorrY += corrDeltaY;
                cumulativeCorrR += corrDeltaR;

                stepperZ.moveTo(pcbRef.Z);
                procedureState.currentState = 47;
            }
            break;
        }

        case 47: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();

            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("AT_PLACE_HEIGHT_FOR_CORRECTED_PLACE");

                digitalWrite(pins.pump, LOW);
                Serial.println("PUMP OFF (corrected place)");
                delay(900);
                procedureState.componentPicked = false;

                stepperZ.moveTo(Z_INSPECTION_HEIGHT);
                procedureState.currentState = 48;
            }
            break;
        }

        case 48: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();

            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("Z_AT_INSPECTION_HEIGHT_AFTER_CORRECTED_PLACE");

               
                procedureState.currentState = 24;
            }
            break;
        }

        case 90: {
            stepperZ.run();
            limitManager.checkLimitsDuringMove();

            if (stepperZ.distanceToGo() == 0) {
                syncCurrentPosFromSteppers();
                Serial.println("Z_LOWERED_TO_TRAVEL_HEIGHT_AFTER_CORRECT");

                // Reset correction sum for next component
                cumulativeCorrX = 0;
                cumulativeCorrY = 0;
                cumulativeCorrR = 0;

                // Reset deltas
                corrDeltaX = 0;
                corrDeltaY = 0;
                corrDeltaR = 0;

                procedureState.currentState = 0;
                sendAwaitingApproval();
            }
            break;
        }

        default:
            break;
    }
}

void ProcedureManager::abortProcedure() {
    stepperManager.stopAllSteppers();
    digitalWrite(pins.pump, LOW);

    stepperManager.procedureOverrideZReturn = false;

    procedureState.active = false;
    procedureState.currentState = 0;
    procedureState.componentPicked = false;

    // Reset deltas
    corrDeltaX = 0;
    corrDeltaY = 0;
    corrDeltaR = 0;

    // Reset cumulative correction
    cumulativeCorrX = 0;
    cumulativeCorrY = 0;
    cumulativeCorrR = 0;

    pickDoneThisCycle  = false;
    placeDoneThisCycle = false;

    visionSystem.initialize();
    Serial.println("PROCEDURE_ABORTED");
}

// ============ messages ============
void ProcedureManager::sendPickCompleted()       { Serial.println("PICK_COMPLETED"); }
void ProcedureManager::sendPlaceCompleted()      { Serial.println("PLACE_COMPLETED"); }
void ProcedureManager::sendAwaitingApproval()    { Serial.println("AWAITING_APPROVAL"); }
void ProcedureManager::sendAwaitingCoordinates() { Serial.println("AWAITING_COORDINATES"); }
void ProcedureManager::sendAwaitingCoordinatesForCorrection() {
    Serial.println("AWAITING_FOR_COORDINATES_CORRECTION");
}

// ============ moves ============
void ProcedureManager::moveToPickPosition() {
    syncCurrentPosFromSteppers();

    long xMove = pickRef.X - currentPos.X;
    long yMove = pickRef.Y - currentPos.Y;
    long zMove = pickRef.Z - currentPos.Z;
    long rMove = pickRef.R - currentPos.R;

    String moveCommand =
        "X" + String(xMove) +
        " Y" + String(yMove) +
        " Z" + String(zMove) +
        " R" + String(rMove);

    Serial.print("Safe move to pick: ");
    Serial.println(moveCommand);

    stepperManager.setTargetPosition(moveCommand);
}

void ProcedureManager::moveToPlacePosition() {
    String moveCommand =
        "X" + String(procedureState.placeX) +
        " Y" + String(procedureState.placeY) +
        " R" + String(procedureState.placeR);

    Serial.print("Safe XY move to place: ");
    Serial.println(moveCommand);

    stepperManager.setTargetPosition(moveCommand);
}

// ============ actions ============
void ProcedureManager::performPick() {
    Serial.println("PICKING_COMPONENT");
    digitalWrite(pins.pump, HIGH);
    delay(500);

    procedureState.componentPicked = true;
    sendPickCompleted();

    stepperManager.procedureOverrideZReturn = true;

    procedureState.currentState = 3;
    sendAwaitingCoordinates();
}

void ProcedureManager::startPlaceSequence() {
    Serial.println("PLACING_COMPONENT");

    digitalWrite(pins.pump, LOW);
    Serial.println("PUMP OFF");
    delay(900);

    procedureState.componentPicked = false;
    sendPlaceCompleted();
    stepperZ.setMaxSpeed(1500); 
    stepperZ.setAcceleration(500);  
    stepperZ.moveTo(Z_INSPECTION_HEIGHT);
    Serial.print("Retracting Z after place to inspection height (abs): ");
    Serial.println(Z_INSPECTION_HEIGHT);

    stepperManager.procedureOverrideZReturn = false;
    procedureState.currentState = 24;
}