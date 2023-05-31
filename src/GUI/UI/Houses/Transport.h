#pragma once

#include "GUI/UI/UIHouses.h"
#include "GUI/UI/UIHouseEnums.h"

class GUIWindow_Transport : public GUIWindow_House {
 public:
    explicit GUIWindow_Transport(HOUSE_ID houseId) : GUIWindow_House(houseId) {}
    virtual ~GUIWindow_Transport() {}

    virtual void houseDialogueOptionSelected(DIALOGUE_TYPE option) override;
    virtual void houseSpecificDialogue() override;

 protected:
    void mainDialogue();
    void transportDialogue();

 private:
    /**
     * @brief                               New function.
     *
     * @param schedule_id                   Index to transport_schedule.
     *
     * @return                              Number of days travel by transport will take with hireling modifiers.
     */
    int getTravelTimeTransportDays(int schedule_id);
};

bool isTravelAvailable(HOUSE_ID houseId);
