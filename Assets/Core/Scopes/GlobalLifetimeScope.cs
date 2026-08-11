using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Scopes
{
    public class GlobalLifetimeScope : LifetimeScope
    {
        [SerializeField] private Features.ClientConfig.ClientConfig clientConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(clientConfig);
        }
    }
}
