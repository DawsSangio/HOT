# HOT
Humble Oculus Toolkit

A simple tool to manage basic function of the Oculus Runtime for Rift CV1 and Rift-S, like Super Sampling or OSD debug screens.
I want it simple, so no install required, no additionl dll required, a single exe that you can put where you want and simply run it.

What you can do:
- Change Super Sampling
- Select ASW mode
- Select On Screen Display debug information

When you run as Administrator you can:
- Save preset per application's exe, that will be loaded on specific apps start
- Enable/Disable Home 2 Enviroment, you'll get only Dash, usefull to save loading time and resources.
- Enable/Disable Dash noisy background sound.
- Select Dash floor texture, black / white.
- Backup and Restore Oculus core runtime library. Usefull to test different runtime.

Bonus:
Thanks to ZNix, author of Open Composite, I've implemented the donwload of latest Open Composite dll, and manage activation.
For more info on Open Composite: https://gitlab.com/znixian/OpenOVR/
- Enable/Disable Open Composite.
- On start it check for Open Composite update and automatically download.


Note:
This project is simply a learning task to improve my c# skills.
There's not intention to make a perfect solution, as I often change approach on solving issues.
