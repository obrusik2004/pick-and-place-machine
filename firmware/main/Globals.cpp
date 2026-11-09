#include "Config.h"
#include "Steppers.h"
#include "Procedure.h"
#include "References.h"
#include "Homing.h"
#include "Limits.h"
#include <Arduino.h>


void HomingAxis() {
    homingManager.homeAllAxes();
}

void enableMotor(int enablePin) { 
    digitalWrite(enablePin, LOW); 
}

void startProcedure() {
    procedureState.active = true;
    procedureState.currentState = 0;
    procedureState.componentPicked = false;
    Serial.println("PROCEDURE_STARTED");
    procedureManager.sendAwaitingApproval();
}

void procedure(){
    if (!state.isHomed) {
        Serial.println("ERROR: Home the machine before starting procedure.");
        return;
    }
    if (!pickRef.isSet) {
        Serial.println("ERROR: Pick reference not set! Use SETPICK first.");
        return;
    }
    if(!pcbRef.isSet) {
        Serial.println("ERROR: PCB reference not set! Use SETPCB first.");
        return;
    }
    
    startProcedure();
}

void setPCBReference() {
    referenceManager.setPCBReference();
}

void setPickReference() {
    referenceManager.setPickReference();
}

void pick() {
    referenceManager.pickComponent();
}

void emergencyStopAll() {
    stepperManager.stopAllSteppers();
    
    while(stepperX.isRunning() || stepperY.isRunning() || 
          stepperZ.isRunning() || stepperRot.isRunning()) {
        stepperX.run();
        stepperY.run();
        stepperZ.run();
        stepperRot.run();
    }
    
    digitalWrite(pins.enableX, HIGH);
    digitalWrite(pins.enableY, HIGH);
    digitalWrite(pins.enableZ, HIGH);
    digitalWrite(pins.enableRot, HIGH);
    
    targetPos.moving = false;
    state.emergencyStop = true;
}

void immediateHardStop(AccelStepper &stepper, int enablePin) {
    stepper.stop();
    while(stepper.isRunning()) stepper.run();
    digitalWrite(enablePin, HIGH);
    delay(100);
}

void stopSingleAxis(char axis, int enablePin) {
    switch(axis) {
        case 'X': 
            stepperX.stop();
            while(stepperX.isRunning()) stepperX.run();
            break;
        case 'Y':
            stepperY.stop();
            while(stepperY.isRunning()) stepperY.run();
            break;
        case 'Z':
            stepperZ.stop();
            while(stepperZ.isRunning()) stepperZ.run();
            break;
    }
    digitalWrite(enablePin, HIGH);
}


void handleProcedureApproval() {
    procedureManager.handleProcedureApproval();
}

void sendPickCompleted() {
    procedureManager.sendPickCompleted();
}

void sendPlaceCompleted() {
    procedureManager.sendPlaceCompleted();
}

void sendAwaitingApproval() {
    procedureManager.sendAwaitingApproval();
}

void sendAwaitingCoordinates() {
    procedureManager.sendAwaitingCoordinates();
}

void abortProcedure() {
    procedureManager.abortProcedure();
}

void checkLimitsDuringMove() {
    limitManager.checkLimitsDuringMove();
}

void recoverFromLimitX() {
    limitManager.recoverFromLimitX();
}

void recoverFromLimitY() {
    limitManager.recoverFromLimitY();
}

void recoverFromLimitZ() {
    limitManager.recoverFromLimitZ();
}

void immediateAxisStop(AccelStepper &stepper, int enablePin) {
    limitManager.immediateAxisStop(stepper, enablePin);
}

long parseAxisValue(String input, char axis, int stepsPerMM) {
    return stepperManager.parseAxisValue(input, axis, stepsPerMM);
}

int getNextAxisIndex(String input, int start) {
    return stepperManager.getNextAxisIndex(input, start);
}

void runMultiMove() {
    stepperManager.runMultiMove();
}