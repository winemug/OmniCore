namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IPodRequestEntity : IPodRequestAttributes, IEntity
    {
        IPodEntity Pod { get; set; }
    }
}
