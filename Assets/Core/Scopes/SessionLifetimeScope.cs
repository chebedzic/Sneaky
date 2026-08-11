using VContainer;
using VContainer.Unity;

namespace Core.Scopes
{
    public class SessionLifetimeScope : LifetimeScope
    {
        protected override LifetimeScope FindParent() => Find<GlobalLifetimeScope>();

        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}
