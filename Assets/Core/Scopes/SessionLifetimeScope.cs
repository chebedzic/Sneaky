using VContainer;
using VContainer.Unity;

namespace Core.Scopes
{
    public class SessionLifetimeScope : LifetimeScope
    {
        protected override LifetimeScope FindParent()
        {
            var parent = Find<GlobalLifetimeScope>();
            if (parent != null && parent.Container == null) parent.Build();
            return parent;
        }

        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}
