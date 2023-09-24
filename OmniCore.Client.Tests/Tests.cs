namespace OmniCore.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    // [Test]
    // public async Task Test1()
    // {
    //     var message1 = new MessageBuilder()
    //         .WithSequence(0)
    //         .WithAddress(0x01020304)
    //         .Build(new SetBeepingMessage
    //         {
    //             BeepNow = BeepType.BeepBeep
    //         });
    //     var data1 = (SetBeepingMessage)message1.Data;
    //
    //     var message2 = new MessageBuilder().Build(message1.Body);
    //     var data2 = (SetBeepingMessage)message2.Data;
    //
    //     Assert.That(message1.Sequence, Is.EqualTo(message2.Sequence));
    //     Assert.That(message1.Address, Is.EqualTo(message2.Address));
    //
    //     Assert.That(data1.BeepNow, Is.EqualTo(data2.BeepNow));
    //     Assert.That(data1.BeepNow, Is.EqualTo(data2.BeepNow));
    // }
}