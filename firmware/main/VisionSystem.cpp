#include "VisionSystem.h"
#include "Procedure.h"
#include "Steppers.h"
#include "Limits.h"
#include "Config.h"

VisionSystem visionSystem;

// IMPORTANT:
// Do NOT hardcode inspection offsets (like +80 X only) here anymore.
// In the new architecture, Procedure.cpp owns the inspection offsets (X and Y),
// and state 41 handles returning from camera -> nozzle (P1) properly.
//
// VisionSystem should only:
// - set waiting flag
// - map CORRECT/INCORRECT to proper procedure states
// - optionally command Z move for CORRECT (safe absolute)

static const long Z_TRAVEL_HEIGHT = 2000; // must match Procedure.cpp

void VisionSystem::initialize() {
    waitingForFeedback = false;
}

void VisionSystem::handleVisionCommand(String command) {
    command.trim();        // remove spaces/newlines from C#
    command.toUpperCase();

    if (command == "CORRECT") {
        processCorrectPlacement();
    }
    else if (command == "INCORRECT") {
        processIncorrectPlacement();
    }
    else {
        Serial.print("VISION_SYSTEM: Unknown command: ");
        Serial.println(command);
    }
}

void VisionSystem::startVisionCheck() {
    waitingForFeedback = true;
    Serial.println("VISION_SYSTEM: Waiting for placement feedback...");
}

// ==========================
// CORRECT CASE
// ==========================
// - Procedure is currently at camera inspection position.
// - VisionSystem should NOT try to handle X/Y offsets.
// - We just command Z to travel height and jump to state 90,
//   which will finish the cycle and request approval again.
// =============================================================
void VisionSystem::processCorrectPlacement() {
    if (!waitingForFeedback) {
        Serial.println("VISION_SYSTEM: Not waiting for feedback, ignoring CORRECT");
        return;
    }

    waitingForFeedback = false;
    Serial.println("VISION_SYSTEM: Placement CORRECT - finishing cycle");

    stepperZ.moveTo(Z_TRAVEL_HEIGHT);

    procedureState.currentState = 90;
}

// ==========================
// INCORRECT CASE
// ==========================
// - VisionSystem does NOT move steppers.
// - It only tells Procedure state machine to enter incorrect flow.
//
// Procedure state 41 will:
// - return from camera to nozzle (undo inspection offsets X/Y)
// - then move to actual component position using cumulative correction
// - repick, request correction coords, apply delta, place, return to P1,
// - then go to camera again (always inspect at P1)
// ==============================================================================
void VisionSystem::processIncorrectPlacement() {
    if (!waitingForFeedback) {
        Serial.println("VISION_SYSTEM: Not waiting for feedback, ignoring INCORRECT");
        return;
    }

    waitingForFeedback = false;
    Serial.println("VISION_SYSTEM: Placement INCORRECT - handing over to Procedure correction flow");

    // Let Procedure handle all movements:
    // state 41 = start incorrect correction sequence
    procedureState.currentState = 41;
}