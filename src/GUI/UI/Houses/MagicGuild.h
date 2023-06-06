#pragma once

#include <vector>

#include "GUI/UI/UIHouses.h"
#include "GUI/UI/UIHouseEnums.h"

#include "Utility/IndexedArray.h"

class GUIWindow_MagicGuild : public GUIWindow_House {
 public:
    explicit GUIWindow_MagicGuild(HOUSE_ID houseId) : GUIWindow_House(houseId) {}
    virtual ~GUIWindow_MagicGuild() {}

    virtual void houseDialogueOptionSelected(DIALOGUE_TYPE option) override;
    virtual void houseSpecificDialogue() override;
    virtual std::vector<DIALOGUE_TYPE> listDialogueOptions(DIALOGUE_TYPE option) override;

 protected:
    /**
     * @offset 0x4BC8D5
     */
    void generateSpellBooksForGuild();

    void mainDialogue();
    void buyBooksDialogue();
};

extern IndexedArray<int, HOUSE_FIRST_MAGIC_GUILD, HOUSE_LAST_MAGIC_GUILD> guildMembershipFlags;
