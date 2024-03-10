# gtav-entrance-trains
The mod adds the ability to stop and hijack trains.

Each train now has a driver, killing him will put you in the cab and start the train. The driver will react to obstacles and will try to stop if he sees an obstacle ahead. However, at night or in bad weather, the driver's reaction may be delayed, so keep that in mind. If you kill the engineer while the train is moving, the train will continue to move on inertia for a while until it comes to a complete stop.

You can board the train as you would a normal car. You can also shoot from the cab while the train is moving. To get off the train, it must come to a complete stop.

By default, trains are marked with an blip on the map. You can disable this by setting blips to 0 in the Entrance Trains.ini file.

**PLEASE NOTE THAT YOU MUST HAVE THE CHOP SHOP UPDATE INSTALLED FOR THIS MOD TO WORK**. Read the installation instructions carefully. If you have modified trains.xml, read the installation instructions in MANUAL INSTALL. If you still have questions, write me to Discord: https://discord.gg/vvxmKP5y5J

# INSTALL
1. Download ScriptHookV and ScriptHookVDotNet
2. Move dll, ini and pdb files to the scripts folder (If there is no such folder, create one)
3. Use OpenIV to replace the trains.xml file in update/update.rpf/common/data/levels/gta5


# MANUAL INSTALL (if you have a modified trains.xml file)

1. Download ScriptHookV and ScriptHookVDotNet
2. Move dll, ini and pdb files to the scripts folder (If there is no such folder, create one)
3. Open the trains.xml file in update/update.rpf/common/data/levels/gta5
4. Use the search to find all mentions of "freight" and replace them with "freight2" (unquoted)
5. Save your changes
