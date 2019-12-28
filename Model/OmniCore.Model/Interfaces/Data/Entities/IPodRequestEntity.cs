namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IPodRequestEntity : IPodRequestAttributes, IEntity
    {
        IPodEntity Pod { get; set; }
    }
}
