namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IPodRequestEntity : IPodRequestAttributes, IEntity
    {
        IPodEntity Pod { get; set; }
    }
}
