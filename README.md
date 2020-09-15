# HOT
Humble Oculus Toolkit

A simple tool to manage basic functions of the Oculus Runtime for Rift CV1 and Rift-S, like Super Sampling or OSD debug screens.

I want it simple, so no install required, no additional dll required, a single exe that you can put where you want and simply run it.

Update 2020.9
Now HOT need to run as Administrator, to simplify its use. Otherwise you'll be prompted by UAC too many times!

What you can do:
- Change Super Sampling ( this will be memorized when relaoad HOT )
- Select ASW mode ( this will be memorized when relaoad HOT )
- Select On Screen Display debug information
- Start and Stop Oculus Service
- Save preset of SS and ASW mode per application's exe, that will be loaded on specific apps start
- Enable/Disable Home 2 Enviroment, you'll get only Dash, usefull to save loading time and resources.(deprecated as it's obsolete)
- Enable/Disable Dash noisy background sound.
- Select Dash floor texture, black / white.
- Backup and Restore Oculus core runtime library. Usefull to test different runtime.

Bonus:
Thanks to ZNix, author of Open Composite, I've implemented the donwload of latest Open Composite dll, and manage activation.
For more info on Open Composite: https://gitlab.com/znixian/OpenOVR/
- Enable/Disable Open Composite.
- Donwload or update to latest Open Composite available.

Note:
This project is simply a learning task to improve my c# skills.
There's not intention to make a perfect solution, as I often change approach on solving issues.
