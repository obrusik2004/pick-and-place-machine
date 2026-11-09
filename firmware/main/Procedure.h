#ifndef PROCEDURE_H
#define PROCEDURE_H

#include <Arduino.h>
#include "Config.h"

class ProcedureManager {
public:
    // init
    void initialize();

    // main procedure
    void startProcedure();
    void handleProcedureApproval();
    void parseProcedureCoordinates(String command);
    void runProcedureStateMachine();
    void abortProcedure();

    // calibration
    void startCalibration();
    void handleCalibrationCommand(String command);
    bool isCalibrating() const { return calibrationState.isCalibrating; }

    // helpers
    bool isProcedureActive() const { return procedureState.active; }

    // messages
    void sendPickCompleted();
    void sendPlaceCompleted();
    void sendAwaitingApproval();
    void sendAwaitingCoordinates();
    void sendAwaitingCoordinatesForCorrection();

private:
    void moveToPickPosition();
    void moveToPlacePosition();
    void performPick();
    void startPlaceSequence();

    // calibration state lives inside the class
    struct {
        bool isCalibrating = false;

        long originalOffsetX = 0;
        long originalOffsetY = 0;
        long currentOffsetX  = 0;
        long currentOffsetY  = 0;

        long nozzlePosX = 0;
        long nozzlePosY = 0;
    } calibrationState;

    // =========================================================
    // (INCORRECT loops)
    //
    // We keep INSPECTION FRAME fixed at P1 (first place position),
    // and maintain a cumulative correction vector so we can:
    // - repick from the actual component position (P1 + cumulative)
    // - after corrected place, return back to P1 before going to camera
    // =========================================================

    // Sum of ALL corrections applied so far for the CURRENT component
    long cumulativeCorrX = 0;
    long cumulativeCorrY = 0;
    long cumulativeCorrR = 0; 

    // One correction DELTA received from C# (applied once per INCORRECT)
    long corrDeltaX = 0;
    long corrDeltaY = 0;
    long corrDeltaR = 0;

    long correctionOffsetX = 0;  
    long correctionOffsetY = 0;
    long correctionOffsetR = 0;
};

extern ProcedureManager procedureManager;

#endif