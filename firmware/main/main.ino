    #include "Config.h"
    #include "Steppers.h"
    #include "Limits.h"
    #include "Homing.h"
    #include "References.h"
    #include "Procedure.h"
    #include "Commands.h"
    #include "Emergency.h"
    #include "Feeder.h"
    #include "VisionSystem.h"
    #include <AccelStepper.h>


    void setup() {
    Serial.begin(19200);
    state.isHomed = true;
    feeder.initialize(); 
    stepperManager.initializeSteppers();
    limitManager.initializeLimitPins();
    emergencyManager.clearEmergencyStop();
    visionSystem.initialize(); 
      procedureManager.initialize(); 
    Serial.println("System ready! Send 'HOME' to start.");
    }
    void loop() {
    if (Serial.available() > 0) {
        String input = Serial.readStringUntil('\n');
        input.trim();
        commandParser.parseCommand(input);
    }

    if (state.emergencyStop) return;

    // Always tick the procedure state machine (it decides what to do)
    procedureManager.runProcedureStateMachine();

    // Only allow manual move engine when procedure is not active
    if (!procedureState.active) {
        stepperManager.runMultiMove();
    }

    limitManager.checkLimitsDuringMove();
    commandParser.liveMonitor();
}
