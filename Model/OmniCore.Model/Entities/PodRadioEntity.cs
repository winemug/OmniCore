namespace OmniCore.Model.Entities
{
    public class PodRadioEntity : Entity
    {
        public PodEntity Pod { get; set; }
        public RadioEntity Radio { get; set; }
    }
}