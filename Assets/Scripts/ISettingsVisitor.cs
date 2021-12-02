using CBodies.Settings.Physics;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;

public interface ISettingsVisitor
{
    void VisitShapeSettings(RockShape sp);
    void VisitShapeSettings(GaseousShape sp);
    void VisitShapeSettings(StarShape sp);
    
    void VisitShadingSettings(RockShading sd);
    void VisitShadingSettings(GaseousShading sd);
    void VisitShadingSettings(StarShading sd);

    void VisitPhysicsSettings(Physics ps);
}