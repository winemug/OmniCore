using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class MessageParts : IMessageParts
{
    public IMessagePart MainPart { get; set; }
    public IMessagePart? SubPart { get; set; }

    public MessageParts(IMessagePart singlePart)
    {
        MainPart = singlePart;
    }

    public MessageParts(IMessagePart mainPart, IMessagePart subPart)
    {
        MainPart = mainPart;
        SubPart = subPart;
    }
    
    public MessageParts(List<IMessagePart> partsList)
    {
        if (partsList.Count == 1)
        {
            MainPart = partsList[0];
        }
        else if (partsList.Count == 2)
        {
            MainPart = partsList[1];
            SubPart = partsList[0];
        }
        else
        {
            throw new ApplicationException("Invalid part list");
        }
    }

    public List<IMessagePart> AsList()
    {
        var list = new List<IMessagePart>();
        if (SubPart != null)
            list.Add(SubPart);
        list.Add(MainPart);
        return list;
    }
}