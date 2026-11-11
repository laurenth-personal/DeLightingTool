# De-Lighting Tool

The De-Lighting Tool removes lighting information from textures in a photogrammetry asset pipeline.

## Install with Unity Package Manager

1. In Unity, open **Window > Package Manager**.
2. Click **+ > Add package from git URL...**.
3. Enter `https://github.com/laurenth-personal/DeLightingTool.git`.
4. After the package is installed, open the package in Package Manager, expand **Samples**, and import **Rock01 Data** to add the sample archive to your project.
5. Extract the imported `DeLighting-Tool-Data-Rock01.zip` and import its contents into the Unity project as described by the included sample files.
6. Open the tool from **Window > Delighting Tool**.

The package includes the editor tool, shaders, GUI helpers, and documentation. The sample is optional and is not required to use the tool with your own photogrammetry textures.

The project was developed with Unity `6000.3.25f1`. The original project documentation lists compatibility from Unity 2017.1; older Unity versions may need compatibility adjustments.

## More information

- [Tool documentation](Assets/DeLightingTool/Documentation/De-LightingTool.pdf)
- [Unity blog post](https://labs.unity.com/article/experimental-feature-de-lighting-tool)
- [Video overview](https://www.youtube.com/watch?v=Mo96vqMqnwA&feature=youtu.be)
- [Unity Experimental forum discussion](https://forum.unity3d.com/threads/de-lighting-tool.482133/)
