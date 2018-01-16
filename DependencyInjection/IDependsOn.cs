namespace UnityUtil.DependencyInjection {

    public interface IDependsOn<T> {

        void Inject(T dependency);

    }

}
