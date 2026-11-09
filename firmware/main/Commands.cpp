#include "Commands.h"
#include "Config.h"
#include "Homing.h"
#include "References.h"
#include "Procedure.h"
#include "Emergency.h"
#include "Steppers.h"
#include "Limits.h"
#include "Feeder.h"
#include "VisionSystem.h"

CommandParser commandParser;

static int read100() {
    long sum = 0;
    for (int i = 0; i < 10; i++) {
        sum += analogRead(A6);
        delayMicroseconds(200);
    }
    int avg = (int)(sum / 10);
    Serial.print("AVG (100 samples) = ");
    Serial.println(avg);
    return avg;
}

static unsigned long lastMonitorTime = 0;
static const unsigned long monitorInterval = 10000;

static bool looksLikeCoords(const String& s) {
    // Accept coords in any order, with or without spaces: "X100 Y-50 R0" or "Y1000 X-1000"
    return (s.indexOf('X') != -1 || s.indexOf('x') != -1 ||
            s.indexOf('Y') != -1 || s.indexOf('y') != -1 ||
            s.indexOf('R') != -1 || s.indexOf('r') != -1);
}

void CommandParser::parseCommand(String input) {
    input.trim();
    if (input.length() == 0) return;

    // Print raw command for debugging
    Serial.print("RAW COMMAND RECEIVED: ");
    Serial.println(input);

    // =========================================================
    // 1) HARD PRIORITY: if procedure is waiting for coordinates,
    //    forward them BEFORE any other "ignore manual" logic.
    //
    // State meanings (per your new logic):
    //  - state 3: normal placement coords (P1)
    //  - state 8: correction DELTA coords (repo)
    // =========================================================
    if (procedureState.active &&
        (procedureState.currentState == 3 || procedureState.currentState == 8) &&
        looksLikeCoords(input)) {
        Serial.print("FORWARDING COORDS: ");
        Serial.println(input);
        procedureManager.parseProcedureCoordinates(input);
        return;
    }

    // =========================================================
    // 2) Emergency / reset
    // =========================================================
    if (input.equalsIgnoreCase("S")) {
        emergencyStopAll();
        procedureManager.abortProcedure();
        Serial.println("EMERGENCY STOP ACTIVATED!");
        return;
    }
    if (input.equalsIgnoreCase("RESET")) {
        state.emergencyStop = false;
        state.autoRecoveryInProgress = false;
        Serial.println("Emergency stop cleared.");
        return;
    }

    // =========================================================
    // 3) Homing gate
    // =========================================================
    if (!state.isHomed && !input.equalsIgnoreCase("HOME")) {
        Serial.println("ERROR: Please HOME the machine first.");
        return;
    }
    if (input.equalsIgnoreCase("HOME")) {
        HomingAxis();
        return;
    }

    // =========================================================
    // 4) Calibration mode
    // =========================================================
    if (input.equalsIgnoreCase("CALIBRATE_CAMERA")) {
        procedureManager.startCalibration();
        return;
    }

    if (procedureManager.isCalibrating()) {
        if (input.equalsIgnoreCase("MOVE_TO_CAMERA") ||
            input.startsWith("CX:") ||
            input.startsWith("CY:") ||
            input.equalsIgnoreCase("CALIBRATE") ||
            input.equalsIgnoreCase("RECALIBRATE") ||
            input.equalsIgnoreCase("CANCEL")) {

            procedureManager.handleCalibrationCommand(input);
            return;
        }

        // Allow normal movement commands during calibration
        char c0 = input.charAt(0);
        if (c0=='X' || c0=='Y' || c0=='Z' || c0=='R' || c0=='x' || c0=='y' || c0=='z' || c0=='r') {
            stepperManager.setTargetPosition(input);
            return;
        }

        Serial.println("ERROR: Invalid command during calibration");
        Serial.println("Calibration commands: MOVE_TO_CAMERA, CX:value, CY:value, CALIBRATE, RECALIBRATE, CANCEL");
        Serial.println("Or use normal X, Y, Z, R movement commands");
        return;
    }

    // =========================================================
    // 5) Dedicated commands
    // =========================================================
    if (input.equalsIgnoreCase("FEEDER")) {
        feeder.advanceTape();
        Serial.println("FEEDER_ADVANCED");
        return;
    }

    if (input.equalsIgnoreCase("SENSOR")) {
        int value = analogRead(A6);
        Serial.print("Sensor value: ");
        Serial.println(value);

        int avg = read100();
        Serial.print("AVG100: ");
        Serial.println(avg);
        return;
    }

    if (input.equalsIgnoreCase("SETPCB")) {
        setPCBReference();
        return;
    }

    if (input.equalsIgnoreCase("SETPICK")) {
        setPickReference();
        return;
    }

    if (input.equalsIgnoreCase("PICK")) {
        pick();
        return;
    }

    if (input.equalsIgnoreCase("PROCEDURE")) {
        procedureManager.startProcedure();
        return;
    }

    if (input.equalsIgnoreCase("APPROVE")) {
        procedureManager.handleProcedureApproval();
        return;
    }

    if (input.equalsIgnoreCase("ABORT")) {
        procedureManager.abortProcedure();
        return;
    }

    if (input.equalsIgnoreCase("PON")) {
        // Clear Serial buffer
        while (Serial.available() > 0) {
            Serial.read();
        }

        digitalWrite(pins.pump, HIGH);

        delay(10);  

        Serial.println("PUMP ON");
        Serial.flush();
        return;
    }

    if (input.equalsIgnoreCase("POFF")) {
        // Clear Serial buffer before printing helped was good
        while (Serial.available() > 0) {
            Serial.read();
        }

        Serial.print("DEBUG: Manual POFF called. Procedure active=");
        Serial.println(procedureState.active);
        Serial.print(" State=");
        Serial.println(procedureState.currentState);
        Serial.flush();  

        digitalWrite(pins.pump, LOW);

        // Small delay to prevent buffer overflow
        delay(10);

        Serial.println("PUMP OFF (MANUAL)");
        Serial.flush();
        return;
    }

    // =========================================================
    // 6) Vision responses
    // =========================================================
    if (input.equalsIgnoreCase("CORRECT") || input.equalsIgnoreCase("INCORRECT")) {
        visionSystem.handleVisionCommand(input);
        return;
    }

    // =========================================================
    // 7) Manual movement (blocked during procedure)
    // =========================================================
    if (procedureState.active) {
        Serial.println("IGNORED: manual command during PROCEDURE");
        return;
    }

    // Only treat commands starting with X/Y/Z/R as movement commands
    char c = input.charAt(0);
    if (c=='X' || c=='Y' || c=='Z' || c=='R' || c=='x' || c=='y' || c=='z' || c=='r') {
        stepperManager.setTargetPosition(input);
        return;
    }

    Serial.println("ERROR: Unknown command");
}

void CommandParser::liveMonitor() {
    unsigned long now = millis();
    if (now - lastMonitorTime >= monitorInterval) {
        lastMonitorTime = now;

        currentPos.X = stepperX.currentPosition();
        currentPos.Y = stepperY.currentPosition();
        currentPos.Z = stepperZ.currentPosition();
        currentPos.R = stepperRot.currentPosition();

        Serial.print("POS ");
        Serial.print("Y:");
        Serial.print(currentPos.Y);
        Serial.print(" steps  |  ");

        Serial.print("X:");
        Serial.print(currentPos.X);
        Serial.print(" steps  |  ");

        Serial.print("Z:");
        Serial.print(currentPos.Z);
        Serial.print(" steps  |  ");

        Serial.print("R:");
        Serial.print(currentPos.R);
        Serial.println(" steps");
    }
}