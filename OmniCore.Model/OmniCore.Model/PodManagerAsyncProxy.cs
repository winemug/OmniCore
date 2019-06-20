using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    [Fody.ConfigureAwait(false)]
    public class PodManagerAsyncProxy : IPodManager
    {
        private readonly IPodManager PodManager;
        public PodManagerAsyncProxy(IPodManager podManager)
        {
            PodManager = podManager;
        }

        public IPod Pod => PodManager.Pod;

        public async Task AcknowledgeAlerts(IConversation conversation, byte alertMask)
        {
            await PodManager.AcknowledgeAlerts(conversation, alertMask);
        }

        public async Task Activate(IConversation conversation)
        {
            await PodManager.Activate(conversation);
        }

        public async Task Bolus(IConversation conversation, decimal bolusAmount, bool waitForBolusToFinish = true)
        {
            await PodManager.Bolus(conversation, bolusAmount, waitForBolusToFinish);
        }

        public async Task CancelBolus(IConversation conversation)
        {
            await PodManager.CancelBolus(conversation);
        }

        public async Task CancelExtendedBolus(IConversation conversation)
        {
            await PodManager.CancelExtendedBolus(conversation);
        }

        public async Task CancelTempBasal(IConversation conversation)
        {
            await PodManager.CancelTempBasal(conversation);
        }

        public async Task ConfigureAlerts(IConversation conversation, AlertConfiguration[] alertConfigurations)
        {
            await PodManager.ConfigureAlerts(conversation, alertConfigurations);
        }

        public async Task Deactivate(IConversation conversation)
        {
            await PodManager.Deactivate(conversation);
        }

        public async Task InjectAndStart(IConversation conversation, IProfile profile)
        {
            await PodManager.InjectAndStart(conversation, profile);
        }

        public async Task Pair(IConversation conversation, int utcTimeOffsetMinutes)
        {
            await PodManager.Pair(conversation, utcTimeOffsetMinutes);
        }

        public async Task SetBasalSchedule(IConversation conversation, IProfile profile)
        {
            await PodManager.SetBasalSchedule(conversation, profile);
        }

        public async Task SetTempBasal(IConversation conversation, decimal basalRate, decimal durationInHours)
        {
            await PodManager.SetTempBasal(conversation, basalRate, durationInHours);
        }

        public async Task<IConversation> StartConversation(int timeout = 0, RequestSource source = RequestSource.OmniCoreUser)
        {
            return await PodManager.StartConversation(timeout, source);
        }

        public async Task StartExtendedBolus(IConversation conversation, decimal bolusAmount, decimal durationInHours)
        {
            await PodManager.StartExtendedBolus(conversation, bolusAmount, durationInHours);
        }

        public async Task SuspendBasal(IConversation conversation)
        {
            await PodManager.SuspendBasal(conversation);
        }

        public async Task UpdateStatus(IConversation conversation, StatusRequestType requestType = StatusRequestType.Standard)
        {
            await PodManager.UpdateStatus(conversation, requestType);
        }
    }
}
