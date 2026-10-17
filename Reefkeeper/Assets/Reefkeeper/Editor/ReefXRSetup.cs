using System;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Reefkeeper.Editor
{
    public static class ReefXRSetup
    {
        public static void ImportSamples()
        {
            foreach (var sample in Sample.FindByPackage("com.unity.xr.interaction.toolkit", "3.3.2"))
                if (sample.displayName == "Starter Assets" || sample.displayName == "XR Interaction Simulator")
                {
                    if (!sample.isImported && !sample.Import(Sample.ImportOptions.OverridePreviousImports))
                        throw new Exception("Cannot import XR sample " + sample.displayName);
                    Debug.Log("REEFKEEPER_XR_SAMPLE " + sample.displayName + " " + sample.importPath);
                }
        }
    }
}
