# Prototype

this prototype stands implementation attempt, the refactor to C/C++ will be required once its complete.

## Requirements
### Hardware
+ OpenGL ES ~~2.0 minimum~~(NanoVG needs get fixed first, so far only GLES3 backend), 3.1 is recommended. Allowed to use ANGLE library to translate API to other API like Vulkan or whatever.
> Not measured the hardware usage yet, cannot confirm how much RAM usage after all but so far it not more than 500MiB or even 100MiB

### Software
+ .NET 9 SDK
+ GLFW windowing and graphics-context system (temporarily usage, will be replaced by our own implementation)
+ NanoVG shared library (GLES3, with stb_image and font)