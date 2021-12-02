
namespace CBodies.Settings
{
    public interface ISettingsComponent
    {
        void AcceptVisitor(ISettingsVisitor visitor);
    }
}
