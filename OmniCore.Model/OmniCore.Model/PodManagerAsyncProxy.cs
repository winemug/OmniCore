using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    [Fody.ConfigureAwait(false)]
    public class PodManagerAsyncProxy : IPodManager
    {
        public IPodManager Direct { get; }

        public PodManagerAsyncProxy(IPodManager podManager)
        {
            Direct = podManager;
        }

        public IPod Pod => Direct.Pod;

        public async Task AcknowledgeAlerts(IConversation conversation, byte alertMask)
        {
            await Direct.AcknowledgeAlerts(conversation, alertMask);
        }

        public async Task Activate(IConversation conversation)
        {
            await Direct.Activate(conversation);
        }

        public async Task Bolus(IConversation conversation, decimal bolusAmount, bool waitForBolusToFinish = true)
        {
            await Direct.Bolus(conversation, bolusAmount, waitForBolusToFinish);
        }

        public async Task CancelBolus(IConversation conversation)
        {
            await Direct.CancelBolus(conversation);
        }

        public async Task CancelExtendedBolus(IConversation conversation)
        {
            await Direct.CancelExtendedBolus(conversation);
        }

        public async Task CancelTempBasal(IConversation conversation)
        {
            await Direct.CancelTempBasal(conversation);
        }

        public async Task ConfigureAlerts(IConversation conversation, AlertConfiguration[] alertConfigurations)
        {
            await Direct.ConfigureAlerts(conversation, alertConfigurations);
        }

        public async Task Deactivate(IConversation conversation)
        {
            await Direct.Deactivate(conversation);
        }

        public async Task InjectAndStart(IConversation conversation, IProfile profile)
        {
            await Direct.InjectAndStart(conversation, profile);
        }

        public async Task Pair(IConversation conversation, IProfile profile)
        {
            await Direct.Pair(conversation, profile);
        }

        public async Task SetBasalSchedule(IConversation conversation, IProfile profile)
        {
            await Direct.SetBasalSchedule(conversation, profile);
        }

        public async Task SetTempBasal(IConversation conversation, decimal basalRate, decimal durationInHours)
        {
            await Direct.SetTempBasal(conversation, basalRate, durationInHours);
        }

        public async Task<IConversation> StartConversation(string intent, int timeout = 0, RequestSource source = RequestSource.OmniCoreUser)
        {
            return await Direct.StartConversation(intent, timeout, source);
        }

        public async Task StartExtendedBolus(IConversation conversation, decimal bolusAmount, decimal durationInHours)
        {
            await Direct.StartExtendedBolus(conversation, bolusAmount, durationInHours);
        }

        public async Task SuspendBasal(IConversation conversation)
        {
            await Direct.SuspendBasal(conversation);
        }

        public async Task UpdateStatus(IConversation conversation, StatusRequestType requestType = StatusRequestType.Standard, int? timeout = null)
        {
            await Direct.UpdateStatus(conversation, requestType, timeout);
        }
    }
}
