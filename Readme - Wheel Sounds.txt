-------------------------------------------------------------
 Wheel Sounds
 Author:    pizzaoverhead
 Version:   1.1
 Released:  2014-09-25
 KSP:       v0.24.2

 Thread:    http://forum.kerbalspaceprogram.com/threads/55104
 Licence:   GNU v2, http://www.gnu.org/licenses/gpl-2.0.html
 Source:    https://github.com/pizzaoverhead/WheelSounds
-------------------------------------------------------------

Wheel sounds adds sound effects when using rover wheels. Sounds are only added to wheels with the motor enabled. Check the forum thread for updates:
http://forum.kerbalspaceprogram.com/threads/52896

This plugin makes use of Sarbian's Module Manager to avoid needing part.cfg edits. Check here for new versions:
http://forum.kerbalspaceprogram.com/threads/55219


Sounds used
-----------
Kerbal Motion TR-2L	http://www.freesound.org/people/mrmayo/sounds/74915/
RoveMax S2		http://www.freesound.org/people/Adam_N/sounds/144956/
RoveMax XL-3		http://www.freesound.org/people/suonidigenova/sounds/55025/


Installation
------------
Extract the zip to the root KSP folder, merging with the GameData folder.


Adding new sounds
-----------------
Add the new file(s) into the following directory:

	KSP/GameData/WheelSounds/Sounds

Edit ModuleManager_WheelSounds.cfg (found in GameData\RcsSounds\) to add the new sound, referencing the new sound's filename.

For example, if you want to change the sound for the largest rover wheel to a new one called myRoverWheel.wav, under the name of the part (roverWheel3 in this case) change:

	wheelSoundFile = WheelSounds/Sounds/RoveMaxXL3

to:

	wheelSoundFile = WheelSounds/Sounds/myRoverWheel

Note that neither the file extension (.wav) nor "quotation marks" should be included.


Version history
---------------
1.2 (2014-10-10)
- Rebuilt for 0.25.
- Upgraded to Module Manager 2.5.1.

1.1 (2014-09-25)
- Rebuilt for 0.24.x.
- Fixed null reference exception.
- Added Module Manager licence.
- Upgraded to Module Manager 2.3.5.
- Added support for the KSP Add-on Version Checker.

1.0 (2014-06-07)
- Added sounds for all four stock rover wheels.
- Removed landing gear lowering sound. This will be added in a separate mod.

0.5 (2014-03-25)
- Fixed the sounds not being affected by the game's volume settings.

0.4 (2014-01-11)
- Added a prototype of sounds for raising and lowering landing gear. This can be disabled by removing the gear section in ModuleManager_WheelSounds.cfg or by deleting the sound.

0.3 (2014-01-07)
- Added sounds to the largest rover wheel, the RoveMax XL3.