public interface IServiceLocator<T>
{
    public TP Register<TP>(TP newService) where TP : T;
    public TP Get<TP>() where TP : T;
    public void Unregister<TP>(TP service) where TP : T;
}
