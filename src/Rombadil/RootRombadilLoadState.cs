namespace Rombadil;

[Root]
public class RootRombadilLoadState(
    RootState state,
    RootScope root,
    RombadilStartupRom startupRom) : State
{
    public override void Load() =>
        root.Scope<RombadilScope>()
            .With(new RombadilRom(startupRom.Bytes))
            .Run(x => x.Scope<RombadilLoaderScope>()
                .Run(x => x.Get<RombadilWindowLoader>().Run())
                .Run(x => x.Get<RombadilRuntimeLoader>().Run()))
            .Run(x => state.Current = x.New<RombadilRunState>());
}
