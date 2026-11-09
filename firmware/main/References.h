#ifndef REFERENCES_H
#define REFERENCES_H

#include "Config.h"

class ReferenceManager {
public:
    void setPCBReference();
    void setPickReference();
    void pickComponent();
    bool isPCBReferenceSet() { return pcbRef.isSet; }
    bool isPickReferenceSet() { return pickRef.isSet; }
    
private:
    void moveToPickPosition();
};

extern ReferenceManager referenceManager;

#endif