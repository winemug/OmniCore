namespace OmniCore.Common.Pod;

public interface IMessageParts
{
    IMessagePart MainPart { get; set; }
    IMessagePart? SubPart { get; set; }
    List<IMessagePart> AsList();
}