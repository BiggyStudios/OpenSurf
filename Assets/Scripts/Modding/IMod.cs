public interface IMod
{
    string ModID { get; }
    string ModName { get; }
    string Version { get; }

    void Initialize();
    void OnEnabled();
    void OnDisabled();
}
