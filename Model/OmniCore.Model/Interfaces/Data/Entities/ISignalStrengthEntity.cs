namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface ISignalStrengthEntity : ISignalStrengthAttributes, IEntity
    {
        IPodEntity Pod { get; set; }
        IRadioEntity Radio { get; set; }
        IPodRequestEntity Request { get; set; }
    }
}
