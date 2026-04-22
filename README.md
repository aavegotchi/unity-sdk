# Getting Started

Watch the Getting Started with the Aavegotchi Unity SDK video here: https://www.youtube.com/watch?v=kZBDkg6Nq-g&t=13s

See Unity SDK - Readme.pdf for more documentation

## Render Pipeline Requirement

This SDK's environment art, materials, and many prefabs use Shader Graph shaders authored for Universal Render Pipeline (URP).

If imported prefabs appear pink, the target project is usually missing one of these requirements:

- `com.unity.render-pipelines.universal`
- `com.unity.shadergraph`
- An active Universal Render Pipeline Asset assigned in `Project Settings > Graphics`
- The same URP asset assigned for the active quality level in `Project Settings > Quality`

The package manifest now declares the required URP and Shader Graph dependencies, but Unity still requires the host project to use a URP asset before those materials can render correctly.
