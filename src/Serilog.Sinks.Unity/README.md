# Serilog.Sinks.Unity

![Serilog.Sinks.Unity](../../docs-assets/serilog-sinks.png)

**Work in progress!**

This repo has been under open-source development since ~2017, but only since late 2022 has it been seriously considered for "usability" by 3rd parties,
so documentation content/organization are still in development.

## Overview

A [Serilog](https://serilog.net/) sink that writes logs to the Unity `Debug` Console.
Safely schedules logging on the Unity main thread, and includes `tag` strings or `context` objects if provided.

You can also enrich these logs with additional Unity info like the current `Time.frameCount`
using [Serilog.Enrichers.Unity](https://github.com/DerploidEntertainment/UnityUtil/blob/main/src/Serilog.Enrichers.Unity).

Copyright © 2017 - present Dan Vicarel & Contributors. All rights reserved.
