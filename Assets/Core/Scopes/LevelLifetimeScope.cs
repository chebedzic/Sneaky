using VContainer;
using VContainer.Unity;

namespace Core.Scopes
{
    public class LevelLifetimeScope : LifetimeScope
    {
        protected override LifetimeScope FindParent() => Find<SessionLifetimeScope>();

        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}
