# Materials

`PastelToon.mat` is created automatically by **DivineDrift → Build Playable Scene**
(it instantiates the `DivineDrift/PastelToon` shader). You normally don't create it
by hand.

Suggested tuning on the material:
- **Toon Steps**: 2–3 for chunky pastel banding.
- **Ambient Floor**: ~0.55 so the shadowed side stays soft, not black.
- **Pastel Desaturate**: ~0.25 to pull colors toward the soft pastel range.

The black edges come from the screen-space **OutlineEffect** on the camera, not from
the material.
