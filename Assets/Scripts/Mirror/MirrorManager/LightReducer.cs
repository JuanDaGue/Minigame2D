using UnityEngine;

public class LightReducer
{
    private readonly MirrorCollection _mirrorCollection;
    private readonly float _lightReductionAmount;
    
    public LightReducer(MirrorCollection mirrorCollection, float lightReductionAmount)
    {
        _mirrorCollection = mirrorCollection;
        _lightReductionAmount = lightReductionAmount;
    }
    
    public void ReduceLights()
    {
        foreach (var mirror in _mirrorCollection.Mirrors)
        {
            if (mirror == null) continue;
            
            var lightsController = mirror.GetComponent<LigthsController>();
            if (lightsController != null)
            {
                lightsController.IntensityController(_lightReductionAmount);
            }
        }
    }
}