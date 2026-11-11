# Unity Package Manager installation

Install this repository in Unity through **Window > Package Manager > + > Add package from git URL...** using:

```text
https://github.com/laurenth-personal/DeLightingTool.git
```

The package manifest is at the repository root. The existing Unity project assets under `Assets/` are included with the package; the Editor extension opens from **Window > Delighting Tool**.

## Rock01 sample

The package manifest exposes `Documentation/` as the **Rock01 Sample Data** UPM sample. In Package Manager, select the package and click **Import** beside that sample. Unity copies the existing `DeLighting-Tool-Data-Rock01.zip` into the project under `Assets/Samples/De-Lighting Tool/1.0.0/Documentation/`. Extract the ZIP and use the sample assets with the tool.

The sample is optional. The tool requires compute-shader-capable hardware.

## Compatibility

The repository project is currently developed with Unity `6000.3.25f1`. The original README lists compatibility from Unity 2017.1; editor API differences in older Unity versions may require adjustments.
